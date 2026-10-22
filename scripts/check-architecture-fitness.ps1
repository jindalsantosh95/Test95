#Requires -Version 5.1
[CmdletBinding()]
param([string]$GraphFixturePath)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$failures = [System.Collections.Generic.List[string]]::new()

function Normalize([string]$Path) { return ($Path -replace '\\','/').TrimStart('./') }
function Is-Test([string]$Path) { return $Path -match '(?i)(^|/)[^/]*Tests?(/|\.)|\.Tests?\.csproj$' }
function Is-Automation([string]$Path) { return $Path -match '^(?i)VAA\.EAPI\.Automation/' }
function Is-Deployment([string]$Path) { return $Path -match '(?i)(^|/)[^/]*Deployment[^/]*\.csproj$|(^|/)VAA\.EAPI\.Deployments/' }

$nodes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$edges = [System.Collections.Generic.List[object]]::new()
$exceptions = @()

if (-not [string]::IsNullOrWhiteSpace($GraphFixturePath)) {
    $fixtureFull = if ([System.IO.Path]::IsPathRooted($GraphFixturePath)) { $GraphFixturePath } else { Join-Path $repoRoot $GraphFixturePath }
    $fixture = Get-Content -LiteralPath $fixtureFull -Raw | ConvertFrom-Json
    foreach ($node in @($fixture.nodes)) { [void]$nodes.Add((Normalize ([string]$node))) }
    foreach ($edge in @($fixture.edges)) { $edges.Add([pscustomobject]@{ source = Normalize ([string]$edge.source); target = Normalize ([string]$edge.target) }) }
} else {
    $inventoryPath = Join-Path $repoRoot 'docs/architecture-fitness-scopes.json'
    $inventory = Get-Content -LiteralPath $inventoryPath -Raw | ConvertFrom-Json
    $exceptions = @($inventory.referenceExceptions)
    if ($inventory.schemaVersion -ne 1 -or @($inventory.scopes).Count -eq 0) { $failures.Add('Architecture fitness scope inventory is invalid or empty.') }
    foreach ($solution in @($inventory.scopes)) {
        $solutionFull = Join-Path $repoRoot ([string]$solution)
        if (-not (Test-Path -LiteralPath $solutionFull -PathType Leaf)) { $failures.Add("Active solution is missing: $solution"); continue }
        $solutionDir = Split-Path -Parent $solutionFull
        $solutionText = Get-Content -LiteralPath $solutionFull -Raw
        $projectMatches = @([regex]::Matches($solutionText, '"(?<path>[^"\r\n]+\.(csproj|deployproj|vcxproj|fsproj))"'))
        if ($projectMatches.Count -eq 0) { $failures.Add("Active solution has no projects: $solution") }
        foreach ($match in $projectMatches) {
            $projectFull = [System.IO.Path]::GetFullPath((Join-Path $solutionDir ($match.Groups['path'].Value -replace '\\',[System.IO.Path]::DirectorySeparatorChar)))
            if (-not (Test-Path -LiteralPath $projectFull -PathType Leaf)) { $failures.Add("Solution '$solution' references missing project: $($match.Groups['path'].Value)") }
        }
    }

    $projectPaths = @(Get-ChildItem -LiteralPath $repoRoot -Recurse -File -Filter '*.csproj' |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
        Select-Object -ExpandProperty FullName)
    foreach ($projectFull in $projectPaths) {
        $full = [System.IO.Path]::GetFullPath($projectFull)
        $source = Normalize ($full.Substring($repoRoot.Length + 1))
        [void]$nodes.Add($source)
        try { [xml]$xml = Get-Content -LiteralPath $full -Raw }
        catch { $failures.Add("Unable to parse project: $source"); continue }
        foreach ($reference in @($xml.Project.ItemGroup.ProjectReference)) {
            if ($null -eq $reference -or [string]::IsNullOrWhiteSpace([string]$reference.Include)) { continue }
            $targetFull = [System.IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $full) ([string]$reference.Include)))
            $target = if ($targetFull.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) { Normalize ($targetFull.Substring($repoRoot.Length + 1)) } else { Normalize $targetFull }
            $edges.Add([pscustomobject]@{ source = $source; target = $target })
        }
    }
}

foreach ($edge in $edges) {
    if (-not $nodes.Contains([string]$edge.source)) { $failures.Add("Graph edge source is missing: $($edge.source)") }
    if (-not $nodes.Contains([string]$edge.target)) { $failures.Add("Project reference target is missing: $($edge.source) -> $($edge.target)") }
    $testException = @($exceptions | Where-Object { $_.rule -eq 'production-does-not-reference-tests' -and (Normalize ([string]$_.source)) -eq $edge.source -and (Normalize ([string]$_.target)) -eq $edge.target }).Count -gt 0
    if (-not (Is-Test $edge.source) -and (Is-Test $edge.target) -and -not $testException) { $failures.Add("Production project references test project: $($edge.source) -> $($edge.target)") }
    if (-not (Is-Automation $edge.source) -and (Is-Automation $edge.target)) { $failures.Add("Production project references Automation project: $($edge.source) -> $($edge.target)") }
    if (-not (Is-Deployment $edge.source) -and (Is-Deployment $edge.target)) { $failures.Add("Runtime project references deployment project: $($edge.source) -> $($edge.target)") }
}

$adjacency = @{}
foreach ($node in $nodes) { $adjacency[$node] = [System.Collections.Generic.List[string]]::new() }
foreach ($edge in $edges) { if ($adjacency.ContainsKey([string]$edge.source)) { $adjacency[[string]$edge.source].Add([string]$edge.target) } }
$color = @{}
$cycleFound = $false
function Visit([string]$Node) {
    if ($script:cycleFound) { return }
    if ($color[$Node] -eq 1) { $script:cycleFound = $true; return }
    if ($color[$Node] -eq 2) { return }
    $color[$Node] = 1
    foreach ($target in @($adjacency[$Node])) { if ($adjacency.ContainsKey($target)) { Visit $target } }
    $color[$Node] = 2
}
foreach ($node in $nodes) { if (-not $color.ContainsKey($node)) { Visit $node } }
if ($cycleFound) { $failures.Add('Project-reference graph contains a cycle.') }

if ($failures.Count -gt 0) {
    Write-Host 'architecture fitness check failed:' -ForegroundColor Red
    $failures | Select-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "architecture fitness check passed ($($nodes.Count) projects; $($edges.Count) references)" -ForegroundColor Green
exit 0
