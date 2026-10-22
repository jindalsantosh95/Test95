#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ScopesJson,
    [ValidateRange(1, 200)][int]$MaxJobs = 200
)

$ErrorActionPreference = 'Stop'
$scopes = @($ScopesJson | ConvertFrom-Json)
foreach ($scope in $scopes) {
    if ($scope.kind -notin @('SolutionPath', 'ProjectPath', 'ScopePath') -or
        [string]::IsNullOrWhiteSpace([string]$scope.path)) {
        throw 'Every verification scope must have a supported kind and a non-empty path.'
    }
}

# Keep every scope while bounding matrix expansion below GitHub's job limit.
$batchSize = [Math]::Max(1, [int][Math]::Ceiling($scopes.Count / [double]$MaxJobs))
$batches = @(
    for ($offset = 0; $offset -lt $scopes.Count; $offset += $batchSize) {
        $last = [Math]::Min($offset + $batchSize, $scopes.Count) - 1
        [PSCustomObject]@{
            batch = [int]($offset / $batchSize) + 1
            scopes = @($scopes[$offset..$last])
        }
    }
)
ConvertTo-Json -InputObject $batches -Depth 5 -Compress
