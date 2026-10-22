#Requires -Version 5.1
<#
.SYNOPSIS
    Generic repository harness verification.

.DESCRIPTION
    Runs local harness checks for a solution or project:
      1. dotnet build (Release) unless -SkipBuild is supplied
      2. test projects discovered under the selected scope, or explicit -TestProjectPath values,
         with run-scoped TRX, coverage, and analyser output
      3. optional deployment project coverage for Azure Functions projects in a solution folder
      4. repository knowledge health checks
      5. repository harness rule checks
      6. repository agent-memory contract and optional local-state checks
      7. repository context-routing checks
      8. repository docs harness support checks
      9. sequence-diagram status consistency checks
      10. preferred shared-extension usage on newly added C# lines
      11. dotnet format --verify-no-changes on newly added .cs files under the selected scope

    Pass -SolutionPath or -ProjectPath to target a specific workspace. If neither is
    supplied, the script uses the current directory when it contains exactly one .sln
    or .csproj. In fast mode, format workspaces can also be inferred from new .cs files.

.PARAMETER Base
    Base branch/ref to diff against for new-file format checks. Default: main.

.PARAMETER SolutionPath
    Solution file to build/test/format.

.PARAMETER ProjectPath
    Project file to build/test/format.

.PARAMETER ScopePath
    Directory scope used for test discovery and new-file format filtering.

.PARAMETER TestProjectPath
    Optional explicit test project paths. When omitted, *.Tests.csproj files are discovered under the scope.

.PARAMETER SkipBuild
    Skip build/tests and run lightweight harness checks only.

.PARAMETER SkipDeploymentCheck
    Skip deployment project coverage checks.

.PARAMETER SkipKnowledgeHealthCheck
    Skip repository knowledge health checks.

.PARAMETER SkipHarnessRuleCheck
    Skip repository harness rule checks.

.PARAMETER SkipAgentMemoryCheck
    Skip repository agent-memory contract and optional local-state checks.

.PARAMETER SkipContextRoutingCheck
    Skip repository context-routing checks.

.PARAMETER SkipDocsHarnessSupportCheck
    Skip repository docs harness support checks.

.PARAMETER SkipSequenceDiagramStatusCheck
    Skip sequence-diagram status consistency checks.

.PARAMETER InstallHooks
    Set core.hooksPath to the root .githooks folder and exit.
#>
[CmdletBinding()]
param(
    [string]$Base = 'main',
    [string]$SolutionPath,
    [string]$ProjectPath,
    [string]$ScopePath,
    [string[]]$TestProjectPath,
    [switch]$SkipBuild,
    [switch]$SkipDeploymentCheck,
    [switch]$SkipKnowledgeHealthCheck,
    [switch]$SkipHarnessRuleCheck,
    [switch]$SkipAgentMemoryCheck,
    [switch]$SkipContextRoutingCheck,
    [switch]$SkipDocsHarnessSupportCheck,
    [switch]$SkipSequenceDiagramStatusCheck,
    [switch]$InstallHooks
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path

if ($InstallHooks) {
    & git -C $repoRoot config core.hooksPath '.githooks'
    Write-Host 'core.hooksPath set to .githooks' -ForegroundColor Green
    exit 0
}

function Resolve-RepoInputPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }

    $candidates = @()
    if ([System.IO.Path]::IsPathRooted($Path)) {
        $candidates += $Path
    } else {
        $candidates += (Join-Path (Get-Location).Path $Path)
        $candidates += (Join-Path $repoRoot $Path)
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "path not found: $Path"
}

