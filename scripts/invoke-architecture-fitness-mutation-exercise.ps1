#Requires -Version 5.1
[CmdletBinding()]
param([string]$RunId = "architecture-mutation-$([guid]::NewGuid().ToString('N'))")
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$rootRelative = "TestResults/architecture-fitness/$RunId"
$root = Join-Path $repoRoot $rootRelative
$checker = Join-Path $PSScriptRoot 'check-architecture-fitness.ps1'
New-Item -ItemType Directory -Path $root -Force | Out-Null

& $checker
if ($LASTEXITCODE -ne 0) { throw 'Baseline architecture graph must pass before mutation testing.' }

$fixtures = @(
    @{ name='missing-target'; nodes=@('VAA.EAPI.X/App.csproj'); edges=@(@{source='VAA.EAPI.X/App.csproj';target='VAA.EAPI.X/Missing.csproj'}) },
    @{ name='cycle'; nodes=@('VAA.EAPI.X/A.csproj','VAA.EAPI.X/B.csproj'); edges=@(@{source='VAA.EAPI.X/A.csproj';target='VAA.EAPI.X/B.csproj'},@{source='VAA.EAPI.X/B.csproj';target='VAA.EAPI.X/A.csproj'}) },
    @{ name='production-to-test'; nodes=@('VAA.EAPI.X/App.csproj','VAA.EAPI.X/App.Tests.csproj'); edges=@(@{source='VAA.EAPI.X/App.csproj';target='VAA.EAPI.X/App.Tests.csproj'}) },
    @{ name='production-to-automation'; nodes=@('VAA.EAPI.X/App.csproj','VAA.EAPI.Automation/Suite.csproj'); edges=@(@{source='VAA.EAPI.X/App.csproj';target='VAA.EAPI.Automation/Suite.csproj'}) },
    @{ name='runtime-to-deployment'; nodes=@('VAA.EAPI.X/App.csproj','VAA.EAPI.X/App.Deployment.csproj'); edges=@(@{source='VAA.EAPI.X/App.csproj';target='VAA.EAPI.X/App.Deployment.csproj'}) }
)
$results = @()
foreach ($fixture in $fixtures) {
    $path = Join-Path $root "$($fixture.name).json"
    $fixture | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $path -Encoding utf8
    & $checker -GraphFixturePath "$rootRelative/$($fixture.name).json"
    $results += [ordered]@{ mutation = $fixture.name; killed = $LASTEXITCODE -ne 0 }
}
$killed = @($results | Where-Object killed).Count
$evidence = [ordered]@{ schemaVersion=1; runId=$RunId; mutations=$results; killed=$killed; total=$results.Count; score=[math]::Round(($killed/$results.Count)*100,2); completedAtUtc=[datetime]::UtcNow.ToString('o') }
$evidence | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $root 'mutation-evidence.json') -Encoding utf8
if ($killed -ne $results.Count) { throw "Architecture mutation exercise survived $($results.Count-$killed) mutation(s)." }
Write-Host "architecture fitness mutation exercise passed ($killed/$($results.Count) killed): $rootRelative/mutation-evidence.json" -ForegroundColor Green
exit 0
