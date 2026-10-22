#Requires -Version 5.1
<#
.SYNOPSIS
    Harness verification for VAA.EAPI.Automation - the local maintainability sensor.

.DESCRIPTION
    Runs the harness checks an agent or contributor should pass before pushing:

      1. dotnet restore + build (Release) - the analyzer ratchet. Any NEW warning code (one
         not frozen in docs/warnings-baseline.md) fails the build.
      2. The offline test suites - architecture fitness and Core behavior.
      3. dotnet format --verify-no-changes, scoped to .cs files NEWLY ADDED since
         the base branch - the formatting sensor for new files.

    See docs/HARNESS_ENGINEERING.md (Thread 2). Exit code is 0 on pass, 1 on fail,
    so the script is usable from CI and git hooks.

.PARAMETER Base
    Branch/ref to diff against for the format check. Default: main.

.PARAMETER SkipBuild
    Skip the build and the offline test suites (format check only) - a fast run.

.PARAMETER InstallHooks
    Point this repo's core.hooksPath at VAA.EAPI.Automation/.githooks and exit.

.EXAMPLE
    ./scripts/verify.ps1

.EXAMPLE
    ./scripts/verify.ps1 -InstallHooks
#>
[CmdletBinding()]
param(
    [string]$Base = 'main',
    [switch]$SkipBuild,
    [switch]$InstallHooks
)

$ErrorActionPreference = 'Stop'
$solutionDir = Split-Path -Parent $PSScriptRoot
$solution    = Join-Path $solutionDir 'VAA.EAPI.Automation.sln'

# Wrap a native-command invocation so its stderr does NOT get re-thrown as a
# PowerShell terminating error. PowerShell 5.1 under `$ErrorActionPreference =
# 'Stop'` wraps every native stderr line in a NativeCommandError and tears the
# script down — even when the command itself returned exit code 0. Two real
# offenders fire on every run:
#   - `git diff` emits "LF will be replaced by CRLF the next time Git touches
#     it" warnings whenever the working tree has a *.cs file with LF endings;
#   - `dotnet format` emits "Warnings were encountered while loading the
#     workspace" on any solution with legacy NETFX targets (Microsoft.Web.Services3,
#     MicrosoftOfficeCore, etc. — permanent backlog for this solution).
# Both are benign. Use $LASTEXITCODE as the source of truth instead.
function Invoke-Native {
    param([scriptblock]$Block)
    $prev = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Block } finally { $ErrorActionPreference = $prev }
}

# Pipe through Out-String so we always have a string to .Trim() — even when
# git produces no stdout (the common case when it fails, e.g. script run
# outside a worktree). Without Out-String, `(Invoke-Native { ... }).Trim()`
# would NRE on $null before the $LASTEXITCODE / empty-string check ran, and
# PS 5.1 does NOT silently coerce $null to '' for method calls. `$LASTEXITCODE`
# is preserved across Out-String (it's a cmdlet, not a native call) so the
# explicit check below still sees git's real exit code.
$repoRoot = (Invoke-Native { & git -C $solutionDir rev-parse --show-toplevel } | Out-String).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrEmpty($repoRoot)) {
    Write-Host "verify.ps1: 'git rev-parse --show-toplevel' failed in '$solutionDir' (exit $LASTEXITCODE). Are you running this from inside a git working tree?" -ForegroundColor Red
    exit 1
}

$solutionDirFull = (Resolve-Path -LiteralPath $solutionDir).Path
$repoRootFull    = (Resolve-Path -LiteralPath $repoRoot).Path
if ($repoRootFull -ne $solutionDirFull) {
    $nestedGit = Join-Path $solutionDirFull '.git'
    if (Test-Path -LiteralPath $nestedGit) {
        Write-Host "verify.ps1: Found a nested '.git' at '$nestedGit'." -ForegroundColor Red
        Write-Host "This causes Visual Studio to detect multiple repositories for this solution." -ForegroundColor Red
        Write-Host "Fix: delete or rename '$nestedGit'. The git repo root for this tree is '$repoRootFull'." -ForegroundColor Yellow
        exit 1
    }
}

if ($InstallHooks) {
    & git -C $repoRoot config core.hooksPath 'VAA.EAPI.Automation/.githooks'
    Write-Host "core.hooksPath set to VAA.EAPI.Automation/.githooks" -ForegroundColor Green
    exit 0
}

