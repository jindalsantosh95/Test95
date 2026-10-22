[CmdletBinding()]
param([Parameter(Mandatory)][string]$PublishPath)

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'test-rest-function-key-order.ps1')
$repo = Split-Path -Parent $PSScriptRoot
$pipeline = Get-Content -LiteralPath (Join-Path $repo 'VAA.EAPI.Pipelines.DigitalServices/functions_digital/Graphql_Rest_Gateway/build_graphqlrest.yaml') -Raw
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('rest-hook-package-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$fixture/merge/hostextensions", "$fixture/Parameters" -Force | Out-Null

function Invoke-PipelineScript([string]$DisplayName) {
    $pattern = "(?ms)displayName: '" + [regex]::Escape($DisplayName) + "'\r?\n.*?script: \|\r?\n(?<body>(?: {16}[^\r\n]*\r?\n)+)"
    $match = [regex]::Match($pipeline, $pattern)
    if (-not $match.Success) { throw "Cannot find pipeline script: $DisplayName" }
    $body = [regex]::Replace($match.Groups['body'].Value, '(?m)^ {16}', '')
    $body = $body.Replace('$(Build.ArtifactStagingDirectory)', $fixture)
    & ([scriptblock]::Create($body))
}

$rejected = $false
try { Invoke-PipelineScript 'Verify REST startup hook package' }
catch {
    if ($_.Exception.Message -notlike '*missing the native WebSocket startup hook*') { throw }
    $rejected = $true
}
if (-not $rejected) { throw 'Missing startup hook was accepted.' }

$invalidHook = "$fixture/merge/hostextensions/VAA.EAPI.GraphQL.WebSocketHost.dll"
Set-Content -LiteralPath $invalidHook -Value 'Not a managed assembly.'
$rejected = $false
try { Invoke-PipelineScript 'Verify REST startup hook package' }
catch {
    if ($_.Exception.GetBaseException() -isnot [System.BadImageFormatException]) { throw }
    $rejected = $true
}
if (-not $rejected) { throw 'Invalid startup hook was accepted.' }

$publishedHook = Join-Path $PublishPath 'hostextensions/VAA.EAPI.GraphQL.WebSocketHost.dll'
Copy-Item -LiteralPath $publishedHook -Destination "$fixture/merge/hostextensions/"
Invoke-PipelineScript 'Verify REST startup hook package'

$source = Join-Path $repo 'VAA.EAPI.GraphQL/VAA.EAPI.GraphQL.Deployment/Parameters/dev-digital-azuredeploy.parameters.json'
$sourceHash = (Get-FileHash -LiteralPath $source).Hash
$destination = "$fixture/Parameters/dev-digital-azuredeploy.parameters.json"
Copy-Item -LiteralPath $source -Destination $destination
$expected = Get-Content -LiteralPath $source -Raw | ConvertFrom-Json
$expected.parameters.NativeWebSocketsConfiguration.value.startupHookPath = 'C:\home\site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll'
Invoke-PipelineScript 'Set DEV REST startup hook content path'
$actual = Get-Content -LiteralPath $destination -Raw | ConvertFrom-Json
if (($actual | ConvertTo-Json -Depth 100 -Compress) -ne ($expected | ConvertTo-Json -Depth 100 -Compress)) {
    throw 'REST artifact changed settings other than the DEV hook path.'
}
if ((Get-FileHash -LiteralPath $source).Hash -ne $sourceHash) { throw 'Shared parameters were modified.' }
Write-Output 'Passed: missing and invalid DLLs rejected, published assembly accepted, DEV path corrected, all other settings and shared source preserved.'
Write-Output "Evidence directory: $fixture"
