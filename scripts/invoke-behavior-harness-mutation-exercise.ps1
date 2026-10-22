#Requires -Version 5.1
[CmdletBinding()]
param([string]$RunId = "behavior-mutation-$([guid]::NewGuid().ToString('N'))")

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$rootRelative = "TestResults/behavior-harness/$RunId"
$root = Join-Path $repoRoot $rootRelative
$source = Get-Content -LiteralPath (Join-Path $repoRoot 'docs/behavior-harness.md') -Raw
$checker = Join-Path $PSScriptRoot 'check-behavior-harness.ps1'
New-Item -ItemType Directory -Path $root -Force | Out-Null

& $checker
if ($LASTEXITCODE -ne 0) { throw 'Baseline behavior harness must pass before mutation testing.' }
$mutations = @()

$missingCatalog = ($source -split "`r?`n" | Where-Object { $_ -notmatch '^\| `vaa-eapi-explore-all-graphql-scenarios`' }) -join [Environment]::NewLine
$missingCatalogPath = Join-Path $root 'missing-catalog.md'
$missingCatalog | Set-Content -LiteralPath $missingCatalogPath -Encoding utf8
& $checker -BehaviorHarnessPath "$rootRelative/missing-catalog.md"
$mutations += [ordered]@{ name = 'remove-complete-flow-from-catalog'; killed = $LASTEXITCODE -ne 0 }

$brokenSensor = $source.Replace('VAA.EAPI.Automation/VAA.EAPI.Automation.Explore/Tests/AppointmentBooker/SaveAppointmentBookingTests.cs','VAA.EAPI.Automation/MissingBehaviorSensor.cs')
$brokenSensorPath = Join-Path $root 'broken-sensor.md'
$brokenSensor | Set-Content -LiteralPath $brokenSensorPath -Encoding utf8
& $checker -BehaviorHarnessPath "$rootRelative/broken-sensor.md"
$mutations += [ordered]@{ name = 'replace-sensor-with-missing-file'; killed = $LASTEXITCODE -ne 0 }

$placeholder = $source.Replace('`VAA.EAPI.Automation/VAA.EAPI.Automation.Explore/Tests/AppointmentBooker/SaveAppointmentBookingTests.cs`','`Planned: placeholder`')
$placeholderPath = Join-Path $root 'placeholder-sensor.md'
$placeholder | Set-Content -LiteralPath $placeholderPath -Encoding utf8
& $checker -BehaviorHarnessPath "$rootRelative/placeholder-sensor.md"
$mutations += [ordered]@{ name = 'replace-sensor-with-placeholder'; killed = $LASTEXITCODE -ne 0 }

$killed = @($mutations | Where-Object killed).Count
$evidence = [ordered]@{ schemaVersion = 1; runId = $RunId; mutations = $mutations; killed = $killed; total = $mutations.Count; score = [math]::Round(($killed / $mutations.Count) * 100, 2); completedAtUtc = [datetime]::UtcNow.ToString('o') }
$evidence | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $root 'mutation-evidence.json') -Encoding utf8
if ($killed -ne $mutations.Count) { throw "Behavior harness mutation exercise survived $($mutations.Count - $killed) mutation(s)." }
Write-Host "behavior harness mutation exercise passed ($killed/$($mutations.Count) killed): $rootRelative/mutation-evidence.json" -ForegroundColor Green
exit 0
