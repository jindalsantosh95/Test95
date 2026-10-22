#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$templates = Join-Path $repo 'VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Deployment/Template'
foreach ($name in @('azuredeploy_digital.json', 'azuredeploy_digital_slot.json')) {
    $template = Get-Content (Join-Path $templates $name) -Raw | ConvertFrom-Json
    if ($template.parameters.PublishFunctionKey.defaultValue -ne $true) {
        throw "$name must preserve legacy publication by default."
    }
    $secret = @($template.resources | Where-Object type -eq 'Microsoft.KeyVault/vaults/secrets')
    if ($secret.Count -ne 1 -or $secret[0].condition -cne "[parameters('PublishFunctionKey')]" -or
        -not $secret[0].properties.value.StartsWith("[if(parameters('PublishFunctionKey'), listkeys(")) {
        throw "$name must guard both the secret resource and host-dependent listKeys."
    }
}
$publication = Get-Content (Join-Path $templates 'publish_function_key.json') -Raw | ConvertFrom-Json
if ($publication.resources.Count -ne 1 -or
    $publication.resources[0].type -ne 'Microsoft.KeyVault/vaults/secrets' -or
    $publication.resources[0].properties.value -notlike '*functionKeys.default]*' -or
    $publication.PSObject.Properties.Name -contains 'outputs') {
    throw 'Post-deployment template must publish the existing default key without outputs.'
}
$pipelineRoot = Join-Path $repo 'VAA.EAPI.Pipelines.DigitalServices/functions_digital/Graphql_Rest_Gateway'
foreach ($name in @('deployment_GqlRest.yaml', 'deployment_GqlRest_slot.yaml')) {
    $text = Get-Content (Join-Path $pipelineRoot $name) -Raw
    if ([regex]::Matches($text, 'overrideParameters: -PublishFunctionKey false -OperationsVisibilityMode').Count -ne 2) {
        throw "$name must defer publication for both apps."
    }
    $publish = [regex]::Matches($text, '- template: publish_rest_function_key.yaml')
    $readyPattern = if ($name -like '*_slot.yaml') { "displayName: 'Complete Swap slot to Env'" }
        else { "displayName: 'Start Azure App Service:" }
    $ready = [regex]::Matches($text, $readyPattern)
    if ($publish.Count -ne 2 -or $ready.Count -ne 2) { throw "$name must publish after both apps start/swap." }
    for ($i = 0; $i -lt 2; $i++) {
        if ($publish[$i].Index -lt $ready[$i].Index -or
            ($i -eq 0 -and $publish[$i].Index -gt $ready[1].Index)) {
            throw "$name publishes before the appropriate app is ready."
        }
    }
}
$readinessYaml = Get-Content (Join-Path $pipelineRoot 'publish_rest_function_key.yaml') -Raw
$match = [regex]::Match($readinessYaml, '(?ms)      inlineScript: \|\r?\n(?<body>(?: {8}[^\r\n]*\r?\n)+)')
if (-not $match.Success -or $match.Index -gt $readinessYaml.IndexOf('- task: AzureResourceManagerTemplateDeployment@3')) {
    throw 'Readiness must run before key publication.'
}
$readiness = [regex]::Replace($match.Groups['body'].Value, '(?m)^ {8}', '')
# Execute the exact pipeline loop without network calls or real delays.
foreach ($succeedOn in @(1, 3, 0)) {
    & {
        param($body, $successAttempt)
        $script:requests = 0
        $script:delays = 0
        function az { $global:LASTEXITCODE = 0; return 'fixture.invalid' }
        function Invoke-WebRequest {
            param($Uri, [switch]$UseBasicParsing, $TimeoutSec)
            if ($TimeoutSec -ne 10 -or $Uri -ne 'https://fixture.invalid/api/heartbeat') { throw 'Unexpected probe contract' }
            $script:requests++
            if ($script:requests -eq $successAttempt) { return @{ StatusCode = 200 } }
            throw 'Host is still starting'
        }
        function Start-Sleep { param($Seconds) if ($Seconds -ne 5) { throw 'Unexpected delay' }; $script:delays++ }
        $failed = $false
        try { & ([scriptblock]::Create($body)) } catch {
            if ($_.Exception.Message -ne 'REST host heartbeat did not become ready within 12 attempts.') { throw }
            $failed = $true
        }
        $expected = if ($successAttempt -eq 0) { 12 } else { $successAttempt }
        if ($script:requests -ne $expected -or $script:delays -ne ($expected - 1) -or $failed -ne ($successAttempt -eq 0)) {
            throw 'Readiness retry/termination contract failed.'
        }
    } $readiness $succeedOn
}
Write-Output 'Passed: deployment order, legacy defaults and readiness immediate/retry/exhaustion checks.'