function Resolve-DefaultWorkspace {
    $current = (Get-Location).Path
    $solutions = @(Get-ChildItem -LiteralPath $current -Filter '*.sln' -File -ErrorAction SilentlyContinue)
    if ($solutions.Count -eq 1) {
        return $solutions[0].FullName
    }

    $projects = @(Get-ChildItem -LiteralPath $current -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
    if ($projects.Count -eq 1) {
        return $projects[0].FullName
    }

    return $null
}

function Get-WorkspaceDirectory {
    param([Parameter(Mandatory)][string]$WorkspacePath)

    if (Test-Path -LiteralPath $WorkspacePath -PathType Leaf) {
        return (Split-Path -Parent $WorkspacePath)
    }

    return $WorkspacePath
}

function Resolve-WorkspaceForRepoFile {
    param([Parameter(Mandatory)][string]$RepoRelativePath)

    $absolutePath = Join-Path $repoRoot $RepoRelativePath
    if (-not (Test-Path -LiteralPath $absolutePath)) {
        return $null
    }

    $dir = Split-Path -Parent $absolutePath
    while (-not [string]::IsNullOrWhiteSpace($dir) -and $dir.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $projects = @(Get-ChildItem -LiteralPath $dir -Filter '*.csproj' -File -ErrorAction SilentlyContinue)
        if ($projects.Count -eq 1) {
            return $projects[0].FullName
        }

        $solutions = @(Get-ChildItem -LiteralPath $dir -Filter '*.sln' -File -ErrorAction SilentlyContinue)
        if ($solutions.Count -eq 1) {
            return $solutions[0].FullName
        }

        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) {
            break
        }

        $dir = $parent
    }

    return $null
}

function Test-PathUnderScope {
    param(
        [Parameter(Mandatory)][string]$RepoRelativePath,
        [Parameter(Mandatory)][string]$ScopeRoot
    )

    $absolutePath = Join-Path $repoRoot $RepoRelativePath
    return $absolutePath.StartsWith($ScopeRoot, [System.StringComparison]::OrdinalIgnoreCase)
}

function Invoke-DotnetFormat {
    param(
        [Parameter(Mandatory)][string]$WorkspacePath,
        [Parameter(Mandatory)][string[]]$RepoRelativeFiles
    )

    $workspaceDir = Get-WorkspaceDirectory -WorkspacePath $WorkspacePath
    Push-Location $workspaceDir
    try {
        $relative = @(
            $RepoRelativeFiles |
            ForEach-Object {
                $absolutePath = Join-Path $repoRoot $_
                (Resolve-Path -LiteralPath $absolutePath -Relative) -replace '^\.[\\/]', ''
            }
        )

        if ($relative.Count -eq 0) {
            return 0
        }

        & dotnet format $WorkspacePath --include $relative --verify-no-changes
        return $LASTEXITCODE
    } finally {
        Pop-Location
    }
}

if (-not [string]::IsNullOrWhiteSpace($SolutionPath) -and -not [string]::IsNullOrWhiteSpace($ProjectPath)) {
    throw 'Provide either -SolutionPath or -ProjectPath, not both.'
}

$workspace = $null
if (-not [string]::IsNullOrWhiteSpace($SolutionPath)) {
    $workspace = Resolve-RepoInputPath -Path $SolutionPath
} elseif (-not [string]::IsNullOrWhiteSpace($ProjectPath)) {
    $workspace = Resolve-RepoInputPath -Path $ProjectPath
} else {
    $workspace = Resolve-DefaultWorkspace
}

$scopeRoot = $null
if (-not [string]::IsNullOrWhiteSpace($ScopePath)) {
    $scopeRoot = Resolve-RepoInputPath -Path $ScopePath
} elseif (-not [string]::IsNullOrWhiteSpace($workspace)) {
    $scopeRoot = Get-WorkspaceDirectory -WorkspacePath $workspace
} else {
    $scopeRoot = (Get-Location).Path
}

$baseRef = $Base
& git -C $repoRoot rev-parse --verify --quiet "$Base" *> $null
if ($LASTEXITCODE -ne 0) {
    & git -C $repoRoot rev-parse --verify --quiet "origin/$Base" *> $null
    if ($LASTEXITCODE -eq 0) {
        $baseRef = "origin/$Base"
    } else {
        Write-Host "base ref '$Base' not found; comparing format against HEAD only" -ForegroundColor Yellow
        $baseRef = 'HEAD'
    }
}

