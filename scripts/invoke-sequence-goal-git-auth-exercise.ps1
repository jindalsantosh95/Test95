#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'update-sequence-goal-document.ps1')
$root = Join-Path ([IO.Path]::GetTempPath()) "goal-auth-$([guid]::NewGuid().ToString('N'))"
$remote = Join-Path $root remote.git
$writer = Join-Path $root writer
New-Item -ItemType Directory $root | Out-Null
git init --bare $remote | Out-Host
git clone $remote $writer | Out-Host
git -C $writer config user.name test
git -C $writer config user.email test@example.invalid
$path = 'docs/sequence-diagrams/test.md'
New-Item -ItemType Directory (Join-Path $writer 'docs/sequence-diagrams') -Force | Out-Null
Set-Content (Join-Path $writer $path) '# Goal'
git -C $writer add .
git -C $writer commit -m initial | Out-Host
git -C $writer push origin HEAD:goal | Out-Host
# Fake credentials only. Native Git reads local and environment configuration;
# the wrapper applies Git's documented empty-value reset semantics to the list.
$global:goalAuthExercise = @{
    executable=(Get-Command git -CommandType Application | Select-Object -First 1).Source
    expected=''; operations=@(); rejectPush=$false
}
$names = @('GH_TOKEN','COORDINATOR_GH_TOKEN','GIT_CONFIG_COUNT','GIT_CONFIG_KEY_0','GIT_CONFIG_VALUE_0')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name) }
function git {
    $arguments = @($args)
    $network = @($arguments | Where-Object { $_ -in @('fetch','push','ls-remote') })
    if ($network.Count) {
        $prefix = @()
        for ($i=0; $i -lt $arguments.Count -and $arguments[$i] -in @('-c','-C'); $i+=2) { $prefix += $arguments[$i..($i+1)] }
        $values = @(& $global:goalAuthExercise.executable @prefix config --get-all http.https://github.com/.extraheader)
        if ($LASTEXITCODE -ne 0) { throw 'Missing effective Git authentication configuration.' }
        $effective = @()
        foreach ($value in $values) { if ($value -eq '') { $effective=@() } else { $effective += $value } }
        if ($effective.Count -ne 1 -or $effective[0] -cne $global:goalAuthExercise.expected) { throw 'Git operation has duplicate or incorrect authorization headers.' }
        $global:goalAuthExercise.operations += $network[0]
        if ($network[0] -eq 'push' -and $global:goalAuthExercise.rejectPush) { $global:LASTEXITCODE=1; return }
    }
    & $global:goalAuthExercise.executable @arguments
    $global:LASTEXITCODE=$LASTEXITCODE
}
Push-Location $writer
try {
    & $global:goalAuthExercise.executable config http.https://github.com/.extraheader 'AUTHORIZATION: basic checkout-fixture'
    $env:GIT_CONFIG_COUNT='1'
    $env:GIT_CONFIG_KEY_0='http.https://github.com/.extraheader'
    $env:GIT_CONFIG_VALUE_0='AUTHORIZATION: basic environment-fixture'
    foreach ($scenario in @('coordinator-token','fallback-token','rejected-push')) {
        $env:GH_TOKEN='target-fixture'
        $env:COORDINATOR_GH_TOKEN=if ($scenario -eq 'fallback-token') { $null } else { 'coordinator-fixture' }
        $token=if ($env:COORDINATOR_GH_TOKEN) { $env:COORDINATOR_GH_TOKEN } else { $env:GH_TOKEN }
        $global:goalAuthExercise.expected='AUTHORIZATION: basic '+[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("x-access-token:$token"))
        $global:goalAuthExercise.operations=@()
        $global:goalAuthExercise.rejectPush=$scenario -eq 'rejected-push'
        $rejected=$false
        try {
            Update-SequenceGoalDocument -RepositoryRoot $writer -Branch goal -DiagramPath $path -Message $scenario -Edit { param($document) Add-Content $document $scenario }
        } catch {
            if ($scenario -ne 'rejected-push' -or $_.Exception.Message -notlike 'Goal push failed without*') { throw }
            $rejected=$true
        }
        $expectedOperations=if ($rejected) { 'fetch,push,ls-remote' } else { 'fetch,push' }
        if (($global:goalAuthExercise.operations -join ',') -ne $expectedOperations) { throw 'Required authenticated Git operations were not exercised.' }
        $stored = & $global:goalAuthExercise.executable config --local --get-all http.https://github.com/.extraheader
        if ($stored -cne 'AUTHORIZATION: basic checkout-fixture' -or $env:GIT_CONFIG_VALUE_0 -cne 'AUTHORIZATION: basic environment-fixture') { throw 'Command-scoped authentication changed caller credentials.' }
        Write-Host "PASS: $scenario authenticates fetch/push/recovery without duplicate headers"
    }
    # Execute the actual workflow fetch with fake inputs and real Git. The QA
    # publisher has already removed its credentials at this point in production.
    $global:goalAuthExercise.rejectPush=$false
    & $global:goalAuthExercise.executable config --local --unset-all http.https://github.com/.extraheader
    $env:GIT_CONFIG_COUNT='0'
    $env:GH_TOKEN='qa-fixture'
    $global:goalAuthExercise.expected='AUTHORIZATION: basic '+[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('x-access-token:qa-fixture'))
    $workflow = Get-Content (Join-Path (Split-Path $PSScriptRoot -Parent) '.github/workflows/ai-delivery-sequence-goal-qa.yml') -Raw
    $step=[regex]::Match($workflow,'(?ms)- name: Pin published QA verification inputs.*?(?=^      - name:)').Value
    $fetch=[regex]::Match($step,'(?m)^          \$gitAuthHeader =.*\r?\n          git .* fetch --no-tags origin .*').Value -replace '(?m)^          ',''
    if (-not $fetch) { throw 'Missing authenticated QA verification fetch.' }
    $checkpoint=@{headBranch='goal'}
    & ([scriptblock]::Create($fetch))
    if ($LASTEXITCODE -ne 0) { throw 'QA verification fetch failed.' }
    $stored = @(& $global:goalAuthExercise.executable config --local --get-all http.https://github.com/.extraheader)
    if ($stored.Count -ne 0) { throw 'QA verification left credentials in the checkout.' }
    $global:LASTEXITCODE=0
    Write-Host 'PASS: actual QA verification fetch uses command-scoped credentials'
} finally {
    Pop-Location
    Remove-Item Function:git
    Remove-Variable goalAuthExercise -Scope Global
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name,$previous[$name]) }
}
