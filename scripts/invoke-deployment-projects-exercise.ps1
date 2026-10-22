#Requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$checker = Join-Path $PSScriptRoot 'check-deployment-projects.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) ('deployment-projects-' + [guid]::NewGuid().ToString('N'))
try {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    $solutionDir = Join-Path $root 'Applications'
    New-Item -ItemType Directory -Path $solutionDir | Out-Null
    $solution = Join-Path $solutionDir 'Gateway.sln'
    $appContent = '<Project><PropertyGroup><AzureFunctionsVersion>v4</AzureFunctionsVersion><OutputType>Exe</OutputType></PropertyGroup><ItemGroup><PackageReference Include="Microsoft.Azure.Functions.Worker" /></ItemGroup></Project>'
    foreach ($name in @('Gateway', 'Neighbour')) {
        $directory = Join-Path $solutionDir $name
        New-Item -ItemType Directory -Path $directory | Out-Null
        Set-Content -LiteralPath (Join-Path $directory "$name.csproj") -Value $appContent
        Set-Content -LiteralPath (Join-Path $directory 'host.json') -Value '{}'
    }
    $deployment = Join-Path $solutionDir 'Gateway.Deployment'
    New-Item -ItemType Directory -Path $deployment | Out-Null
    Set-Content -LiteralPath (Join-Path $deployment 'Gateway.Deployment.deployproj') -Value '<Project />'
    $asset = Join-Path $deployment 'Deployment.targets'
    Set-Content -LiteralPath $asset -Value '<Project />'
    $appEntry = 'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Gateway", "Gateway\Gateway.csproj", "{00000000-0000-0000-0000-000000000001}"'
    $deploymentEntry = 'Project("{151D2E53-A2C4-4D7D-83FE-D05416EBD58E}") = "Gateway.Deployment", "Gateway.Deployment\Gateway.Deployment.deployproj", "{00000000-0000-0000-0000-000000000002}"'
    $entries = @($appEntry, 'EndProject', $deploymentEntry, 'EndProject') -join [Environment]::NewLine

    function Assert-DeploymentCheck {
        param([bool]$ShouldPass, [string]$Case, [string]$ExceptionsPath)
        $rejected = $false
        try {
            $arguments = @{ SolutionPath = $solution; RequiredDeploymentFiles = @('Deployment.targets') }
            if ($ExceptionsPath) { $arguments.ExceptionsPath = $ExceptionsPath }
            & $checker @arguments
        } catch {
            if ($_.Exception.Message -ne 'Deployment project check failed.') { throw }
            $rejected = $true
        }
        if ($rejected -eq $ShouldPass) { throw "Deployment check produced the wrong result: $Case" }
    }

    Set-Content -LiteralPath $solution -Value $entries
    Assert-DeploymentCheck -ShouldPass $true -Case 'Neighbouring app outside solution is ignored'
    Set-Content -LiteralPath $solution -Value $entries.Replace('\', '/')
    Assert-DeploymentCheck -ShouldPass $true -Case 'Forward-slash solution paths are accepted'
    Set-Content -LiteralPath $solution -Value $appEntry
    Assert-DeploymentCheck -ShouldPass $true -Case 'Deployment assets remain valid without a legacy deployment solution entry'
    Rename-Item -LiteralPath (Join-Path $deployment 'Gateway.Deployment.deployproj') -NewName 'Legacy.Deployment.deployproj'
    Assert-DeploymentCheck -ShouldPass $true -Case 'Single legacy project filename need not match the folder'
    $centralRoot = Join-Path $root 'VAA.EAPI.Deployments'
    New-Item -ItemType Directory -Path $centralRoot | Out-Null
    if (-not ([IO.Path]::GetFullPath($deployment).StartsWith($root + [IO.Path]::DirectorySeparatorChar)) -or
        -not ([IO.Path]::GetFullPath($centralRoot).StartsWith($root + [IO.Path]::DirectorySeparatorChar))) {
        throw 'Deployment fixture move must remain within its temporary root.'
    }
    Move-Item -LiteralPath $deployment -Destination $centralRoot
    $deployment = Join-Path $centralRoot 'Gateway.Deployment'
    $asset = Join-Path $deployment 'Deployment.targets'
    Assert-DeploymentCheck -ShouldPass $true -Case 'Repository-central deployment folder is accepted'
    Set-Content -LiteralPath $solution -Value $entries
    Remove-Item -LiteralPath $asset
    Assert-DeploymentCheck -ShouldPass $false -Case 'Included app requires deployment assets'
    $exceptionFile = Join-Path $root 'exceptions.json'
    Set-Content -LiteralPath $exceptionFile -Value '{"schemaVersion":1,"exceptions":[{"project":"Gateway","reason":"Fixture legacy asset exception","allowedMissingFiles":["Deployment.targets"]}]}'
    Assert-DeploymentCheck -ShouldPass $true -Case 'Documented legacy asset exception is scoped to its project' -ExceptionsPath $exceptionFile
    Set-Content -LiteralPath $exceptionFile -Value '{"schemaVersion":1,"exceptions":[{"project":"Neighbour","reason":"Unrelated exception","allowedMissingFiles":["Deployment.targets"]}]}'
    Assert-DeploymentCheck -ShouldPass $false -Case 'Other projects do not inherit an exception' -ExceptionsPath $exceptionFile
    Set-Content -LiteralPath $exceptionFile -Value '{"schemaVersion":1,"exceptions":[{"project":"Gateway","reason":"Invalid core exception","allowedMissingFiles":["Template/azuredeploy.json"]}]}'
    $rejected = $false
    try { & $checker -SolutionPath $solution -ExceptionsPath $exceptionFile } catch {
        if ($_.Exception.Message -notlike '*core templates and parameters cannot be exempted*') { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw 'Core template exemption was accepted.' }
    # Digital-only layouts require their own templates, without a legacy exception.
    Set-Content -LiteralPath $asset -Value '<Project />'
    $project = Join-Path $solutionDir 'Gateway/Gateway.csproj'
    Set-Content -LiteralPath $project -Value $appContent.Replace('<AzureFunctionsVersion>', '<DeploymentTemplateLayout>digital-only</DeploymentTemplateLayout><AzureFunctionsVersion>')
    $templates = Join-Path $deployment 'Template'
    New-Item -ItemType Directory -Path $templates | Out-Null
    Set-Content -LiteralPath (Join-Path $templates 'azuredeploy_digital.json') -Value '{}'
    Set-Content -LiteralPath (Join-Path $templates 'azuredeploy_digital_slot.json') -Value '{}'
    & $checker -SolutionPath $solution -RequiredDeploymentFiles @('Template\azuredeploy.json', 'Template\azuredeploy_slot.json')
    Remove-Item -LiteralPath (Join-Path $templates 'azuredeploy_digital_slot.json')
    $rejected = $false
    try { & $checker -SolutionPath $solution -RequiredDeploymentFiles @('Template\azuredeploy_slot.json') } catch {
        if ($_.Exception.Message -ne 'Deployment project check failed.') { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw 'Digital-only layout accepted a missing digital slot template.' }

    # Exercise the runtime guard through the owning deployment checker.
    Set-Content -LiteralPath $project -Value $appContent.Replace('<AzureFunctionsVersion>', '<TargetFramework>net10.0</TargetFramework><AzureFunctionsVersion>')
    $runtimeFile = Join-Path $templates 'azuredeploy_digital.json'
    foreach ($json in @(
        '{"resources":[{"properties":{"siteConfig":{"appSettings":[{"name":"FUNCTIONS_WORKER_RUNTIME","value":"dotnet-isolated"}]}}}]}',
        '{"parameters":{"AppSetting:FUNCTIONS_WORKER_RUNTIME":{"defaultValue":"dotnet-isolated"}},"resources":[{"properties":{"FUNCTIONS_WORKER_RUNTIME":"[parameters(''AppSetting:FUNCTIONS_WORKER_RUNTIME'')]"}}]}'
    )) {
        Set-Content -LiteralPath $runtimeFile -Value $json
        Assert-DeploymentCheck -ShouldPass $true -Case 'Isolated runtime literals and parameter references are accepted'
    }
    $parameters = Join-Path $deployment 'Parameters'
    New-Item -ItemType Directory -Path $parameters | Out-Null
    foreach ($json in @(
        '{"parameters":{"AppSetting:FUNCTIONS_WORKER_RUNTIME":{"value":"dotnet"}}}',
        '{"resources":[{"properties":{"siteConfig":{"appSettings":[{"name":"FUNCTIONS_WORKER_RUNTIME","value":"dotnet"}]}}}]}',
        '{"parameters":{"AppSetting:FUNCTIONS_WORKER_RUNTIME":{"defaultValue":"dotnet"}}}'
    )) {
        Set-Content -LiteralPath (Join-Path $parameters 'dev.json') -Value $json
        $rejected = $false
        try { & $checker -SolutionPath $solution -RequiredDeploymentFiles @('Deployment.targets') } catch {
            if ($_.Exception.Message -notlike '*FUNCTIONS_WORKER_RUNTIME must be dotnet-isolated*') { throw }
            $rejected = $true
        }
        if (-not $rejected) { throw 'Non-isolated runtime was accepted for .NET 10.' }
    }
    Write-Host 'deployment project exercise passed (16 cases)'  -ForegroundColor Green
} finally {
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $fixtureRoot = [IO.Path]::GetFullPath($root)
    if ((Split-Path -Parent $fixtureRoot) -ne $temporaryRoot -or
        (Split-Path -Leaf $fixtureRoot) -notmatch '^deployment-projects-[0-9a-f]{32}$') {
        throw 'Refusing to remove a fixture outside the expected temporary directory.'
    }
    if (Test-Path -LiteralPath $fixtureRoot) {
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}
