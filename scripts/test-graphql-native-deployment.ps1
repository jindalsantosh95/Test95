#requires -Version 7.0
# Offline regression exercise. No Azure calls or app-setting changes are made.
$ErrorActionPreference = 'Stop'
$scriptUnderTest = Join-Path $PSScriptRoot 'deploy-graphql-native-websockets.ps1'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('graphql-native-deployment-' + [guid]::NewGuid().ToString('N'))
$null = New-Item $testRoot -ItemType Directory
$parameterFile = Join-Path $testRoot 'dev.json'
$preparedFile = Join-Path $testRoot 'prepared.json'
$packageFile = Join-Path $testRoot 'app.zip'
$parameters = @{ parameters = @{
    NativeWebSocketsConfiguration = @{ value = @{
        startupHookPath = 'D:\old\hook.dll'
        existingStartupHooks = 'C:\other\existing.dll'
        workerProcessCount = '1'
        path = '/api/v1/GraphQL/ws'
    } }
} }
$parameters | ConvertTo-Json -Depth 10 | Set-Content $parameterFile
$archive = [IO.Compression.ZipFile]::Open($packageFile, [IO.Compression.ZipArchiveMode]::Create)
$entry = $archive.CreateEntry('hostextensions/VAA.EAPI.GraphQL.WebSocketHost.dll')
$stream = $entry.Open()
$bytes = [Text.Encoding]::UTF8.GetBytes('test fixture, not an assembly')
$stream.Write($bytes)
$stream.Dispose()
$archive.Dispose()
$global:graphqlNativeTest_hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
$global:graphqlNativeTest_writes = [Collections.Generic.List[object]]::new()
$global:graphqlNativeTest_failRestart = $false
$global:graphqlNativeTest_wrongHash = $false
$global:graphqlNativeTest_live = @{
    NativeWebSocketsPath = '/api/v1/GraphQL/ws'
    NativeWebSocketsRedisPrefix = 'graphql-dev-test'
    RedisCacheConnectionString = 'synthetic-test-only'
    DOTNET_STARTUP_HOOKS = 'C:\other\existing.dll;C:\monitor\hook.dll'
    UnrelatedSetting = 'preserve me'
}

function az { $global:LASTEXITCODE = 0; 'offline-token' }
function Start-Sleep { }
function Invoke-RestMethod
{
    param($Uri, $Method, $Headers, $TimeoutSec, $ContentType, $Body)
    if ($Uri -like 'https://fixture.scm.azurewebsites.net/*')
    {
        $remoteHash = if ($global:graphqlNativeTest_wrongHash) { 'wrong' } else { $global:graphqlNativeTest_hash }
        return @{ ExitCode = 0; Output = (@{ path = 'C:\actual\site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll'; hash = $remoteHash } | ConvertTo-Json) }
    }
    if ($Uri -notlike 'https://management.azure.com/*/sites/fixture*') { throw 'Unexpected remote request in offline test.' }
    if ($Method -eq 'PUT' -or $Method -eq 'PATCH')
    {
        $global:graphqlNativeTest_writes.Add(@{ Uri = $Uri; Body = ($Body | ConvertFrom-Json -AsHashtable) })
        return @{}
    }
    if ($Uri -like '*/restart?*')
    {
        if ($global:graphqlNativeTest_failRestart) { $global:graphqlNativeTest_failRestart = $false; throw 'Simulated activation failure.' }
        return @{}
    }
    if ($Uri -like '*/config/appsettings/list?*') { return @{ properties = $global:graphqlNativeTest_live } }
    return @{ properties = @{ reserved = $false; defaultHostName = 'fixture.azurewebsites.net'; hostNameSslStates = @(@{ hostType = 'Repository'; name = 'fixture.scm.azurewebsites.net' }) } }
}

function Assert([bool]$Condition, [string]$Message)
{
    if (-not $Condition) { throw $Message }
}

