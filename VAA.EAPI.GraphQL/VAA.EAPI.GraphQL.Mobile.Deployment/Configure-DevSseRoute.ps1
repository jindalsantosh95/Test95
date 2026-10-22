#requires -Version 7.0
# Preview by default. Add -Apply after deploying SSE support to both Function Apps.
[CmdletBinding()]
param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$apimBase = 'https://management.azure.com/subscriptions/aaef96b6-2082-4d05-b34a-e94b35addbde/resourceGroups/RG-VA-H-EUN-APIM-DEV-001/providers/Microsoft.ApiManagement/service/avaxeunamcag001'
$token = az account get-access-token --resource https://management.azure.com --query accessToken -o tsv
if ($LASTEXITCODE -ne 0 -or -not $token) { throw 'Unable to obtain Azure management credentials.' }
try
{
    $headers = @{ Authorization = "Bearer $token" }
    foreach ($api in @('enterprise-graph-ql-public-dev', 'eapi-graphql-stitch-dev'))
    {
        $http = Invoke-RestMethod "$apimBase/apis/$($api)?api-version=2022-08-01" -Headers $headers
        $backend = [UriBuilder]::new($http.properties.serviceUrl)
        if ($backend.Scheme -ne 'https' -or $backend.Query -or $backend.UserName -or
            -not $http.properties.path.StartsWith('dev/', [StringComparison]::OrdinalIgnoreCase))
        {
            throw 'Expected a DEV HTTPS backend without query credentials.'
        }
        $backend.Path = '/api/v1/GraphQL/sse'
        $operationUri = "$apimBase/apis/$api/operations/graphql-sse"
        $operation = @{ properties = @{ displayName = 'GraphQL SSE subscriptions'; method = 'POST'; urlTemplate = '/sse'; templateParameters = @(); responses = @() } }
        $existing = $null
        try { $existing = Invoke-RestMethod "$($operationUri)?api-version=2022-08-01" -Headers $headers }
        catch { if ([int]$_.Exception.Response.StatusCode -ne 404) { throw } }
        if ($existing -and ($existing.properties.method -ne 'POST' -or $existing.properties.urlTemplate -ne '/sse'))
        {
            throw 'Refusing to replace a different operation.'
        }
        $authority = [Security.SecurityElement]::Escape($backend.Uri.GetLeftPart([UriPartial]::Authority))
        $policy = @"
<policies>
  <inbound>
    <base />
    <set-backend-service base-url="$authority" />
    <rewrite-uri template="/api/v1/GraphQL/sse" copy-unmatched-params="false" />
  </inbound>
  <backend>
    <forward-request timeout="120" buffer-response="false" />
  </backend>
  <outbound><base /></outbound>
  <on-error><base /></on-error>
</policies>
"@
        $policyUri = "$operationUri/policies/policy?api-version=2022-08-01"
        try
        {
            $existingPolicy = Invoke-RestMethod $policyUri -Headers $headers
            if (([xml]$existingPolicy.properties.value).OuterXml -ne ([xml]$policy).OuterXml)
            {
                throw 'Existing SSE operation policy differs. Review it explicitly before replacing it.'
            }
        }
        catch { if ([int]$_.Exception.Response.StatusCode -ne 404) { throw } }
        Write-Output "POST $($http.properties.path)/sse -> $($backend.Uri.AbsoluteUri), response buffering disabled."
        if ($Apply)
        {
            Invoke-RestMethod -Method Put -Uri "$($operationUri)?api-version=2022-08-01" -Headers $headers -ContentType application/json -Body ($operation | ConvertTo-Json -Depth 10) | Out-Null
            $body = @{ properties = @{ format = 'rawxml'; value = $policy } } | ConvertTo-Json -Depth 10
            Invoke-RestMethod -Method Put -Uri $policyUri -Headers $headers -ContentType application/json -Body $body | Out-Null
            $actual = Invoke-RestMethod $policyUri -Headers $headers
            if (([xml]$actual.properties.value).OuterXml -ne ([xml]$policy).OuterXml) { throw 'SSE policy readback differed.' }
            Write-Output "Verified $api SSE policy."
        }
    }
}
finally { $token = $null; $headers = $null }
