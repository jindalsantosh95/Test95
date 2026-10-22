#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][int]$PrNumber,
    [string]$BaseRef = 'origin/main',
    [Parameter(Mandatory)][string]$EvidencePath,
    [switch]$RetainEvidence
)

$ErrorActionPreference = 'Stop'
$diagramPaths = @(git diff --name-only "$BaseRef...HEAD" -- 'docs/sequence-diagrams/*.md' |
    Where-Object { $_ -notmatch '/_template' })
if ($diagramPaths.Count -ne 1) { throw "Expected exactly one coordinator goal diagram; found $($diagramPaths.Count)." }

$normalizedEvidencePath = $EvidencePath -replace '\\', '/'
if ($normalizedEvidencePath -notmatch "^\.codex/agent-loop/(?:pr-reviews|human-comments)/pr-$PrNumber-[A-Za-z0-9_.-]+\.json$") {
    throw "EvidencePath must identify the current PR's review or human-comment evidence file."
}
$evidenceFiles = @(Get-Item -LiteralPath $EvidencePath -ErrorAction Stop)

$evidenceTotals = [ordered]@{ totalTokens = 0L; inputTokens = 0L; cachedInputTokens = 0L; outputTokens = 0L; reasoningOutputTokens = 0L; estimatedUsd = 0.0 }
foreach ($file in $evidenceFiles) {
    $evidence = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
    $actual = $evidence.resourceUsage.actual
    if ($null -eq $actual) { throw "Evidence $($file.FullName) has no actual resource usage." }
    foreach ($field in @('totalTokens','inputTokens','cachedInputTokens','outputTokens','reasoningOutputTokens')) {
        $value = if ($null -eq $actual.$field) { 0L } else { [long]$actual.$field }
        $evidenceTotals[$field] += $value
    }
    $evidenceTotals.estimatedUsd += if ($null -eq $actual.estimatedUsd) { 0.0 } else { [double]$actual.estimatedUsd }
}

$diagram = $diagramPaths[0]
$text = Get-Content -LiteralPath $diagram -Raw
$batchId = [System.IO.Path]::GetFileNameWithoutExtension((Resolve-Path -LiteralPath $EvidencePath).Path)
$batchMarker = "<!-- sequence-goal-evidence-batch:$batchId -->"
$canonicalBatchPattern = '(?m)^### PR #' + [regex]::Escape([string]$PrNumber) + ' review evidence batch ' + [regex]::Escape($batchId) + '\r?\n\r?\n' +
    [regex]::Escape($batchMarker) + '\r?\n- Archived files: \d+\r?\n- Additional agent resources: [\d,]+ tokens\s*$'