$arguments = @{
    Environment = 'dev'; SubscriptionId = 'fixture'; ResourceGroupName = 'fixture'; AppName = 'fixture'
    ParameterFile = $parameterFile; PreparedParameterFile = $preparedFile; PackagePattern = $packageFile
}
try
{
    & $scriptUnderTest @arguments -Phase Prepare
    $prepared = Get-Content $preparedFile -Raw | ConvertFrom-Json
    Assert ($prepared.parameters.NativeWebSocketsConfiguration.value.startupHookPath -eq '') 'Prepare must clear the assumed path.'
    Assert ($prepared.parameters.NativeWebSocketsConfiguration.value.existingStartupHooks -eq 'C:\other\existing.dll;C:\monitor\hook.dll') 'Prepare must preserve and deduplicate unrelated hooks.'
    Assert ($global:graphqlNativeTest_writes.Count -eq 0) 'Prepare must not change live settings.'
    Assert ((Get-Content $preparedFile -Raw) -notmatch 'preserve me') 'Live settings must never be written to the parameter file.'
    Write-Host 'PASS: prepare disables native hook and preserves unrelated hooks without leaking live settings.'

    $global:graphqlNativeTest_wrongHash = $true
    $failed = $false
    try { & $scriptUnderTest @arguments -Phase Activate } catch { $failed = $true }
    Assert $failed 'An old or mismatched deployed DLL must fail verification.'
    Assert ($global:graphqlNativeTest_writes.Count -eq 0) 'Invalid package must not enable the hook.'
    Write-Host 'PASS: mismatched deployed DLL prevents activation.'

    $global:graphqlNativeTest_wrongHash = $false
    $global:graphqlNativeTest_failRestart = $true
    $failed = $false
    try { & $scriptUnderTest @arguments -Phase Activate } catch { $failed = $true }
    Assert $failed 'Restart failure must fail deployment.'
    $updates = @($global:graphqlNativeTest_writes | Where-Object { $_.Uri -like '*/config/appsettings?*' })
    Assert ($updates.Count -eq 2) 'Activation failure must write rollback settings.'
    Assert ($updates[0].Body.properties.DOTNET_STARTUP_HOOKS -like '*;C:\actual\site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll') 'Use the discovered absolute path.'
    Assert ($updates[0].Body.properties.FUNCTIONS_WORKER_PROCESS_COUNT -eq '1') 'Activation must use one worker.'
    Assert ($updates[1].Body.properties.DOTNET_STARTUP_HOOKS -eq 'C:\other\existing.dll;C:\monitor\hook.dll') 'Rollback must retain unrelated hooks.'
    Assert ($updates[1].Body.properties.UnrelatedSetting -eq 'preserve me') 'Rollback must preserve other settings.'
    Write-Host 'PASS: discovered C: path replaces old D: path; activation failure restores safe settings.'

    $global:graphqlNativeTest_writes.Clear()
    $global:graphqlNativeTest_live.Remove('RedisCacheConnectionString')
    $parameters.parameters.NativeWebSocketsConfiguration.value.redisPrefix = 'graphql-dev-test'
    $parameters.parameters.NativeWebSocketsConfiguration.value.workerProcessCount = '2'
    $parameters | ConvertTo-Json -Depth 10 | Set-Content $parameterFile
    $failed = $false
    try { & $scriptUnderTest @arguments -Phase Activate } catch { $failed = $true }
    Assert $failed 'Redis activation must reject missing deployed Redis settings.'
    Assert ($global:graphqlNativeTest_writes.Count -eq 0) 'Missing Redis settings must not activate the hook.'
    $global:graphqlNativeTest_live.NativeWebSocketsRedisPrefix = 'graphql-dev-test'
    $global:graphqlNativeTest_live.RedisCacheConnectionString = 'synthetic-test-only'
    $global:graphqlNativeTest_failRestart = $true
    try { & $scriptUnderTest @arguments -Phase Activate } catch { }
    $updates = @($global:graphqlNativeTest_writes | Where-Object { $_.Uri -like '*/config/appsettings?*' })
    Assert ($updates.Count -eq 2) 'Redis activation failure must also roll back.'
    Assert ($updates[0].Body.properties.FUNCTIONS_WORKER_PROCESS_COUNT -eq '2') 'Redis mode must preserve the selected worker count.'
    Assert (-not $updates[0].Body.properties.ContainsKey('NativeWebSocketsSingleInstance')) 'Activation must not add a single-instance setting.'
    Write-Host 'PASS: Redis activation requires deployed configuration and preserves multiple workers.'

    $arguments.Environment = 'prod'
    $failed = $false
    try { & $scriptUnderTest @arguments -Phase Prepare } catch { $failed = $true }
    Assert $failed 'Non-DEV activation must be rejected.'
    Write-Host 'PASS: non-DEV environment rejected.'

    $repository = Split-Path $PSScriptRoot -Parent
    foreach ($deployment in @('VAA.EAPI.GraphQL.Deployment', 'VAA.EAPI.GraphQL.Stitch.Deployment'))
    {
        $deploymentRoot = Join-Path $repository "VAA.EAPI.GraphQL/$deployment"
        foreach ($file in Get-ChildItem "$deploymentRoot/Template/*.json")
        {
            $template = Get-Content $file.FullName -Raw | ConvertFrom-Json -AsHashtable
            Assert ($template.parameters.Count -le 256) "$($file.Name): exceeds the ARM 256-parameter limit."
            if ($template.parameters.ContainsKey('NativeWebSocketsConfiguration'))
            {
                Assert ($template.parameters.ContainsKey('NativeWebSocketsConfiguration')) "$($file.Name): missing grouped native settings."
            }
        }
        foreach ($file in Get-ChildItem "$deploymentRoot/Parameters/prod*parameters.json")
        {
            $production = Get-Content $file.FullName -Raw | ConvertFrom-Json -AsHashtable
            Assert ($production.parameters.NativeWebSocketsConfiguration.value.startupHookPath -eq '') "PROD activation must remain deferred until accepted."
        }
    }
    foreach ($name in @('azuredeploy_digital.json', 'azuredeploy_sandbox.json'))
    {
        $template = Get-Content (Join-Path $repository "VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Deployment/Template/$name") -Raw | ConvertFrom-Json -AsHashtable
        $appSettings = @($template.resources | Where-Object { $_.type -eq 'Microsoft.Web/sites/config' -and $_.name -match 'appsettings' })
        Assert ($appSettings.Count -eq 1) "$name must have one authoritative appsettings resource."
        foreach ($key in @('NativeWebSocketsRedisPrefix', 'NativeWebSocketsPath', 'NativeWebSocketsBridgeSecret', 'FUNCTIONS_WORKER_PROCESS_COUNT', 'DOTNET_STARTUP_HOOKS'))
        {
            Assert ($appSettings[0].properties.ContainsKey($key)) "$name is missing authoritative setting $key."
        }
    }
    foreach ($relativePath in @(
        'VAA.EAPI.GraphQL.Deployment/Template/azuredeploy_slot.json',
        'VAA.EAPI.GraphQL.Deployment/Template/azuredeploy_digital_slot.json',
        'VAA.EAPI.GraphQL.Mobile.Deployment/Template/azuredeploy_slot.json',
        'VAA.EAPI.GraphQL.Stitch.Deployment/Template/azuredeploy_slot.json'))
    {
        $template = Get-Content (Join-Path $repository "VAA.EAPI.GraphQL/$relativePath") -Raw | ConvertFrom-Json -AsHashtable
        $deployment = @($template.resources | Where-Object { $_.type -eq 'Microsoft.Resources/deployments' -and $_.name -match 'subscription-slots-' })
        Assert ($deployment.Count -eq 1) "$relativePath must configure slot-sticky subscription namespaces."
        $properties = $deployment[0].properties
        Assert ($properties.expressionEvaluationOptions.scope -eq 'inner') 'Slot settings must be read outside the nested write deployment to avoid self-reference.'
        Assert ($properties.parameters.existingSlotSettings.value -match "reference\(resourceId\('Microsoft.Web/sites/config'.*'slotConfigNames'") 'Read existing sticky settings from the parent app.'
        Assert ($properties.template.variables.emptyAppSettingNames -is [array] -and $properties.template.variables.emptyAppSettingNames.Count -eq 0) 'Use a literal empty array in the nested template; avoid the ARM json empty-array evaluation failure.'
        $sticky = $properties.template.resources[0]
        Assert ($sticky.name -eq "[concat(parameters('appName'), '/slotConfigNames')]") 'Sticky settings belong to the parent app, not the slot.'
        Assert ($sticky.properties.StartsWith("[union(parameters('existingSlotSettings'), createObject('appSettingNames', union(")) 'Preserve all existing sticky properties, including connection strings and storage.'
        Assert ($sticky.properties.Contains("coalesce(tryGet(parameters('existingSlotSettings'), 'appSettingNames'), variables('emptyAppSettingNames'))")) 'Handle both existing sticky app settings and an empty configuration.'
        Assert ($sticky.properties.Contains("createArray('NativeWebSocketsRedisPrefix')")) 'Add the Redis prefix without replacing other sticky app settings.'
        Assert ($properties.parameters.existingAppSettings.value -eq "[list(resourceId('Microsoft.Web/sites/config', parameters('FunctionAppName'), 'appsettings'), '2022-03-01').properties]") 'Read production app settings before migrating the namespace.'
        Assert ($properties.template.parameters.existingAppSettings.type -eq 'secureObject') 'Existing production settings must not be exposed in deployment history.'
        $seed = @($properties.template.resources | Where-Object { $_.name -eq "[concat(parameters('appName'), '/appsettings')]" })
        Assert ($seed.Count -eq 1) 'First promotion must seed production app settings.'
        Assert ($seed[0].condition -eq "[empty(coalesce(tryGet(parameters('existingAppSettings'), 'NativeWebSocketsRedisPrefix'), ''))]") 'Seed missing or empty prefixes only; preserve established production namespaces.'
        Assert ($seed[0].properties -eq "[union(parameters('existingAppSettings'), createObject('NativeWebSocketsRedisPrefix', parameters('appName')))]") 'Seed the production app name without replacing existing app settings.'

    }
    $pipeline = Get-Content (Join-Path $repository 'VAA.EAPI.Pipelines/functions/deployment_graphqlstitch.yaml') -Raw
    $publicOverrides = @($pipeline -split '\r?\n' | Where-Object { $_ -match 'overrideParameters: -FunctionAppName.*functionAppName_Public' })
    Assert ($publicOverrides.Count -eq 2) 'Public Stitch must have separate DEV and non-DEV overrides.'
    Assert ($publicOverrides[0] -notmatch 'NativeWebSocketsConfiguration') 'DEV must retain the complete prepared native configuration.'
    Assert ($publicOverrides[1] -match 'NativeWebSocketsConfiguration.*"startupHookPath":""') 'Non-DEV must retain the deferred native hook override.'
    Assert ($pipeline -match '(?m)if eq\(parameters.environment, ''dev''\).*\r?\n\s+overrideParameters: -FunctionAppName.*functionAppName_Public') 'The preserving override must be selected for DEV.'
    Write-Host 'PASS: all four slot namespaces are sticky and public DEV Stitch retains prepared configuration.'
    Write-Host 'PASS: flag-free templates, deferred PROD and authoritative Digital/Sandbox app settings.'
}
finally
{
    # Only delete the exact temporary fixture directory created by this invocation.
    $resolved = [IO.Path]::GetFullPath($testRoot)
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup path.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
    Remove-Variable -Scope Global -Name 'graphqlNativeTest_*'
}
