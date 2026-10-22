#Requires -Version 5.1
<#
.SYNOPSIS
    Verifies Azure Functions app projects have matching deployment projects.

.DESCRIPTION
    Finds Azure Functions isolated worker app projects included in the solution and checks
    that each has a sibling or repository-central <FunctionAppProject>.Deployment folder with expected
    deployment assets. Legacy .deployproj files need not be loaded in the solution;
    deployment assets are validated on disk independently of Visual Studio support.
#>
[CmdletBinding()]
param(
    [string]$SolutionPath,
    [string]$ExceptionsPath = (Join-Path $PSScriptRoot '../docs/deployment-asset-exceptions.json'),
    [string[]]$RequiredDeploymentFiles = @(
        'Deploy-AzureResourceGroup.ps1',
        'Deployment.targets',
        'Template\azuredeploy.json',
        'Template\azuredeploy_slot.json',
        'Template\azuredeploy_apim.json',
        'Parameters\dev-azuredeploy.parameters.json',
        'Parameters\sit-azuredeploy.parameters.json',
        'Parameters\uat-azuredeploy.parameters.json',
        'Parameters\prod-azuredeploy.parameters.json'
    )
)

$ErrorActionPreference = 'Stop'
$exceptions = @{}
$registry = Get-Content -LiteralPath $ExceptionsPath -Raw | ConvertFrom-Json
if ($registry.schemaVersion -ne 1) { throw 'Unsupported deployment exception schema.' }
foreach ($entry in $registry.exceptions) {
    if ([string]::IsNullOrWhiteSpace($entry.project) -or [string]::IsNullOrWhiteSpace($entry.reason) -or $exceptions.ContainsKey($entry.project)) {
        throw 'Deployment exceptions require unique projects and a documented reason.'
    }
    $files = @($entry.allowedMissingFiles | ForEach-Object { $_.Replace('\', '/') })
    foreach ($file in $files) {
        if ($file -notin @('Template/azuredeploy_slot.json', 'Template/azuredeploy_apim.json',
                'Deploy-AzureResourceGroup.ps1', 'Deployment.targets', "$($entry.project).Deployment.deployproj")) {
            throw "Unsupported deployment exception '$file': core templates and parameters cannot be exempted."
        }
    }
    $exceptions[$entry.project] = $files
}

if ([string]::IsNullOrWhiteSpace($SolutionPath)) {
    Write-Host 'no solution supplied; deployment project check skipped' -ForegroundColor DarkGray
    return
}

$resolvedSolutionPath = (Resolve-Path -LiteralPath $SolutionPath).Path
$solutionDir = Split-Path -Parent $resolvedSolutionPath
$solutionContent = Get-Content -LiteralPath $resolvedSolutionPath -Raw
$solutionProjectPaths = @([regex]::Matches($solutionContent, '(?m)^Project\("[^"]+"\)\s*=\s*"[^"]+",\s*"([^"]+)"') |
    ForEach-Object { $_.Groups[1].Value.Replace('/', '\') })

$appProjectDirs = Get-ChildItem -LiteralPath $solutionDir -Directory |
    Where-Object {
        $_.Name -notmatch '\.(Tests|Deployment)$' -and
        $solutionProjectPaths -contains "$($_.Name)\$($_.Name).csproj" -and
        (Test-Path -LiteralPath (Join-Path $_.FullName "$($_.Name).csproj") -PathType Leaf)
    } |
    Where-Object {
        $projectFile = Join-Path $_.FullName "$($_.Name).csproj"
        $projectText = Get-Content -LiteralPath $projectFile -Raw
        $projectText -match 'Microsoft\.Azure\.Functions\.Worker' -and
        $projectText -match '<AzureFunctionsVersion>' -and
        $projectText -match '<OutputType>\s*Exe\s*</OutputType>' -and
        $projectText -notmatch '<DeploymentProjectRequired>\s*false\s*</DeploymentProjectRequired>' -and
        (Test-Path -LiteralPath (Join-Path $_.FullName 'host.json') -PathType Leaf)
    } |
    Sort-Object Name

if ($appProjectDirs.Count -eq 0) {
    Write-Host 'no Azure Functions app projects found; deployment project check skipped' -ForegroundColor DarkGray
    return
}

$missing = @()

foreach ($appProjectDir in $appProjectDirs) {
    $appProjectName = $appProjectDir.Name
    $deploymentProjectName = "$appProjectName.Deployment"
    $allowedMissing = @($exceptions[$appProjectName])
    $deploymentDir = Join-Path $solutionDir $deploymentProjectName

    # Microservices keep deployment assets in the repository's shared deployment tree.
    if (-not (Test-Path -LiteralPath $deploymentDir -PathType Container)) {
        $centralDeploymentDir = Join-Path (Split-Path -Parent $solutionDir) "VAA.EAPI.Deployments/$deploymentProjectName"
        if (Test-Path -LiteralPath $centralDeploymentDir -PathType Container) {
            $deploymentDir = $centralDeploymentDir
        }
    }

    if (-not (Test-Path -LiteralPath $deploymentDir -PathType Container)) {
        $missing += "$deploymentProjectName directory"
        continue
    }

    $deploymentProjectFileName = "$deploymentProjectName.deployproj"
    $deploymentProjectFile = Join-Path $deploymentDir $deploymentProjectFileName

    # Legacy project names can differ from the owning folder (for example PartnerDataMangament).
    $deploymentProjects = @(Get-ChildItem -LiteralPath $deploymentDir -Filter '*.deployproj' -File)
    if ($deploymentProjects.Count -gt 1 -or ($deploymentProjects.Count -eq 0 -and $allowedMissing -notcontains $deploymentProjectFileName)) {
        $missing += "$deploymentProjectName\$deploymentProjectFileName"
    }

    $projectText = Get-Content -LiteralPath (Join-Path $appProjectDir.FullName "$appProjectName.csproj") -Raw
    $digitalOnly = $projectText -match '<DeploymentTemplateLayout>\s*digital-only\s*</DeploymentTemplateLayout>'
    foreach ($requiredFile in $RequiredDeploymentFiles) {
        if ($digitalOnly) {
            # An intentional deployment layout is not a missing-asset exception.
            $requiredFile = $requiredFile.Replace('Template\azuredeploy.json', 'Template\azuredeploy_digital.json').Replace('Template\azuredeploy_slot.json', 'Template\azuredeploy_digital_slot.json')
        }
        $path = Join-Path $deploymentDir $requiredFile
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            if ($allowedMissing -contains $requiredFile.Replace('\', '/')) {
                Write-Host "documented legacy exception: $deploymentProjectName\$requiredFile" -ForegroundColor Yellow
            } else {
                $missing += "$deploymentProjectName\$requiredFile"
            }
        }
    }

    if ($projectText -match '<TargetFramework>\s*net10\.0\s*</TargetFramework>') {
        & (Join-Path $PSScriptRoot 'check-functions-worker-runtime.ps1') -DeploymentPath $deploymentDir
    }
}

if ($missing.Count -gt 0) {
    Write-Host 'deployment project check failed; missing required deployment assets:' -ForegroundColor Red
    foreach ($item in $missing) {
        Write-Host " - $item" -ForegroundColor Red
    }

    throw 'Deployment project check failed.'
}

$projectList = ($appProjectDirs | ForEach-Object { $_.Name }) -join ', '
Write-Host "deployment project check passed for $($appProjectDirs.Count) app project(s): $projectList" -ForegroundColor Green
