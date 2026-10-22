# Inferential (semantic/LLM) review placeholder for a selected repo scope.
param(
    [string]$ScopePath = '.',
    [int]$MaxFiles = 10,
    [int]$MaxCharsPerFile = 12000
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $scriptRoot rev-parse --show-toplevel).Trim())).Path

$candidateScope = if ([System.IO.Path]::IsPathRooted($ScopePath)) {
    $ScopePath
} else {
    Join-Path (Get-Location).Path $ScopePath
}
if (-not (Test-Path -LiteralPath $candidateScope)) {
    $candidateScope = Join-Path $repoRoot $ScopePath
}

$scanRoot = (Resolve-Path -LiteralPath $candidateScope).Path
$resultsDir = Join-Path $scanRoot 'TestResults'

if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

Write-Host "==> Running inferential (semantic/LLM) review for $scanRoot"
# Try to run Copilot CLI if available.
try {
    $ghPath = 'C:\Program Files\GitHub CLI\gh.exe'
    if (-not (Test-Path $ghPath)) {
        $ghCommand = Get-Command 'gh' -ErrorAction SilentlyContinue
        if ($ghCommand) {
            $ghPath = $ghCommand.Source
        }
    }

    if (Test-Path $ghPath) {
        $previousErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        & $ghPath auth status *> $null
        $authExitCode = $LASTEXITCODE
        $ErrorActionPreference = $previousErrorActionPreference
        if ($authExitCode -ne 0) {
            Write-Host '[WARN] GitHub CLI is not authenticated. Run: gh auth login'
            Write-Host '==> Inferential review complete.'
            exit 0
        }

        & $ghPath copilot --help *> $null
        $copilotBuiltInAvailable = ($LASTEXITCODE -eq 0)
        if (-not $copilotBuiltInAvailable) {
            $copilotExt = & $ghPath extension list | Select-String 'copilot'
            if (-not $copilotExt) {
                Write-Host '[WARN] GitHub Copilot command is not available. Install or enable gh copilot support.'
                Write-Host '==> Inferential review complete.'
                exit 0
            }
        }

        Write-Host '[INFO] Running Copilot CLI code review...'
        $csFiles = Get-ChildItem -Path $scanRoot -Recurse -Include *.cs -File |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
            Select-Object -First $MaxFiles

        if (-not $csFiles) {
            Write-Host '[WARN] No C# source files found for inferential review.'
            Write-Host '==> Inferential review complete.'
            exit 0
        }

        $outputFile = Join-Path $resultsDir 'inferential_review_copilot.txt'
        "Inferential Review - $(Get-Date -Format o)" | Out-File -FilePath $outputFile -Encoding utf8

        foreach ($file in $csFiles) {
            Write-Host "[INFO] Reviewing $file"
            $content = Get-Content -Path $file.FullName -Raw
            if ($content.Length -gt $MaxCharsPerFile) {
                $content = $content.Substring(0, $MaxCharsPerFile)
            }

            $prompt = @"
Review this C# file snippet for correctness, reliability, and maintainability risks.
Return concise bullet points with severity labels (High/Medium/Low).

File: $($file.FullName)

$content
"@

            $response = & $ghPath copilot -- -p "$prompt"
            "`n=== $($file.FullName) ===" | Out-File -Append -FilePath $outputFile -Encoding utf8
            $response | Out-File -Append -FilePath $outputFile -Encoding utf8
        }
        Write-Host "[INFO] Copilot CLI review complete. Output: $outputFile"
    } else {
        Write-Host '[WARN] GitHub CLI (gh) not found. Please install GitHub CLI and Copilot CLI extension.'
    }
} catch {
    Write-Host "[ERROR] Copilot CLI review failed: $_"
}

Write-Host '==> Inferential review complete.'