$failed = @()

if (-not [string]::IsNullOrWhiteSpace($workspace)) {
    Write-Host "Workspace: $workspace" -ForegroundColor DarkGray
}
Write-Host "Scope: $scopeRoot" -ForegroundColor DarkGray

if (-not $SkipBuild) {
    if ([string]::IsNullOrWhiteSpace($workspace)) {
        throw 'No solution or project could be resolved. Run from a folder containing exactly one .sln/.csproj, or pass -SolutionPath / -ProjectPath.'
    }

    Write-Host ''
    Write-Host '==> Build (Release)' -ForegroundColor Cyan
    & dotnet build $workspace -c Release --nologo
    if ($LASTEXITCODE -ne 0) {
        $failed += 'build'
    } else {
        $testProjects = @()
        if ($TestProjectPath -and $TestProjectPath.Count -gt 0) {
            $testProjects = @($TestProjectPath | ForEach-Object { Resolve-RepoInputPath -Path $_ })
        } else {
            $workspaceName = [System.IO.Path]::GetFileName($workspace)
            if ($workspaceName -like '*.Tests.csproj') {
                $testProjects = @($workspace)
            } else {
                $testProjects = @(
                    Get-ChildItem -LiteralPath $scopeRoot -Recurse -Filter '*.Tests.csproj' -File -ErrorAction SilentlyContinue |
                    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
                    ForEach-Object { $_.FullName } |
                    Sort-Object -Unique
                )
            }
        }

        $testRunId = "verify-$((Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssfffZ'))"
        $testResultsRoot = Join-Path $repoRoot "TestResults/$testRunId"
        $testResultsAnalyser = Join-Path $PSScriptRoot 'analyse-test-run-results.ps1'
        $isAutomationSolution = [System.IO.Path]::GetFileName($workspace) -eq 'VAA.EAPI.Automation.sln'

        Write-Host ''
        Write-Host '==> Test projects' -ForegroundColor Cyan
        if ($testProjects.Count -eq 0) {
            Write-Host 'no test projects discovered; test step skipped' -ForegroundColor DarkGray
        } else {
            foreach ($testProject in $testProjects) {
                $testProjectName = [System.IO.Path]::GetFileNameWithoutExtension($testProject)
                $safeProjectName = $testProjectName -replace '[^A-Za-z0-9_.-]', '-'
                $projectResultsDirectory = Join-Path $testResultsRoot $safeProjectName
                $trxFileName = "$safeProjectName.trx"
                $trxPath = Join-Path $projectResultsDirectory $trxFileName
                New-Item -ItemType Directory -Path $projectResultsDirectory -Force | Out-Null

                $testProjectText = Get-Content -LiteralPath $testProject -Raw
                $usesMicrosoftTestingPlatform = $testProjectText -match '(?is)<TestingPlatformDotnetTestSupport>\s*true\s*</TestingPlatformDotnetTestSupport>' -or
                    $testProjectText -match '(?is)<UseMicrosoftTestingPlatformRunner>\s*true\s*</UseMicrosoftTestingPlatformRunner>'

                $testArguments = @('test', $testProject, '-c', 'Release', '--no-restore', '--nologo', '--results-directory', $projectResultsDirectory)
                if ($usesMicrosoftTestingPlatform) {
                    $testArguments += @('--', '--report-trx', '--report-trx-filename', $trxFileName)
                    if (-not $isAutomationSolution) {
                        $testArguments += '--coverage'
                    }
                } else {
                    $testArguments += @('--logger', "trx;LogFileName=$trxFileName")
                    if (-not $isAutomationSolution) {
                        $testArguments += '--collect:XPlat Code Coverage'
                    }
                }

                & dotnet @testArguments
                $testExitCode = $LASTEXITCODE
                if ($testExitCode -ne 0) { $failed += "tests:$testProjectName" }

                if (-not (Test-Path -LiteralPath $trxPath -PathType Leaf)) {
                    Write-Host "expected TRX was not produced: $trxPath" -ForegroundColor Yellow
                    $failed += "test-results:$testProjectName"
                    continue
                }

                try {
                    & $testResultsAnalyser -TrxPath $trxPath -SuiteName $testProjectName -EnvironmentName 'local'
                    if ($LASTEXITCODE -ne 0) { $failed += "test-analysis:$testProjectName" }
                } catch {
                    $failed += "test-analysis:$testProjectName"
                    Write-Host "test result analysis failed for $testProjectName`: $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }

            Write-Host "test evidence: $testResultsRoot" -ForegroundColor DarkGray
        }
    }
}

if (-not $SkipDeploymentCheck -and -not [string]::IsNullOrWhiteSpace($workspace) -and [System.IO.Path]::GetExtension($workspace) -eq '.sln') {
    Write-Host ''
    Write-Host '==> Deployment project coverage' -ForegroundColor Cyan
    $deploymentCheck = Join-Path $PSScriptRoot 'check-deployment-projects.ps1'
    try {
        & (Join-Path $PSScriptRoot 'invoke-deployment-projects-exercise.ps1')
        & $deploymentCheck -SolutionPath $workspace
    } catch {
        $failed += 'deployment-projects'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

if (-not $SkipKnowledgeHealthCheck) {
    Write-Host ''
    Write-Host '==> Knowledge health' -ForegroundColor Cyan
    $knowledgeCheck = Join-Path $PSScriptRoot 'check-knowledge-health.ps1'
    try {
        & $knowledgeCheck
        if ($LASTEXITCODE -ne 0) {
            $failed += 'knowledge-health'
        }
    } catch {
        $failed += 'knowledge-health'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

if (-not $SkipHarnessRuleCheck) {
    try {
        & (Join-Path $PSScriptRoot 'invoke-sequence-goal-registration-exercise.ps1')
        if ($LASTEXITCODE -ne 0) { $failed += 'sequence-goal-registration' }
    } catch {
        $failed += 'sequence-goal-registration'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
    try {
        & (Join-Path $PSScriptRoot 'invoke-sequence-goal-production-readiness-exercise.ps1')
        if ($LASTEXITCODE -ne 0) { $failed += 'sequence-goal-production-readiness' }
        & (Join-Path $PSScriptRoot 'invoke-sequence-goal-session-exercise.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'Sequence-goal session continuation exercise failed.' }
    } catch {
        $failed += 'sequence-goal-production-readiness'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
    try {
        & (Join-Path $PSScriptRoot 'invoke-sequence-goal-production-recovery-exercise.ps1')
        if ($LASTEXITCODE -ne 0) { $failed += 'sequence-goal-production-recovery' }
    } catch {
        $failed += 'sequence-goal-production-recovery'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
    Write-Host ''
    Write-Host '==> Sequence-goal associated-PR recorder exercise' -ForegroundColor Cyan
    $associatedPrRecorderExercise = Join-Path $PSScriptRoot 'invoke-sequence-goal-associated-pr-recorder-exercise.ps1'
    try {
        & $associatedPrRecorderExercise
        if ($LASTEXITCODE -ne 0) { $failed += 'sequence-goal-associated-pr-recorder' }
    } catch {
        $failed += 'sequence-goal-associated-pr-recorder'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }

    Write-Host ''
    Write-Host '==> Sequence-goal inferential-review normalization exercise' -ForegroundColor Cyan
    $inferentialReviewExercise = Join-Path $PSScriptRoot 'invoke-sequence-goal-inferential-review-normalization-exercise.ps1'
    try {
        & $inferentialReviewExercise
        if ($LASTEXITCODE -ne 0) { $failed += 'sequence-goal-inferential-review-normalization' }
    } catch {
        $failed += 'sequence-goal-inferential-review-normalization'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }

    Write-Host ''
    Write-Host '==> Harness rules' -ForegroundColor Cyan
    $harnessRulesCheck = Join-Path $PSScriptRoot 'check-harness-rules.ps1'
    try {
        & $harnessRulesCheck
        if ($LASTEXITCODE -ne 0) {
            $failed += 'harness-rules'
        }
    } catch {
        $failed += 'harness-rules'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

if (-not $SkipAgentMemoryCheck) {
    Write-Host ''
    Write-Host '==> Agent memory contract/local state' -ForegroundColor Cyan
    $agentMemoryCheck = Join-Path $PSScriptRoot 'check-agent-memory-state.ps1'
    try {
        & $agentMemoryCheck
        if ($LASTEXITCODE -ne 0) {
            $failed += 'agent-memory-state'
        }
    } catch {
        $failed += 'agent-memory-state'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }

    Write-Host ''
    Write-Host '==> Agent memory lifecycle and cross-process resume exercise' -ForegroundColor Cyan
    $agentMemoryExercise = Join-Path $PSScriptRoot 'invoke-agent-memory-lifecycle-exercise.ps1'
    try {
        & $agentMemoryExercise
        if ($LASTEXITCODE -ne 0) { $failed += 'agent-memory-lifecycle' }
    } catch {
        $failed += 'agent-memory-lifecycle'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

if (-not $SkipContextRoutingCheck) {
    Write-Host ''
    Write-Host '==> Context routing' -ForegroundColor Cyan
    $contextRoutingCheck = Join-Path $PSScriptRoot 'check-context-routing.ps1'
    try {
        & $contextRoutingCheck
        if ($LASTEXITCODE -ne 0) {
            $failed += 'context-routing'
        }
    } catch {
        $failed += 'context-routing'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

if (-not $SkipDocsHarnessSupportCheck) {
    Write-Host ''
    Write-Host '==> Docs harness support' -ForegroundColor Cyan
    $docsHarnessSupportCheck = Join-Path $PSScriptRoot 'check-docs-harness-support.ps1'
    try {
        & $docsHarnessSupportCheck
        if ($LASTEXITCODE -ne 0) {
            $failed += 'docs-harness-support'
        }
    } catch {
        $failed += 'docs-harness-support'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

Write-Host ''
Write-Host '==> Operational harness registry' -ForegroundColor Cyan
$operationalHarnessCheck = Join-Path $PSScriptRoot 'check-operational-harness.ps1'
try {
    & $operationalHarnessCheck
    if ($LASTEXITCODE -ne 0) {
        $failed += 'operational-harness'
    }
} catch {
    $failed += 'operational-harness'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Agent workflow execution controls' -ForegroundColor Cyan
$agentWorkflowControlsCheck = Join-Path $PSScriptRoot 'check-agent-workflow-controls.ps1'
try {
    & $agentWorkflowControlsCheck
    if ($LASTEXITCODE -ne 0) {
        $failed += 'agent-workflow-controls'
    }
} catch {
    $failed += 'agent-workflow-controls'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> UI automation architecture' -ForegroundColor Cyan
try {
    & (Join-Path $PSScriptRoot 'check-ui-automation-standards.ps1') -BaseRef $baseRef -ScopePath 'VAA.EAPI.Automation'
    if ($LASTEXITCODE -ne 0) { $failed += 'ui-automation-standards' }
} catch {
    $failed += 'ui-automation-standards'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Agent tool integration and executable environment' -ForegroundColor Cyan
$agentToolEnvironmentCheck = Join-Path $PSScriptRoot 'check-agent-tool-environment.ps1'
$agentToolEnvironmentExercise = Join-Path $PSScriptRoot 'invoke-agent-tool-environment-exercise.ps1'
try {
    & $agentToolEnvironmentCheck -Profile docs-local -Probe
    if ($LASTEXITCODE -ne 0) { $failed += 'agent-tool-environment' }
    & $agentToolEnvironmentExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'agent-tool-environment-strength' }
} catch {
    $failed += 'agent-tool-environment'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Agent resource and cost budget exercise' -ForegroundColor Cyan
$agentResourceBudgetExercise = Join-Path $PSScriptRoot 'invoke-agent-resource-budget-exercise.ps1'
try {
    & $agentResourceBudgetExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'agent-resource-budget' }
} catch {
    $failed += 'agent-resource-budget'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Agent-readable observability and runtime learning' -ForegroundColor Cyan
$agentObservabilityCheck = Join-Path $PSScriptRoot 'check-agent-observability.ps1'
$agentObservabilityExercise = Join-Path $PSScriptRoot 'invoke-agent-observability-exercise.ps1'
try {
    & $agentObservabilityCheck
    if ($LASTEXITCODE -ne 0) { $failed += 'agent-observability' }
    & $agentObservabilityExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'agent-observability-strength' }
} catch {
    $failed += 'agent-observability'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Continuous harness maturity evidence' -ForegroundColor Cyan
$harnessMaturityCheck = Join-Path $PSScriptRoot 'check-harness-maturity-evidence.ps1'
$harnessMaturityExercise = Join-Path $PSScriptRoot 'invoke-harness-maturity-evidence-exercise.ps1'
try {
    & $harnessMaturityCheck
    if ($LASTEXITCODE -ne 0) { $failed += 'harness-maturity-policy' }
    & $harnessMaturityExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'harness-maturity-evidence' }
} catch {
    $failed += 'harness-maturity-evidence'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Agent recovery and diagnostic replay exercise' -ForegroundColor Cyan
$agentRecoveryExercise = Join-Path $PSScriptRoot 'invoke-agent-recovery-exercise.ps1'
try {
    & $agentRecoveryExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'agent-recovery' }
} catch {
    $failed += 'agent-recovery'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> General self-correcting agent loop exercise' -ForegroundColor Cyan
$selfCorrectingLoopExercise = Join-Path $PSScriptRoot 'invoke-self-correcting-loop-exercise.ps1'
try {
    & $selfCorrectingLoopExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'self-correcting-loop' }
} catch {
    $failed += 'self-correcting-loop'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Behavior harness coverage and mutation strength' -ForegroundColor Cyan
$behaviorHarnessExercise = Join-Path $PSScriptRoot 'invoke-behavior-harness-mutation-exercise.ps1'
try {
    & $behaviorHarnessExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'behavior-harness-strength' }
} catch {
    $failed += 'behavior-harness-strength'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host '==> Repository architecture fitness and mutation strength' -ForegroundColor Cyan
$architectureFitnessExercise = Join-Path $PSScriptRoot 'invoke-architecture-fitness-mutation-exercise.ps1'
try {
    & $architectureFitnessExercise
    if ($LASTEXITCODE -ne 0) { $failed += 'architecture-fitness' }
} catch {
    $failed += 'architecture-fitness'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

if (-not $SkipSequenceDiagramStatusCheck) {
    Write-Host ''
    Write-Host '==> Sequence diagram status' -ForegroundColor Cyan
    $sequenceDiagramStatusCheck = Join-Path $PSScriptRoot 'check-sequence-diagram-status.ps1'
    try {
        & $sequenceDiagramStatusCheck
        if ($LASTEXITCODE -ne 0) {
            $failed += 'sequence-diagram-status'
        }
    } catch {
        $failed += 'sequence-diagram-status'
        Write-Host $_.Exception.Message -ForegroundColor Yellow
    }
}

Write-Host ''
Write-Host '==> Mermaid authoring lint (render validation remains separate)' -ForegroundColor Cyan
try {
    & (Join-Path $PSScriptRoot 'check-mermaid-diagrams.ps1') -BaseRef $baseRef -LintOnly
    & (Join-Path $PSScriptRoot 'invoke-mermaid-validation-exercise.ps1') -LintOnly
} catch {
    $failed += 'mermaid-authoring'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host '==> Preferred shared extensions' -ForegroundColor Cyan
$preferredExtensionsCheck = Join-Path $PSScriptRoot 'check-preferred-extensions.ps1'
try {
    & $preferredExtensionsCheck -BaseRef $baseRef -ScopePath $scopeRoot
    if ($LASTEXITCODE -ne 0) {
        $failed += 'preferred-extensions'
    }
} catch {
    $failed += 'preferred-extensions'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host '==> Unit-test coding standards' -ForegroundColor Cyan
$testStandardsCheck = Join-Path $PSScriptRoot 'check-test-standards.ps1'
try {
    & (Join-Path $PSScriptRoot 'invoke-test-standards-exercise.ps1')
    if ($LASTEXITCODE -ne 0) {
        $failed += 'test-standards-strength'
    }
} catch {
    $failed += 'test-standards-strength'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}
try {
    & $testStandardsCheck -BaseRef $baseRef -ScopePath $scopeRoot
    if ($LASTEXITCODE -ne 0) {
        $failed += 'test-standards'
    }
} catch {
    $failed += 'test-standards'
    Write-Host $_.Exception.Message -ForegroundColor Yellow
}

Write-Host ''
Write-Host "==> Format check on new .cs files vs '$baseRef'" -ForegroundColor Cyan
$newFiles = @(
    & git -C $repoRoot diff $baseRef --name-only --diff-filter=A -- '*.cs' |
    ForEach-Object { $_.Replace('\', '/') } |
    Where-Object { Test-PathUnderScope -RepoRelativePath $_ -ScopeRoot $scopeRoot }
)

if ($newFiles.Count -eq 0) {
    Write-Host 'no new .cs files; format check skipped' -ForegroundColor DarkGray
} else {
    $formatWorkspaces = @()
    if (-not [string]::IsNullOrWhiteSpace($workspace)) {
        $formatWorkspaces = @($workspace)
    } else {
        $formatWorkspaces = @(
            $newFiles |
            ForEach-Object { Resolve-WorkspaceForRepoFile -RepoRelativePath $_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Sort-Object -Unique
        )
    }

    if ($formatWorkspaces.Count -eq 0) {
        $failed += 'format'
        Write-Host 'format check failed; no solution/project workspace could be inferred for new .cs files' -ForegroundColor Yellow
    } else {
        foreach ($formatWorkspace in $formatWorkspaces) {
            $formatWorkspaceDir = Get-WorkspaceDirectory -WorkspacePath $formatWorkspace
            $workspaceFiles = @(
                $newFiles |
                Where-Object {
                    $absolutePath = Join-Path $repoRoot $_
                    $absolutePath.StartsWith($formatWorkspaceDir, [System.StringComparison]::OrdinalIgnoreCase)
                }
            )

            if ($workspaceFiles.Count -eq 0) {
                continue
            }

            $fmtExit = Invoke-DotnetFormat -WorkspacePath $formatWorkspace -RepoRelativeFiles $workspaceFiles
            if ($fmtExit -ne 0) {
                $failed += "format:$([System.IO.Path]::GetFileName($formatWorkspace))"
                Write-Host "format check failed; run 'dotnet format' for $formatWorkspace" -ForegroundColor Yellow
            } else {
                Write-Host "$($workspaceFiles.Count) new .cs file(s) OK for $([System.IO.Path]::GetFileName($formatWorkspace))" -ForegroundColor Green
            }
        }
    }
}

if ($failed.Count -gt 0) {
    Write-Host ''
    Write-Host ("verification failed: " + ($failed -join ', ')) -ForegroundColor Red
    exit 1
}

Write-Host ''
Write-Host 'verification passed' -ForegroundColor Green
exit 0
