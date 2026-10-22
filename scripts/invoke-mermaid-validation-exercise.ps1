<#
.SYNOPSIS
    Proves sequence punctuation lint and, unless LintOnly, Mermaid rendering.
#>
[CmdletBinding()]
param([switch]$LintOnly)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exerciseRoot = Join-Path $repoRoot ("TestResults/mermaid-validation-exercise-{0}" -f [guid]::NewGuid().ToString('N'))
$validPath = Join-Path $exerciseRoot 'valid.md'
$invalidPath = Join-Path $exerciseRoot 'invalid-semicolon.md'
$nonMarkdownPath = Join-Path $exerciseRoot 'not-markdown.txt'

New-Item -ItemType Directory -Path $exerciseRoot -Force | Out-Null
try {
    @'
```mermaid
sequenceDiagram
    participant GQL
    participant FlightSvc
    Note over GQL,FlightSvc: Implemented aircraft and meal enrichment, Starlink pending
```
'@ | Set-Content -LiteralPath $validPath -Encoding UTF8

    @'
```mermaid
sequenceDiagram
    participant GQL
    participant FlightSvc
    Note over GQL,FlightSvc: Implemented aircraft and meal enrichment; Starlink pending
```
'@ | Set-Content -LiteralPath $invalidPath -Encoding UTF8

    'not a Markdown diagram' | Set-Content -LiteralPath $nonMarkdownPath -Encoding UTF8

    $nonMarkdownRejected = $false
    try {
        & (Join-Path $PSScriptRoot 'check-mermaid-diagrams.ps1') -Path ([IO.Path]::GetRelativePath($repoRoot, $nonMarkdownPath).Replace('\', '/'))
    } catch {
        if ($_.Exception.Message -notmatch 'must identify a Markdown \(\.md\) file') {
            throw
        }
        $nonMarkdownRejected = $true
    }
    if (-not $nonMarkdownRejected) {
        throw 'Mermaid validation exercise did not reject an explicit non-Markdown path.'
    }

    $validRelativePath = [IO.Path]::GetRelativePath($repoRoot, $validPath).Replace('\', '/')
    & (Join-Path $PSScriptRoot 'check-mermaid-diagrams.ps1') -Path $validRelativePath -LintOnly:$LintOnly

    $invalidRejected = $false
    try {
        & (Join-Path $PSScriptRoot 'check-mermaid-diagrams.ps1') -Path ([IO.Path]::GetRelativePath($repoRoot, $invalidPath).Replace('\', '/'))
    } catch {
        if ($_.Exception.Message -notmatch 'invalid-semicolon\.md:5 \(Mermaid block 1\): raw semicolon') {
            throw
        }
        $invalidRejected = $true
        # The rejected Mermaid CLI invocation is expected in this exercise. Clear
        # its native exit code so a successful pwsh step does not inherit failure.
        $global:LASTEXITCODE = 0
    }
    if (-not $invalidRejected) {
        throw 'Mermaid validation exercise did not reject the semicolon-broken sequence-diagram note.'
    }

    # Cover other sequence text positions, multiple blocks and safe punctuation.
    $checker = Join-Path $PSScriptRoot 'check-mermaid-diagrams.ps1'
    foreach ($statement in @('GQL->>FlightSvc: Send; retry', 'participant GQL as Gateway; public', 'alt Ready; eligible')) {
        $fixture = "``````mermaid`nsequenceDiagram`n    $statement`n```````n"
        Set-Content -LiteralPath $invalidPath -Value $fixture -Encoding UTF8
        $rejected = $false
        try { & $checker -Path ([IO.Path]::GetRelativePath($repoRoot, $invalidPath)) -LintOnly }
        catch {
            if ($_.Exception.Message -notmatch 'invalid-semicolon\.md:3 \(Mermaid block 1\): raw semicolon') { throw }
            $rejected = $true
        }
        if (-not $rejected) { throw "Unsafe sequence statement was accepted: $statement" }
    }
    @'
Prose can contain semicolons; it is not Mermaid.
```mermaid
sequenceDiagram
    participant GQL
    participant FlightSvc
    %% Comments can contain semicolons; ignored by the authoring rule
    GQL->>FlightSvc: Send#59; retry<br/>when ready
    Note over GQL,FlightSvc: Safe comma, safe #35; entity
```
```mermaid
flowchart LR
    A[Start] --> B[Finish];
```
'@ | Set-Content -LiteralPath $validPath -Encoding UTF8
    & $checker -Path $validRelativePath -LintOnly:$LintOnly
    Add-Content -LiteralPath $validPath -Value "``````mermaid`nsequenceDiagram`n    Note over GQL: Unsafe; note`n``````"
    $rejected = $false
    try { & $checker -Path $validRelativePath -LintOnly }
    catch {
        if ($_.Exception.Message -notmatch '\(Mermaid block 3\): raw semicolon') { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw 'Unsafe note in a later Mermaid block was accepted.' }

    if (-not $LintOnly) {
        # This has no punctuation violation: the real parser must still reject it.
        @'
```mermaid
sequenceDiagram
    this is not a valid sequence statement
```
'@ | Set-Content -LiteralPath $invalidPath -Encoding UTF8
        $rejected = $false
        try { & $checker -Path ([IO.Path]::GetRelativePath($repoRoot, $invalidPath)) }
        catch {
            if ($_.Exception.Message -notmatch 'invalid-semicolon\.md \(Mermaid block 1\) failed to render') { throw }
            $rejected = $true
            $global:LASTEXITCODE = 0
        }
        if (-not $rejected) { throw 'Mermaid renderer accepted a malformed sequence statement.' }
    }

    if ($LintOnly) {
        Write-Host 'Mermaid authoring exercise passed: unsafe sequence punctuation rejected, safe entities/comments/prose/flowcharts accepted. Rendering was not run.'
    } else {
        Write-Host 'Mermaid validation exercise passed: safe syntax rendered and semicolon regressions rejected.'
    }
} finally {
    if (Test-Path -LiteralPath $exerciseRoot -PathType Container) {
        Remove-Item -LiteralPath $exerciseRoot -Recurse -Force
    }
}
