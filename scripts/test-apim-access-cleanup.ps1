param([string]$RepositoryRoot = (Split-Path $PSScriptRoot -Parent))

$ErrorActionPreference = 'Stop'
$template = Get-Content (Join-Path $RepositoryRoot 'VAA.EAPI.Pipelines/functions/deployment_apim.yaml') -Raw
if ($template -notmatch 'cancelTimeoutInMinutes: 5' -or
    $template -notmatch "displayName: 'Restore APIM-only access \(always\)'\s+condition: always\(\)") {
    throw 'APIM cleanup must run on failure/cancellation with a cancellation grace period.'
}
if ($template -match 'Stop-AzWebApp|Start-AzWebApp|Restart-AzWebApp') {
    throw 'APIM publication must not change the application lifecycle.'
}
# Exercise the actual final inline task with local doubles only; never contact Azure.
$body = ($template -split 'Inline: \|')[-1] -replace '(?m)^              ', ''
$body = $body.Replace('${{parameters.resourceGroupName}}', 'test-group').Replace('${{parameters.functionAppName}}', 'test-app').Replace('${{parameters.APIM_IP}}', '192.0.2.1')
$cleanup = [scriptblock]::Create($body)

function Get-AzResource {
    [CmdletBinding()] param($ResourceType, $ResourceGroupName, $ResourceName, $ResourceId, $ApiVersion, [switch]$ExpandProperties)
    if ($ApiVersion -ne '2023-12-01' -or
        ($ResourceId -and $ResourceId -ne '/subscriptions/test/resourceGroups/test-group/providers/Microsoft.Web/sites/test-app/config/web') -or
        (!$ResourceId -and ($ResourceType -ne 'Microsoft.Web/sites/config' -or $ResourceName -ne 'test-app/web'))) {
        throw 'Read only the targeted web configuration, not the parent site.'
    }
    if ($script:scenario -eq 'read-fails') { throw 'Simulated config read error' }
    [pscustomobject]@{ ResourceId = '/subscriptions/test/resourceGroups/test-group/providers/Microsoft.Web/sites/test-app/config/web'; Properties = [pscustomobject]@{
        ipSecurityRestrictionsDefaultAction = $script:defaultAction
        ipSecurityRestrictions = @($script:rules)
        alwaysOn = $true
    } }
}
function Set-AzResource {
    [CmdletBinding()] param($ResourceId, $ApiVersion, $Properties, [switch]$UsePatchSemantics, [switch]$Force)
    if ($ResourceId -ne '/subscriptions/test/resourceGroups/test-group/providers/Microsoft.Web/sites/test-app/config/web' -or
        $ApiVersion -ne '2023-12-01' -or !$UsePatchSemantics -or $Properties.Count -ne 1 -or
        $Properties.ipSecurityRestrictionsDefaultAction -notin @('Allow', 'Deny')) {
        throw 'Write only the default action; never round-trip site state, settings or access rules.'
    }
    $script:events.Add($Properties.ipSecurityRestrictionsDefaultAction.ToLowerInvariant())
    if ($script:scenario -in @('deny-fails', 'allow-fails')) { throw 'Simulated control-plane error' }
    if ($script:scenario -ne 'deny-not-applied') { $script:defaultAction = $Properties.ipSecurityRestrictionsDefaultAction }
}
function Add-AzWebAppAccessRestrictionRule {
    [CmdletBinding()] param($ResourceGroupName, $WebAppName, $Name, $Priority, $Action, $IpAddress)
    if ($script:defaultAction -ne 'Deny') { throw 'APIM allow rule must be restored only after default deny.' }
    $script:events.Add('add')
    if ($script:scenario -eq 'add-fails') { throw 'Simulated rule write error' }
    if ($script:scenario -ne 'add-not-applied') {
        $script:rules = @([pscustomobject]@{ name = $Name; priority = $Priority; action = $Action; ipAddress = "$IpAddress/32" })
    }
}
foreach ($script:scenario in @('removed-rule', 'existing-rule', 'read-fails', 'deny-fails', 'deny-not-applied', 'add-fails', 'add-not-applied', 'invalid-rule')) {
    $script:defaultAction = 'Allow'
    $script:rules = @()
    $script:events = [System.Collections.Generic.List[string]]::new()
    if ($scenario -in @('existing-rule', 'invalid-rule')) {
        $script:rules = @([pscustomobject]@{ name = 'Allow_APIM'; priority = 1; action = 'Allow'; ipAddress = '192.0.2.1/32' })
        if ($scenario -eq 'invalid-rule') { $script:rules[0].ipAddress = '0.0.0.0/0' }
    }
    $failure = $null
    try { & $cleanup } catch { $failure = $_.Exception.Message }
    if ($scenario -in @('removed-rule', 'existing-rule')) {
        if ($failure -or $defaultAction -ne 'Deny') { throw "Unexpected cleanup failure: $scenario / $failure" }
        if ($scenario -eq 'existing-rule' -and $events.Contains('add')) { throw 'Cleanup must be idempotent when removal was skipped.' }
        & $cleanup
    }
    else {
        if (!$failure) { throw "Cleanup failure was hidden: $scenario" }
        if ($scenario -eq 'read-fails' -and $failure -ne 'Simulated config read error') { throw 'Preserve the original read failure.' }
        if ($scenario -eq 'deny-fails' -and $failure -ne 'Simulated control-plane error') { throw 'Preserve the original write failure.' }
        if ($scenario -eq 'add-fails' -and $failure -ne 'Simulated rule write error') { throw 'Preserve the original rule failure.' }
        if ($scenario -in @('add-fails', 'add-not-applied', 'invalid-rule') -and $defaultAction -ne 'Deny') {
            throw 'Rule restoration failures must retain default deny.'
        }
    }
    Write-Host "APIM cleanup scenario passed: $scenario"
}

# Exercise the existing temporary-Allow step too: it must not write app state or unrelated config.
$allowBody = (($template -split 'Inline: \|')[2] -split '\r?\n        # Task to publish')[0] -replace '(?m)^              ', ''
$allowBody = $allowBody.Replace('${{parameters.resourceGroupName}}', 'test-group').Replace('${{parameters.functionAppName}}', 'test-app')
$allow = [scriptblock]::Create($allowBody)
foreach ($script:scenario in @('allow', 'allow-fails')) {
    $script:defaultAction = 'Deny'
    $script:events = [System.Collections.Generic.List[string]]::new()
    $failure = $null
    try { & $allow } catch { $failure = $_.Exception.Message }
    if ($scenario -eq 'allow' -and ($failure -or $defaultAction -ne 'Allow')) { throw "Unexpected allow failure: $failure" }
    if ($scenario -eq 'allow-fails' -and ($failure -ne 'Simulated control-plane error' -or $defaultAction -ne 'Deny')) { throw 'Preserve the failed allow write and existing restrictions.' }
    Write-Host "APIM access scenario passed: $scenario"
}
Write-Host 'APIM access exercise passed: 10 offline scenarios; no Azure calls.'
