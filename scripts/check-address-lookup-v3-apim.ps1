param([string]$RepositoryRoot = (Split-Path $PSScriptRoot -Parent))

$ErrorActionPreference = 'Stop'
$graphqlDeployment = Join-Path $RepositoryRoot 'VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Deployment'
foreach ($file in Get-ChildItem (Join-Path $graphqlDeployment 'Template'), (Join-Path $graphqlDeployment 'Parameters') -Recurse -Filter '*.json') {
    if ((Get-Content $file.FullName -Raw) -match 'AddressLookupV3:ApimSubscriptionKey|GraphQL-AddressLookupV3-ApimSubscriptionKey') {
        throw "Retired v3 subscription setting remains in $($file.Name)."
    }
}
$deployment = Join-Path $RepositoryRoot 'VAA.EAPI.Deployments/VAA.EAPI.Microservices.Loyalty.AddressLookup.Deployment'
$template = Get-Content (Join-Path $deployment 'Template/azuredeploy_apim.json') -Raw | ConvertFrom-Json
$api = @($template.resources | Where-Object type -eq 'Microsoft.ApiManagement/service/apis')
if ($api.Count -ne 1 -or $api[0].properties.subscriptionRequired -ne $true) { throw 'Existing API subscription protection must remain enabled.' }
if (@($template.resources | Where-Object type -eq 'Microsoft.ApiManagement/service/namedValues').Count -ne 0) { throw 'Address APIM must not deploy v3 Key Vault named values.' }
$policy = @($template.resources | Where-Object type -eq 'Microsoft.ApiManagement/service/apis/policies')
if ($policy.Count -ne 1) { throw 'Explicitly replace the old signed-context policy; omission leaves it deployed in incremental mode.' }
[xml]$xml = $policy[0].properties.value
foreach ($stage in @('inbound', 'backend', 'outbound', 'on-error')) {
    $children = @($xml.policies.$stage.ChildNodes)
    if ($children.Count -ne 1 -or $children[0].Name -ne 'base') { throw "Expected only inherited v2 policy in $stage." }
}
foreach ($file in Get-ChildItem (Join-Path $deployment 'Template'), (Join-Path $deployment 'Parameters') -Recurse -Filter '*.json') {
    if ((Get-Content $file.FullName -Raw) -match 'AddressLookupV3ApimPolicy|ApimContext|X-Address-Lookup-V3-Context|context-signing-key|x-functions-key') {
        throw "Retired v3 authentication configuration remains in $($file.FullName)."
    }
}
foreach ($environment in @('dev', 'sit', 'uat', 'prod')) {
    $parameters = Get-Content (Join-Path $deployment "Parameters/$environment-azuredeploy-apim.parameters.json") -Raw | ConvertFrom-Json
    if (@($parameters.parameters.PSObject.Properties).Count -ne 0) { throw "Unexpected v3-specific APIM parameters in $environment." }
    Write-Host "${environment}: standard API subscription and inherited policy, no v3 APIM Key Vault dependency."
}
$program = Get-Content (Join-Path $RepositoryRoot 'VAA.EAPI.Microservices/VAA.EAPI.Microservices.Loyalty.AddressLookup/Program.cs') -Raw
if ($program -notmatch 'UseWhen<AuthenticationMiddleware>\(x => !Debugger.IsAttached\)' -or $program -match 'IsV3Function|IApimV3ContextValidator') {
    throw 'V3 must use the same authentication middleware as v2, with no v3 bypass.'
}
Write-Host 'Address Lookup v3 reuses v2 authentication; existing network restrictions are unchanged.'
