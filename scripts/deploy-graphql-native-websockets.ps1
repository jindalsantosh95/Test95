#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Prepare', 'Activate')][string]$Phase,
    [Parameter(Mandatory)][ValidateSet('dev')][string]$Environment,
    [Parameter(Mandatory)][string]$SubscriptionId,
    [Parameter(Mandatory)][string]$ResourceGroupName,
    [Parameter(Mandatory)][string]$AppName,
    [Parameter(Mandatory)][string]$ParameterFile,
    [Parameter(Mandatory)][string]$PreparedParameterFile,
    [string]$PackagePattern
)

$ErrorActionPreference = 'Stop'
$hookName = 'VAA.EAPI.GraphQL.WebSocketHost.dll'
$apiVersion = '2022-03-01'
$resource = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/sites/$AppName"
$token = az account get-access-token --subscription $SubscriptionId --resource https://management.azure.com/ --query accessToken -o tsv --only-show-errors
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($token)) { throw 'Cannot acquire deployment identity token.' }
$headers = @{ Authorization = "Bearer $token" }

function Invoke-Management([string]$Suffix, [string]$Method = 'GET', $Body = $null)
{
    $arguments = @{
        Uri = "${resource}${Suffix}?api-version=$apiVersion"
        Method = $Method
        Headers = $headers
        TimeoutSec = 90
    }
    if ($null -ne $Body)
    {
        $arguments.ContentType = 'application/json'
        $arguments.Body = ConvertTo-Json -InputObject $Body -Depth 50 -Compress
    }
    # Do not emit Azure error bodies: app settings can contain credentials.
    try { Invoke-RestMethod @arguments }
    catch { throw "Azure management request failed: $Method $Suffix. Check service connection permissions and resource availability." }
}

function Remove-NativeHook([string]$Hooks)
{
    (@($Hooks.Split(';', [StringSplitOptions]::RemoveEmptyEntries) | Where-Object {
        [IO.Path]::GetFileName($_.Trim()) -ine $hookName -and $_.Trim() -ine 'VAA.EAPI.GraphQL.WebSocketHost'
    }) -join ';')
}

function Test-WebSocket([uri]$Uri)
{
    $socket = [Net.WebSockets.ClientWebSocket]::new()
    $timeout = [Threading.CancellationTokenSource]::new([TimeSpan]::FromSeconds(20))
    try
    {
        $socket.Options.AddSubProtocol('graphql-transport-ws')
        $socket.ConnectAsync($Uri, $timeout.Token).GetAwaiter().GetResult()
        $bytes = [Text.Encoding]::UTF8.GetBytes('{"type":"connection_init"}')
        $socket.SendAsync([ArraySegment[byte]]::new($bytes), [Net.WebSockets.WebSocketMessageType]::Text, $true, $timeout.Token).GetAwaiter().GetResult()
        $buffer = [byte[]]::new(4096)
        $message = [IO.MemoryStream]::new()
        try
        {
            do
            {
                $frame = $socket.ReceiveAsync([ArraySegment[byte]]::new($buffer), $timeout.Token).GetAwaiter().GetResult()
                if ($frame.MessageType -ne [Net.WebSockets.WebSocketMessageType]::Text) { throw 'Expected connection acknowledgement.' }
                $message.Write($buffer, 0, $frame.Count)
                if ($message.Length -gt 16384) { throw 'Oversized connection acknowledgement.' }
            } while (-not $frame.EndOfMessage)
            $reply = [Text.Encoding]::UTF8.GetString($message.ToArray()) | ConvertFrom-Json
            if ($reply.type -ne 'connection_ack') { throw 'Connection was not acknowledged.' }
        }
        finally { $message.Dispose() }
    }
    finally
    {
        $socket.Abort()
        $socket.Dispose()
        $timeout.Dispose()
    }
}

$parameters = Get-Content -LiteralPath $ParameterFile -Raw | ConvertFrom-Json -AsHashtable
$site = Invoke-Management ''
if ($site.properties.reserved) { throw 'This activation flow requires a Windows Function App.' }
$settings = (Invoke-Management '/config/appsettings/list' 'POST').properties

if ($Phase -eq 'Prepare')
{
    # ARM replaces app settings. Carry forward unrelated hooks before it runs.
    $combinedHooks = (Remove-NativeHook ([string]$settings.DOTNET_STARTUP_HOOKS)) + ';' +
        (Remove-NativeHook ([string]$parameters.parameters.NativeWebSocketsConfiguration.value.existingStartupHooks))
    $hooks = $combinedHooks.Split(';', [StringSplitOptions]::RemoveEmptyEntries) | Select-Object -Unique
    $parameters.parameters.NativeWebSocketsConfiguration.value.existingStartupHooks = $hooks -join ';'
    $parameters.parameters.NativeWebSocketsConfiguration.value.startupHookPath = ''
    # This temporary file contains deployment parameters, never a snapshot of live secrets.
    $parameters | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath $PreparedParameterFile -Encoding utf8
    Write-Host "Prepared DEV deployment for $AppName with the native startup hook disabled."
    return
}

