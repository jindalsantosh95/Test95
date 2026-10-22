#requires -Version 7.0
# Reconcile the public DEV GraphQL and Stitch WebSocket APIs with their HTTP backends.
# Does not change Function Apps, Key Vault permissions, existing policies or legacy routes.
[CmdletBinding()]
param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$apimBase = 'https://management.azure.com/subscriptions/aaef96b6-2082-4d05-b34a-e94b35addbde/resourceGroups/RG-VA-H-EUN-APIM-DEV-001/providers/Microsoft.ApiManagement/service/avaxeunamcag001'
$token = az account get-access-token --resource https://management.azure.com --query accessToken -o tsv
if ($LASTEXITCODE -ne 0 -or -not $token) { throw 'Unable to obtain Azure management credentials.' }
try
{
    $headers = @{ Authorization = "Bearer $token"; Accept = 'application/json' }
    foreach ($route in @(
        @{ HttpApi = 'enterprise-graph-ql-public-dev'; WebSocketApi = 'enterprise-graph-ql-websocket-public-dev'; DisplayName = 'Enterprise Graph QL WebSocket [Dev|Public]' },
        @{ HttpApi = 'eapi-graphql-stitch-dev'; WebSocketApi = 'eapi-graphql-stitch-websocket-dev'; DisplayName = 'EAPI GraphQL Stitch WebSocket [Dev|Public]' }))
    {
        $http = Invoke-RestMethod "$apimBase/apis/$($route.HttpApi)?api-version=2022-08-01" -Headers $headers
        $backend = [UriBuilder]::new($http.properties.serviceUrl)
        if ($backend.Scheme -ne 'https' -or $backend.Query -or $backend.UserName) { throw 'Expected an HTTPS backend without query credentials.' }
        $backend.Scheme = 'wss'
        $backend.Path = '/api/v1/GraphQL/ws'
        $path = $http.properties.path.TrimEnd('/') + '/ws'
        if (-not $path.StartsWith('dev/', [StringComparison]::OrdinalIgnoreCase)) { throw 'Only DEV routes may be configured.' }
        $uri = "$apimBase/apis/$($route.WebSocketApi)?api-version=2022-08-01"
        $existing = $null
        try { $existing = Invoke-RestMethod $uri -Headers $headers }
        catch { if ([int]$_.Exception.Response.StatusCode -ne 404) { throw } }
        if ($existing -and $existing.properties.type -ne 'websocket') { throw 'Refusing to replace a non-WebSocket API.' }
        $properties = if ($existing) { $existing.properties | ConvertTo-Json -Depth 20 | ConvertFrom-Json -AsHashtable } else { @{} }
        $properties.displayName = $route.DisplayName
        $properties.description = 'Public DEV subscriptions through the same backend as the corresponding HTTP API. GraphQL field authorization remains enforced.'
        $properties.type = 'websocket'
        $properties.serviceUrl = $backend.Uri.AbsoluteUri
        $properties.path = $path
        $properties.protocols = @('wss')
        $properties.subscriptionRequired = $http.properties.subscriptionRequired
        if ($http.properties.subscriptionKeyParameterNames) { $properties.subscriptionKeyParameterNames = $http.properties.subscriptionKeyParameterNames }
        Write-Output "$($route.WebSocketApi): $path -> $($backend.Uri.AbsoluteUri); subscriptionRequired=$($properties.subscriptionRequired)"
        if ($Apply)
        {
            $body = @{ properties = $properties } | ConvertTo-Json -Depth 20
            Invoke-RestMethod -Method Put -Uri $uri -Headers $headers -ContentType 'application/json' -Body $body | Out-Null
            $actual = Invoke-RestMethod $uri -Headers $headers
            if ($actual.properties.serviceUrl -ne $properties.serviceUrl -or $actual.properties.path -ne $path -or
                $actual.properties.subscriptionRequired -ne $properties.subscriptionRequired) { throw 'APIM route readback did not match the requested configuration.' }
            Write-Output "Verified $($route.WebSocketApi)."
        }
    }
}
finally { $token = $null; $headers = $null }
