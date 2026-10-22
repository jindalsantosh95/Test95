# Dead code, coverage, and dependency drift scan for a selected repo scope.
param(
    [string]$ScopePath = '.',
    [string]$Solution,
    [string]$CoverageOut = 'TestResults/coverage.json',
    [switch]$SkipCoverage,
    [switch]$SkipDependencyDrift,
    [switch]$SkipDocGardening,
    [int]$ActivePlanStaleDays = 30,
    [int]$OpenDebtStaleDays = 90,
    [ValidateSet('all','sln-only','csproj-only')]
    [string]$DriftScope = 'csproj-only'
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $scriptRoot rev-parse --show-toplevel).Trim())).Path

function Resolve-RepoPath {
    param([Parameter(Mandatory)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path (Get-Location).Path $Path
    }

    if (-not (Test-Path -LiteralPath $candidate)) {
        $candidate = Join-Path $repoRoot $Path
    }

    return (Resolve-Path -LiteralPath $candidate).Path
}

function Get-LastUpdatedUtcForPath {
    param([Parameter(Mandatory)][string]$AbsolutePath)

    if (-not (Test-Path -LiteralPath $AbsolutePath -PathType Leaf)) {
        return $null
    }

    $root = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $rootUri = New-Object System.Uri($root)
    $pathUri = New-Object System.Uri($AbsolutePath)
    $relativePath = [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()).Replace('\', '/')
    $gitTimestamp = (& git -C $repoRoot log -1 --format=%cI -- $relativePath 2>$null).Trim()
    if (-not [string]::IsNullOrWhiteSpace($gitTimestamp)) {
        try {
            return [DateTimeOffset]::Parse($gitTimestamp).UtcDateTime
        } catch {
            # Fall through to filesystem timestamp.
        }
    }

    return (Get-Item -LiteralPath $AbsolutePath).LastWriteTimeUtc
}

$scanRoot = Resolve-RepoPath -Path $ScopePath
$solutionPath = $null
if (-not [string]::IsNullOrWhiteSpace($Solution)) {
    $solutionPath = Resolve-RepoPath -Path $Solution
    $scanRoot = Split-Path -Parent $solutionPath
}

$resultsDir = Join-Path $scanRoot 'TestResults'

if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

function Resolve-DotnetOutdated {
    $existingCommand = Get-Command 'dotnet-outdated' -ErrorAction SilentlyContinue
    if ($existingCommand) {
        Write-Host "[INFO] Using dotnet-outdated from PATH: $($existingCommand.Source)"
        return $existingCommand.Source
    }

    $toolsDir = Join-Path $resultsDir '.tools'
    if (-not (Test-Path $toolsDir)) {
        New-Item -ItemType Directory -Path $toolsDir | Out-Null
    }

    $toolName = if ($IsWindows) { 'dotnet-outdated.exe' } else { 'dotnet-outdated' }
    $toolPath = Join-Path $toolsDir $toolName
    if (Test-Path $toolPath) {
        Write-Host "[INFO] Using dotnet-outdated from local tool path: $toolPath"
        return $toolPath
    }

    Write-Host "[INFO] dotnet-outdated not found on PATH. Installing dotnet-outdated-tool into $toolsDir"
    $installOutput = & dotnet tool install dotnet-outdated-tool --tool-path $toolsDir --ignore-failed-sources 2>&1
    $installOutput | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[WARN] dotnet-outdated-tool install failed with code $LASTEXITCODE. Skipping dependency drift scan."
        return $null
    }

    if (Test-Path $toolPath) {
        Write-Host "[INFO] Installed dotnet-outdated-tool: $toolPath"
        return $toolPath
    }

    Write-Host "[WARN] dotnet-outdated-tool installed but executable was not found at $toolPath. Skipping dependency drift scan."
    return $null
}

Write-Host "==> Running dead code scan (advisory) for $scanRoot"
# No dead code scanner configured. For .NET, consider NDepend or Roslyn analyzers.
Write-Host '[INFO] No dead code scanner configured. Skipping.'

if (-not $SkipDocGardening) {
    Write-Host '==> Running doc-gardening drift scan'
    try {
        $docWarnings = New-Object System.Collections.Generic.List[string]
        $activePlansPath = Join-Path $repoRoot 'docs/exec-plans/active'
        if (Test-Path -LiteralPath $activePlansPath -PathType Container) {
            $activePlans = @(
                Get-ChildItem -LiteralPath $activePlansPath -Filter '*.md' -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -ne 'readme.md' }
            )

            $staleDate = (Get-Date).AddDays(-1 * [Math]::Abs($ActivePlanStaleDays))
            foreach ($plan in $activePlans) {
                $lastUpdatedUtc = Get-LastUpdatedUtcForPath -AbsolutePath $plan.FullName
                if ($null -ne $lastUpdatedUtc -and $lastUpdatedUtc -lt $staleDate.ToUniversalTime()) {
                    $docWarnings.Add("active plan is stale: docs/exec-plans/active/$($plan.Name) (last updated $($lastUpdatedUtc.ToString('yyyy-MM-dd')) UTC)") | Out-Null
                }
            }
        }

        $decisionsPath = Join-Path $repoRoot 'docs/decisions'
        if (Test-Path -LiteralPath $decisionsPath -PathType Container) {
            $decisionFiles = @(
                Get-ChildItem -LiteralPath $decisionsPath -Filter '*.md' -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -ne 'readme.md' -and $_.Name -ne 'template.md' }
            )

            foreach ($decisionFile in $decisionFiles) {
                $decisionText = Get-Content -LiteralPath $decisionFile.FullName -Raw
                if ($decisionText -match '(?im)^-\s*Status:\s*proposed\s*$') {
                    $docWarnings.Add("decision still marked proposed: docs/decisions/$($decisionFile.Name)") | Out-Null
                }
            }
        }

        $techDebtPath = Join-Path $repoRoot 'docs/tech-debt-tracker.md'
        if (Test-Path -LiteralPath $techDebtPath -PathType Leaf) {
            $techDebtLines = @(Get-Content -LiteralPath $techDebtPath)
            $openRows = @(
                $techDebtLines |
                Where-Object { $_ -match '^\|\s*TD-\d+' -and $_ -match '\|\s*Open\s*\|$' }
            )

            if ($openRows.Count -gt 0) {
                $staleDebtDate = (Get-Date).AddDays(-1 * [Math]::Abs($OpenDebtStaleDays))
                $lastDebtUpdateUtc = Get-LastUpdatedUtcForPath -AbsolutePath $techDebtPath
                if ($null -ne $lastDebtUpdateUtc -and $lastDebtUpdateUtc -lt $staleDebtDate.ToUniversalTime()) {
                    $docWarnings.Add("open debt entries have not been updated in $OpenDebtStaleDays days: docs/tech-debt-tracker.md (last updated $($lastDebtUpdateUtc.ToString('yyyy-MM-dd')) UTC)") | Out-Null
                }
            }
        }

        if ($docWarnings.Count -eq 0) {
            Write-Host '[INFO] Doc-gardening drift scan found no warnings.'
        } else {
            Write-Host '[WARN] Doc-gardening drift warnings:'
            foreach ($warning in $docWarnings) {
                Write-Host " - $warning"
            }
            Write-Host '[INFO] Remediation: refresh stale plans, close or update debt entries, and move settled decisions to accepted/superseded states.'
        }
    } catch {
        Write-Host "[ERROR] Doc-gardening drift scan failed: $_"
    }
} else {
    Write-Host '==> Skipping doc-gardening drift scan by request (-SkipDocGardening).'
}

