#Requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$exerciseRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("sequence-goal-inferential-review-$([guid]::NewGuid().ToString('N'))")
New-Item -ItemType Directory -Path $exerciseRoot | Out-Null

try {
    $evidencePath = Join-Path $exerciseRoot 'evidence.json'
    [PSCustomObject]@{
        inferentialReview = [PSCustomObject]@{
            outcome = 'no-findings'
            passes = @(
                [PSCustomObject]@{ name = 'Accessibility'; outcome = 'findings-addressed'; reason = 'Accessibility finding fixed.' }
                [PSCustomObject]@{ name = 'Security and privacy'; outcome = 'no-findings'; reason = 'No security finding.' }
                [PSCustomObject]@{ name = 'Maintainability and architecture'; outcome = 'no-findings'; reason = 'No architecture finding.' }
                [PSCustomObject]@{ name = 'Observability, analytics and diagnosability'; outcome = 'no-findings'; reason = 'No observability finding.' }
                [PSCustomObject]@{ name = 'Testing quality and coverage gaps'; outcome = 'no-findings'; reason = 'No testing finding.' }
                [PSCustomObject]@{ name = 'Behavioural regression and traveller flow correctness'; outcome = 'no-findings'; reason = 'No behavioural finding.' }
            )
        }
    } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $evidencePath -Encoding UTF8

    & (Join-Path $PSScriptRoot 'normalize-sequence-goal-inferential-review.ps1') -EvidencePath $evidencePath
    $normalized = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
    $review = $normalized.inferentialReview
    $expectedNames = @(
        'Security and privacy'
        'Accessibility'
        'Maintainability and architecture'
        'Observability, analytics and diagnosability'
        'Testing quality and coverage gaps'
        'Behavioural regression and traveller flow correctness'
    )
    $expectedOutcomes = @('no-findings', 'findings-addressed', 'no-findings', 'no-findings', 'no-findings', 'no-findings')
    $expectedReasons = @('No security finding.', 'Accessibility finding fixed.', 'No architecture finding.', 'No observability finding.', 'No testing finding.', 'No behavioural finding.')
    $expectedReason = (0..5 | ForEach-Object { "$($expectedNames[$_]): $($expectedReasons[$_])" }) -join ' '
    $details = @($review.passDetails)
    $actualNames = @($review.passes)
    $orderedNamesMatch = $actualNames.Count -eq $expectedNames.Count -and
        (($actualNames -join "`0") -ceq ($expectedNames -join "`0"))
    if (-not $orderedNamesMatch -or
        $review.outcome -ne 'findings-addressed' -or
        $review.reason -ne $expectedReason -or
        $details.Count -ne 6) {
        throw 'Detailed inferential-review evidence was not normalized to the workflow contract.'
    }
    for ($index = 0; $index -lt $details.Count; $index++) {
        if ($details[$index].name -ne $expectedNames[$index] -or
            $details[$index].outcome -ne $expectedOutcomes[$index] -or
            $details[$index].reason -ne $expectedReasons[$index]) {
            throw "Inferential-review pass detail $index was not retained exactly."
        }
    }

    $firstNormalizedJson = ($normalized.inferentialReview | ConvertTo-Json -Depth 20 -Compress)
    & (Join-Path $PSScriptRoot 'normalize-sequence-goal-inferential-review.ps1') -EvidencePath $evidencePath
    $secondNormalized = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
    $secondNormalizedJson = ($secondNormalized.inferentialReview | ConvertTo-Json -Depth 20 -Compress)
    if ($secondNormalizedJson -ne $firstNormalizedJson) {
        throw 'Normalizing already-normalized inferential-review evidence must be idempotent.'
    }

    Write-Host 'sequence-goal inferential-review normalization exercise passed' -ForegroundColor Green
    $global:LASTEXITCODE = 0
    return
}
finally {
    if (Test-Path -LiteralPath $exerciseRoot) {
        Remove-Item -LiteralPath $exerciseRoot -Recurse -Force
    }
}
