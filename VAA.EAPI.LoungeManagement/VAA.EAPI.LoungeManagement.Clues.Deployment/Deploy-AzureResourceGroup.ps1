#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupLocation,
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupName,
    [string] $TemplateFile = 'Template/azuredeploy.json',
    [string] $TemplateParametersFile = 'Parameters/dev-azuredeploy.parameters.json',
    [switch] $ValidateOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

$templatePath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateFile))
$parametersPath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateParametersFile))

if ($null -eq (Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue)) {
    New-AzResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Force | Out-Null
}

if ($ValidateOnly) {
    Test-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile $templatePath -TemplateParameterFile $parametersPath
    return
}

New-AzResourceGroupDeployment -Name "lounge-management-$((Get-Date).ToUniversalTime().ToString('MMdd-HHmm'))" -ResourceGroupName $ResourceGroupName -TemplateFile $templatePath -TemplateParameterFile $parametersPath -Force

