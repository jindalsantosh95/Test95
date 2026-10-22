#Requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$exerciseRoot = Join-Path ([System.IO.Path]::GetTempPath()) "sequence-goal-associated-pr-recorder-$([guid]::NewGuid().ToString('N'))"
$originalLocation = Get-Location

try {
    New-Item -ItemType Directory -Path $exerciseRoot | Out-Null
    Push-Location $exerciseRoot
    git init --quiet
    if ($LASTEXITCODE -ne 0) { throw 'Failed to initialize the recorder exercise repository.' }

    $diagramDirectory = Join-Path $exerciseRoot 'docs/sequence-diagrams'
    $evidenceDirectory = Join-Path $exerciseRoot 'TestResults/associated-prs'
    New-Item -ItemType Directory -Path $diagramDirectory, $evidenceDirectory -Force | Out-Null
    $diagramPath = Join-Path $diagramDirectory 'exercise.md'
    Set-Content -LiteralPath $diagramPath -Encoding UTF8 -Value @(
        '# Exercise'
        ''
        '## Status'
        ''
        'InProgress'
    )
    [ordered]@{
        surface = 'MobileIos'
        phase = 'Production'
        coordinatorRepository = 'VAAD1ST/eapi'
        coordinatorBranch = 'docs/exercise'
        repository = 'VAAD1ST/vaa-mobile-ios'
        baseBranch = 'develop'
        headBranch = 'feat/exercise'
        number = 123
        url = 'https://github.com/VAAD1ST/vaa-mobile-ios/pull/123'
        state = 'OPEN'
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceDirectory 'MobileIos.json') -Encoding UTF8

    function global:gh {
        [CmdletBinding()]
        param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)
        [ordered]@{
            number = 123
            url = 'https://github.com/VAAD1ST/vaa-mobile-ios/pull/123'
            state = 'OPEN'
            mergedAt = $null
            headRefName = 'feat/exercise'
            baseRefName = 'develop'
            body = @'
Associated sequence-goal delivery for `VAAD1ST/eapi@docs/exercise`.

- Surface: `MobileIos`
- Phase: `Production`
'@
            author = @{ login = 'github-actions[bot]' }
            headRepository = @{ nameWithOwner = 'VAAD1ST/vaa-mobile-ios' }
        } | ConvertTo-Json -Depth 5 -Compress
        $global:LASTEXITCODE = 0
    }

    & (Join-Path $PSScriptRoot 'record-sequence-goal-associated-prs.ps1') `
        -EvidenceDirectory $evidenceDirectory `
        -CoordinatorRepository 'VAAD1ST/eapi' `
        -CoordinatorBranch 'docs/exercise' `
        -GitHubAppSlug 'vaa-codex-pr-review-github-app' `
        -DiagramPaths @('docs/sequence-diagrams/exercise.md')

    $result = Get-Content -LiteralPath $diagramPath -Raw
    if ($result -notmatch '(?m)^\|\s*``?MobileIos``?\s*\|\s*``?VAAD1ST/vaa-mobile-ios``?\s*\|') {
        throw 'Associated-PR recorder did not retain the expected evidence after empty stderr.'
    }
} finally {
    Remove-Item function:global:gh -ErrorAction SilentlyContinue
    Set-Location $originalLocation
    if (Test-Path -LiteralPath $exerciseRoot) { Remove-Item -LiteralPath $exerciseRoot -Recurse -Force }
}

Write-Host 'sequence-goal associated-PR recorder exercise passed' -ForegroundColor Green