$nativeConfiguration = $parameters.parameters.NativeWebSocketsConfiguration.value
if ([string]::IsNullOrWhiteSpace($settings.NativeWebSocketsRedisPrefix) -or
    [string]::IsNullOrWhiteSpace($settings.RedisCacheConnectionString))
{
    throw 'Deploy the Redis namespace and connection configuration before enabling scaled native subscriptions.'
}
if ((Remove-NativeHook ([string]$settings.DOTNET_STARTUP_HOOKS)) -ne [string]$settings.DOTNET_STARTUP_HOOKS) { throw 'Deploy the prepared parameters before activation.' }

# Capture the post-ARM baseline in memory for rollback. Never write app settings to disk/logs.
$baseline = $settings | ConvertTo-Json -Depth 30 | ConvertFrom-Json -AsHashtable
$baseline.DOTNET_STARTUP_HOOKS = Remove-NativeHook ([string]$baseline.DOTNET_STARTUP_HOOKS)
$changed = $false
try
{
    $packages = @(Get-ChildItem -Path $PackagePattern -File)
    if ($packages.Count -ne 1) { throw 'Expected exactly one Function deployment ZIP.' }
    $archive = [IO.Compression.ZipFile]::OpenRead($packages[0].FullName)
    try
    {
        $entry = $archive.GetEntry("hostextensions/$hookName")
        if ($null -eq $entry) { throw 'Deployment ZIP is missing the native startup hook.' }
        $stream = $entry.Open()
        try { $expectedHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) }
        finally { $stream.Dispose() }
    }
    finally { $archive.Dispose() }

    $scmHosts = @($site.properties.hostNameSslStates | Where-Object { $_.hostType -eq 'Repository' })
    if ($scmHosts.Count -ne 1) { throw 'Cannot identify the Function App SCM endpoint.' }
    # Resolve HOME on the target, not on the build agent. Windows PowerShell is independent of DOTNET_STARTUP_HOOKS.
    $remoteCommand = @'
$ErrorActionPreference = 'Stop'
$path = Join-Path $env:HOME 'site\wwwroot\hostextensions\VAA.EAPI.GraphQL.WebSocketHost.dll'
if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { exit 2 }
@{ path = (Get-Item -LiteralPath $path).FullName; hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash } | ConvertTo-Json -Compress
'@
    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($remoteCommand))
    $request = @{ command = "powershell.exe -NoProfile -NonInteractive -EncodedCommand $encoded"; dir = 'site\wwwroot' } | ConvertTo-Json
    $verified = $null
    for ($attempt = 1; $attempt -le 12; $attempt++)
    {
        try
        {
            $result = Invoke-RestMethod -Uri "https://$($scmHosts[0].name)/api/command" -Headers $headers -Method Post -ContentType 'application/json' -Body $request -TimeoutSec 30
            if ($result.ExitCode -eq 0)
            {
                $candidate = $result.Output | ConvertFrom-Json
                if ($candidate.hash -eq $expectedHash -and $candidate.path -match '^[A-Za-z]:\\' -and $candidate.path -notmatch '[;\r\n]')
                {
                    $verified = $candidate
                    break
                }
            }
        }
        catch { Write-Host 'Waiting for SCM access and the deployed package to become available.' }
        if ($attempt -lt 12) { Start-Sleep -Seconds 10 }
    }
    if ($null -eq $verified) { throw 'Could not verify the deployed hook path and package hash via SCM. Native transport remains disabled.' }

    $active = $baseline.Clone()
    $active.DOTNET_STARTUP_HOOKS = (@($baseline.DOTNET_STARTUP_HOOKS, $verified.path) | Where-Object { $_ }) -join ';'
    $active.FUNCTIONS_WORKER_PROCESS_COUNT = [string]$nativeConfiguration.workerProcessCount
    $changed = $true
    $null = Invoke-Management '/config/web' 'PATCH' @{ properties = @{ webSocketsEnabled = $true } }
    $null = Invoke-Management '/config/appsettings' 'PUT' @{ properties = $active }
    $null = Invoke-Management '/restart' 'POST'

    $uri = [uri]("wss://" + $site.properties.defaultHostName + $active.NativeWebSocketsPath)
    $acknowledged = $false
    for ($attempt = 1; $attempt -le 12; $attempt++)
    {
        try { Test-WebSocket $uri; $acknowledged = $true; break }
        catch { Write-Host "Waiting for WebSocket connection acknowledgement ($attempt/12)." }
        if ($attempt -lt 12) { Start-Sleep -Seconds 10 }
    }
    if (-not $acknowledged) { throw 'WebSocket handshake failed after activation.' }
    Write-Host "Verified hook $($verified.path) and graphql-transport-ws connection_ack on $AppName."
}
catch
{
    if ($changed)
    {
        $null = Invoke-Management '/config/appsettings' 'PUT' @{ properties = $baseline }
        $null = Invoke-Management '/config/web' 'PATCH' @{ properties = @{ webSocketsEnabled = $false } }
        $null = Invoke-Management '/restart' 'POST'
        Write-Warning 'Native transport disabled again; unrelated hooks and post-ARM settings restored. Deployment failed.'
    }
    throw
}
