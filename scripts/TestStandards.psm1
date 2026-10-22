#Requires -Version 5.1

function Test-ContainsMoqUsage {
    [CmdletBinding()]
    param([AllowEmptyString()][Parameter(Mandatory)][string]$Content)

    $moqNamespacePattern = '(?im)(?:^|[;\r\n])\s*(?:global\s+)?using\s+(?:(?:static\s+)|(?:[A-Za-z_]\w*\s*=\s*))?(?:global::)?Moq\b|(?:global::|\b)Moq\s*(?:\.|::)'
    $moqApiPattern = '(?i)\b(?:Mock|MockBehavior|It|Times)\s*(?:<|\.|\()'
    return $Content -match $moqNamespacePattern -or $Content -match $moqApiPattern
}

function Test-ContainsAdHocHttpConnectorUsage {
    [CmdletBinding()]
    param([AllowEmptyString()][Parameter(Mandatory)][string]$Content)

    $connectorImplementationPattern = '(?is)\bclass\s+\w+[^\r\n{]*:\s*[^\r\n{]*\bIHttpConnector\b'
    $instanceRegistrationPattern = '(?is)\.AddSingleton\s*<\s*IHttpConnector\s*>\s*\('
    return $Content -match $connectorImplementationPattern -or $Content -match $instanceRegistrationPattern
}

function Test-ContainsSharedHttpTestPipeline {
    [CmdletBinding()]
    param([AllowEmptyString()][Parameter(Mandatory)][string]$Content)

    return $Content -match '(?is)AddSingleton\s*<\s*IHttpClientFactory\s*,[^>]*HttpClientFactory\s*>' -and
        $Content -match '(?is)AddSingleton\s*<\s*IHttpConnector\s*,\s*HttpConnector\s*>'
}

Export-ModuleMember -Function Test-ContainsMoqUsage, Test-ContainsAdHocHttpConnectorUsage, Test-ContainsSharedHttpTestPipeline
