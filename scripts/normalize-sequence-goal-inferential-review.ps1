#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string]$EvidencePath
)

$ErrorActionPreference = 'Stop'
$evidence = Get-Content -LiteralPath $EvidencePath -Raw -Encoding UTF8 | ConvertFrom-Json
$review = $evidence.inferentialReview
if ($null -eq $review) {
    return
}

$requiredPasses = @(
    'Security and privacy'
    'Accessibility'
    'Maintainability and architecture'
    'Observability, analytics and diagnosability'
    'Testing quality and coverage gaps'
    'Behavioural regression and traveller flow correctness'
)
$passes = @($review.passes)
$stringPasses = @($passes | Where-Object { $_ -is [string] })
if ($passes.Count -eq 0) { throw 'Inferential-review passes must use the detailed per-pass evidence shape.' }
if ($stringPasses.Count -gt 0) {
    if ($stringPasses.Count -ne $passes.Count) {
        throw 'Inferential-review passes cannot mix normalized strings with detailed objects.'
    }
    $isCanonicalNormalizedList = $stringPasses.Count -eq $requiredPasses.Count -and
        (($stringPasses -join "`0") -ceq ($requiredPasses -join "`0"))
    $passDetails = @($review.passDetails)
    if (-not $isCanonicalNormalizedList -or $passDetails.Count -eq 0) {
        throw 'String-valued inferential-review passes are accepted only for already-normalized evidence with canonical passDetails.'
    }
    $passes = $passDetails
}

$allowedOutcomes = @('no-findings', 'findings-addressed', 'not-applicable')
$detailsByName = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
$passes | ForEach-Object {
    if ($_.name -isnot [string] -or
        $_.outcome -isnot [string] -or
        $_.reason -isnot [string] -or
        [string]::IsNullOrWhiteSpace($_.name) -or
        $_.outcome -notin $allowedOutcomes -or
        [string]::IsNullOrWhiteSpace($_.reason)) {
        throw 'Inferential-review pass details must contain a name, a supported outcome, and a non-empty reason.'
    }

    if ($detailsByName.ContainsKey($_.name)) {
        throw "Inferential-review pass details contain a duplicate or non-canonical pass: $($_.name)"
    }
    $detailsByName[$_.name] = $_
}
if ($detailsByName.Count -ne $requiredPasses.Count -or @($requiredPasses | Where-Object { -not $detailsByName.ContainsKey($_) }).Count -gt 0) {
    throw 'Inferential-review pass details must contain each canonical pass exactly once.'
}
$details = @($requiredPasses | ForEach-Object { $detailsByName[$_] })

$review | Add-Member -NotePropertyName passDetails -NotePropertyValue $details -Force
$review.passes = @($details | ForEach-Object { [string]$_.name })

$detailOutcomes = @($details | ForEach-Object { [string]$_.outcome })
$derivedOutcome = if ($detailOutcomes -contains 'findings-addressed') {
    'findings-addressed'
}
elseif (@($detailOutcomes | Where-Object { $_ -ne 'not-applicable' }).Count -eq 0) {
    'not-applicable'
}
else {
    'no-findings'
}
$review | Add-Member -NotePropertyName outcome -NotePropertyValue $derivedOutcome -Force

$review | Add-Member -NotePropertyName reason -NotePropertyValue (($details | ForEach-Object {
    "$($_.name): $($_.reason)"
}) -join ' ') -Force

$evidence | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $EvidencePath -Encoding UTF8