# Resolve the base ref; fall back gracefully if it is not present locally.
$baseRef = $Base
& git -C $repoRoot rev-parse --verify --quiet "$Base" *> $null
if ($LASTEXITCODE -ne 0) {
    & git -C $repoRoot rev-parse --verify --quiet "origin/$Base" *> $null
    if ($LASTEXITCODE -eq 0) {
        $baseRef = "origin/$Base"
    } else {
        Write-Host "base ref '$Base' not found - comparing format against HEAD only" -ForegroundColor Yellow
        $baseRef = 'HEAD'
    }
}

$failed = @()

if (-not $SkipBuild) {
    Write-Host ''
    Write-Host '==> Restore' -ForegroundColor Cyan
    & dotnet restore $solution --nologo
    if ($LASTEXITCODE -ne 0) {
        $failed += 'restore'
    }
    else {
        Write-Host ''
        Write-Host '==> Build - analyzer ratchet' -ForegroundColor Cyan
        & dotnet build $solution -c Release --no-restore --nologo
        if ($LASTEXITCODE -ne 0) {
            $failed += 'build'
        }
        else {
            Write-Host ''
            Write-Host '==> Offline tests - architecture fitness + Core behavior' -ForegroundColor Cyan
            $offlineTestProjects = @(
                'VAA.EAPI.Automation.ArchitectureTests'
                'VAA.EAPI.Automation.Core.Tests'
            )
            $runName = "linux-portability-$([DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))-$PID"
            $resultsRoot = Join-Path $solutionDir "TestResults/$runName"
            foreach ($testProject in $offlineTestProjects) {
                $results = Join-Path $resultsRoot $testProject
                & dotnet test (Join-Path $solutionDir "$testProject/$testProject.csproj") -c Release --no-build --nologo --results-directory $results --logger "trx;LogFileName=offline.trx"
                if ($LASTEXITCODE -ne 0) { $failed += "tests:$testProject" }
                $trx = Join-Path $results 'offline.trx'
                if (Test-Path -LiteralPath $trx) {
                    try {
                        & (Join-Path $repoRoot 'scripts/analyse-test-run-results.ps1') -TrxPath $trx -SuiteName $testProject -EnvironmentName 'offline'
                    } catch {
                        $failed += "analysis:$testProject"
                        Write-Warning "TRX analysis failed for ${testProject}: $($_.Exception.Message)"
                    }
                } else {
                    $failed += "missing-trx:$testProject"
                }
            }
        }
    }
}

Write-Host ''
Write-Host "==> Format - new .cs files vs '$baseRef'" -ForegroundColor Cyan
# Only NEW .cs files are gated: a file born under the harness must be clean.
# Touching a legacy file must not force a whole-file reformat - the one-time
# normalisation of the existing tree is a separate pass.
# git runs at the monorepo root, which holds far more than this solution; keep
$newFiles = @(Invoke-Native { & git -C $repoRoot diff $baseRef --name-only --diff-filter=A -- '*.cs' } |
              Where-Object { $_ -like 'VAA.EAPI.Automation/*' })
# Invoke-Native preserves $LASTEXITCODE from the inner native command. If
# git diff failed (corrupt index, bad ref, etc.), the pipeline may have
# produced empty or partial output that we must NOT treat as "no new files
# - skip the format check" — that would silently bypass the gate.
$gitDiffExit = $LASTEXITCODE
if ($gitDiffExit -ne 0) {
    Write-Host "git diff failed (exit $gitDiffExit) - cannot enumerate new .cs files; failing format check." -ForegroundColor Red
    $failed += 'format'
} elseif ($newFiles.Count -eq 0) {
    Write-Host 'no new .cs files - format check skipped' -ForegroundColor DarkGray
} else {
    # 'dotnet format --include' only matches paths relative to the working
    # directory, so run it from the solution dir with solution-relative paths.
    Push-Location $solutionDir
    try {
        $rel = $newFiles | ForEach-Object {
            $abs = Join-Path $repoRoot $_
            (Resolve-Path -LiteralPath $abs -Relative) -replace '^\.[\\/]', ''
        }
        Invoke-Native { & dotnet format $solution --no-restore --include $rel --verify-no-changes }
        $fmtExit = $LASTEXITCODE
    } finally {
        Pop-Location
    }
    if ($fmtExit -ne 0) {
        $failed += 'format'
        Write-Host "format check failed - run 'dotnet format' on the new files" -ForegroundColor Yellow
    } else {
        Write-Host "$($newFiles.Count) new .cs file(s) OK" -ForegroundColor Green
    }
}

Write-Host ''
if ($failed.Count -gt 0) {
    Write-Host "VERIFY FAILED: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host 'VERIFY PASSED' -ForegroundColor Green
exit 0