if ([regex]::IsMatch($text, $canonicalBatchPattern)) {
    $repoRoot = (& git rev-parse --show-toplevel).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) { throw 'Cannot authenticate an existing evidence batch outside a Git repository.' }
    $diagramRelative = [IO.Path]::GetRelativePath($repoRoot, (Resolve-Path -LiteralPath $diagram).Path) -replace '\\', '/'
    $headText = @(git show "HEAD:$diagramRelative" 2>$null) -join "`n"
    if ($LASTEXITCODE -ne 0 -or -not [regex]::IsMatch($headText, $canonicalBatchPattern)) {
        throw "Evidence batch $batchId exists only in mutable working-tree content."
    }
    $introducingAuthor = @(git log -1 --format='%an|%ae' -S $batchMarker -- $diagramRelative)
    if ($LASTEXITCODE -ne 0 -or $introducingAuthor.Count -ne 1 -or $introducingAuthor[0] -notmatch '^github-actions\[bot\]\|github-actions\[bot\]@users\.noreply\.github\.com$') {
        throw "Evidence batch $batchId was not introduced by the trusted workflow author."
    }
    Write-Host "Evidence batch $batchId is already applied."
    return
}
if ($text.Contains($batchMarker, [System.StringComparison]::Ordinal)) {
    throw "Evidence batch marker $batchId exists outside a canonical closure batch."
}
$pattern = '(?im)^\|\s*Lifecycle agent resources\s*\|\s*(?<value>.*?)\s*\|\s*$'
$hasLifecycleRow = [regex]::IsMatch($text, $pattern)
$resourcePattern = '^(?<total>[\d,]+) tokens \((?<input>[\d,]+) input, (?<cached>[\d,]+) cached \+ (?<output>[\d,]+) output, (?<reasoning>[\d,]+) reasoning\); \$(?<usd>[\d.]+) USD$'
$currentLifecycle = if ($hasLifecycleRow) { [regex]::Match(([regex]::Match($text, $pattern)).Groups['value'].Value, $resourcePattern) } else { [System.Text.RegularExpressions.Match]::Empty }
if (-not $currentLifecycle.Success) {
    $actualPattern = '(?im)^\|\s*Actual agent resources\s*\|\s*(?<value>.*?)\s*\|\s*$'
    $currentLifecycle = [regex]::Match(([regex]::Match($text, $actualPattern)).Groups['value'].Value, $resourcePattern)
}
$canonicalPlaceholders = @(
    'Recorded automatically at completion from retained JSONL budget evidence',
    'Recorded automatically at PR closure from delivery, AI-review, and human-comment evidence'
)
if (-not $currentLifecycle.Success) {
    $lifecycleValue = if ($hasLifecycleRow) { ([regex]::Match($text, $pattern)).Groups['value'].Value.Trim() } else { '' }
    $actualMatch = [regex]::Match($text, '(?im)^\|\s*Actual agent resources\s*\|\s*(?<value>.*?)\s*\|\s*$')
    $actualValue = if ($actualMatch.Success) { $actualMatch.Groups['value'].Value.Trim() } else { '' }
    $knownPlaceholders = @($lifecycleValue, $actualValue) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    if ($knownPlaceholders.Count -eq 0 -or @($knownPlaceholders | Where-Object { $_ -cnotin $canonicalPlaceholders }).Count -gt 0) {
        throw "Lifecycle and Actual agent resources are malformed in $diagram. Only canonical pre-completion placeholders may be initialized."
    }
    Write-Warning "Initializing lifecycle resources from the canonical pre-completion placeholder in $diagram."
}
$number = { param($value) [long]($value -replace ',', '') }
$existing = if ($currentLifecycle.Success) {
    [ordered]@{
        totalTokens = (& $number $currentLifecycle.Groups['total'].Value)
        inputTokens = (& $number $currentLifecycle.Groups['input'].Value)
        cachedInputTokens = (& $number $currentLifecycle.Groups['cached'].Value)
        outputTokens = (& $number $currentLifecycle.Groups['output'].Value)
        reasoningOutputTokens = (& $number $currentLifecycle.Groups['reasoning'].Value)
        estimatedUsd = [double]$currentLifecycle.Groups['usd'].Value
    }
} else {
    [ordered]@{ totalTokens = 0L; inputTokens = 0L; cachedInputTokens = 0L; outputTokens = 0L; reasoningOutputTokens = 0L; estimatedUsd = 0.0 }
}
$totals = [ordered]@{
    totalTokens = $existing.totalTokens + $evidenceTotals.totalTokens
    inputTokens = $existing.inputTokens + $evidenceTotals.inputTokens
    cachedInputTokens = $existing.cachedInputTokens + $evidenceTotals.cachedInputTokens
    outputTokens = $existing.outputTokens + $evidenceTotals.outputTokens
    reasoningOutputTokens = $existing.reasoningOutputTokens + $evidenceTotals.reasoningOutputTokens
    estimatedUsd = $existing.estimatedUsd + $evidenceTotals.estimatedUsd
}
$usd = [Math]::Ceiling($totals.estimatedUsd * 100 - 1e-9) / 100
$label = '{0:N0} tokens ({1:N0} input, {2:N0} cached + {3:N0} output, {4:N0} reasoning); ${5:F2} USD' -f $totals.totalTokens,$totals.inputTokens,$totals.cachedInputTokens,$totals.outputTokens,$totals.reasoningOutputTokens,$usd
if ($hasLifecycleRow) {
    $text = [regex]::Replace($text, $pattern, "| Lifecycle agent resources | $label |")
} else {
    $actualRowPattern = '(?im)^(\|\s*Actual agent resources\s*\|.*\|\s*)$'
    if (-not [regex]::IsMatch($text, $actualRowPattern)) { throw "Actual agent resources row is missing from $diagram." }
    $text = [regex]::Replace($text, $actualRowPattern, { param($match) "$($match.Value)`n| Lifecycle agent resources | $label |" }, 1)
}
$batch = "### PR #$PrNumber review evidence batch $batchId`n`n$batchMarker`n- Archived files: $($evidenceFiles.Count)`n- Additional agent resources: $($evidenceTotals.totalTokens.ToString('N0')) tokens`n"
if ($text -match '(?m)^## Agent Loop Closure Evidence\s*$') { $text = "$($text.TrimEnd())`n`n$batch`n" }
else { $text = "$($text.TrimEnd())`n`n## Agent Loop Closure Evidence`n`n$batch`n" }
Set-Content -LiteralPath $diagram -Value $text -Encoding UTF8
if (-not $RetainEvidence) { $evidenceFiles | Remove-Item -Force }