if ($SkipCoverage) {
    Write-Host '==> Skipping coverage scan by request (-SkipCoverage).'
} else {
    Write-Host '==> Running coverage scan'
    try {
        $testProjects = Get-ChildItem -Path $scanRoot -Recurse -Filter '*.Tests.csproj' -File |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

        if (-not $testProjects) {
            Write-Host '[WARN] No test projects found.'
        }

        foreach ($testProject in $testProjects) {
            Write-Host "[INFO] Running coverage for $($testProject.FullName)"
            & dotnet test "$($testProject.FullName)" -c Release --collect:"XPlat Code Coverage" --results-directory $resultsDir
            $coverageExitCode = $LASTEXITCODE
            if ($coverageExitCode -ne 0) {
                Write-Host "[WARN] dotnet test exited with code $coverageExitCode for $($testProject.Name). Continuing to dependency drift scan."
            }
        }

        $coverageFile = Get-ChildItem -Path $resultsDir -Filter 'coverage.cobertura.xml' -Recurse | Select-Object -First 1
        if ($coverageFile) {
            Write-Host "[INFO] Coverage file: $($coverageFile.FullName)"
        } else {
            Write-Host '[WARN] No coverage file found.'
        }
    } catch {
        Write-Host "[ERROR] Coverage scan failed: $_"
    }
}

Write-Host '==> Running dependency drift scan (dotnet-outdated)'
if ($SkipDependencyDrift) {
    Write-Host '==> Skipping dependency drift scan by request (-SkipDependencyDrift).'
    Write-Host '==> Drift/coverage scan complete.'
    exit 0
}

try {
    $dotnetOutdated = Resolve-DotnetOutdated
    if (-not $dotnetOutdated) {
        Write-Host '==> Drift/coverage scan complete.'
        exit 0
    }

    $includePatterns = switch ($DriftScope) {
        'sln-only' { @('*.sln') }
        'csproj-only' { @('*.csproj') }
        default { @('*.csproj','*.sln') }
    }

    if ($solutionPath) {
        $items = @((Get-Item -LiteralPath $solutionPath))
    } else {
        $items = Get-ChildItem -Path $scanRoot -Recurse -Include $includePatterns -File |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    }

    if (-not $items) {
        Write-Host '[WARN] No solution/project files found for drift scan.'
    }

    foreach ($item in $items) {
        Write-Host "[INFO] Checking drift for $($item.FullName)"
        & $dotnetOutdated "$($item.FullName)" --output "$resultsDir/$($item.BaseName).outdated.txt" --fail-on-updates --no-restore
    }
    Write-Host '[INFO] Dependency drift scan complete.'
} catch {
    Write-Host "[ERROR] Dependency drift scan failed: $_"
}

Write-Host '==> Drift/coverage scan complete.'
exit 0
