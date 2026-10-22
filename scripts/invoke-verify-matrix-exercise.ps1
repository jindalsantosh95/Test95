#Requires -Version 5.1
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$builder = Join-Path $PSScriptRoot 'new-verify-matrix.ps1'
foreach ($count in @(0, 1, 200, 256, 309, 1000)) {
    $scopes = @(for ($index = 0; $index -lt $count; $index++) {
        [PSCustomObject]@{ kind = 'ProjectPath'; path = "Project $index/Project.csproj" }
    })
    $json = ConvertTo-Json -InputObject $scopes -Compress
    $matrixJson = & $builder -ScopesJson $json
    if (-not $matrixJson.StartsWith('[')) { throw 'Matrix must always be a JSON array.' }
    $batches = @($matrixJson | ConvertFrom-Json)
    $actual = @($batches | ForEach-Object { $_.scopes } | ForEach-Object { $_.path })
    $expected = @($scopes | ForEach-Object { $_.path })
    if ($batches.Count -gt 200 -or $actual.Count -ne $count -or
        ($actual -join "`n") -cne ($expected -join "`n")) {
        throw "Matrix lost, duplicated or reordered scopes, or exceeded the job bound: $count scopes."
    }
}
$rejected = $false
try { & $builder -ScopesJson '[{"kind":"Invalid","path":"."}]' } catch { $rejected = $true }
if (-not $rejected) { throw 'Invalid scope kind was accepted.' }
Write-Host 'verify matrix exercise passed (7 cases; all scopes retained exactly once)'
