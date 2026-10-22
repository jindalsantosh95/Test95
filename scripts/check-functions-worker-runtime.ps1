#Requires -Version 5.1
[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$DeploymentPath)

$ErrorActionPreference = 'Stop'
$invalid = [Collections.Generic.List[string]]::new()

function Test-RuntimeValue {
    param($Value, [string]$File)
    # ARM parameter references are checked through their defaults and environment values.
    if ($Value -is [string] -and $Value -match "^\[parameters\('[^']*FUNCTIONS_WORKER_RUNTIME'\)\]$") { return }
    if ($Value -cne 'dotnet-isolated') {
        $invalid.Add("${File}: FUNCTIONS_WORKER_RUNTIME must be dotnet-isolated for .NET 10.")
    }
}

function Find-RuntimeSetting {
    param($Node, [string]$File)
    if ($null -eq $Node -or $Node -is [string] -or $Node -is [ValueType]) { return }
    if ($Node -is [System.Collections.IList]) {
        foreach ($item in $Node) { Find-RuntimeSetting $item $File }
        return
    }
    if ($Node.name -eq 'FUNCTIONS_WORKER_RUNTIME') { Test-RuntimeValue $Node.value $File }
    foreach ($key in $Node.Keys) {
        $property = @{ Name = $key; Value = $Node[$key] }
        if ($property.Name -match '(^|:)FUNCTIONS_WORKER_RUNTIME$') {
            if ($property.Value -is [string]) { Test-RuntimeValue $property.Value $File }
            else {
                foreach ($field in @('defaultValue', 'value')) {
                    if ($property.Value.Keys -contains $field) {
                        Test-RuntimeValue $property.Value.$field $File
                    }
                }
            }
        }
        Find-RuntimeSetting $property.Value $File
    }
}

foreach ($folder in @('Template', 'Parameters')) {
    $path = Join-Path $DeploymentPath $folder
    if (-not (Test-Path -LiteralPath $path)) { continue }
    foreach ($file in Get-ChildItem -LiteralPath $path -Filter '*.json' -File) {
        $json = Get-Content -LiteralPath $file.FullName -Raw
        if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('AsHashtable')) {
            $document = ConvertFrom-Json -InputObject $json -AsHashtable
        } else {
            # Windows PowerShell's PSCustomObject JSON reader rejects ARM keys differing only by case.
            Add-Type -AssemblyName System.Web.Extensions
            $reader = New-Object System.Web.Script.Serialization.JavaScriptSerializer
            $reader.MaxJsonLength = [int]::MaxValue
            $document = $reader.DeserializeObject($json)
        }
        Find-RuntimeSetting $document "$folder/$($file.Name)"
    }
}
if ($invalid.Count -gt 0) { throw ($invalid -join [Environment]::NewLine) }
Write-Host 'Functions worker runtime check passed' -ForegroundColor Green
