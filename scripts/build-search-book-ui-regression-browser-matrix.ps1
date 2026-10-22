param(
    [Parameter(Mandatory = $true)]
    [string]$TestAssembly,
    [string]$TestFilter = "Category=SearchAndBook-UI-Regression",
    [int]$Seed = 1,
    [ValidateSet('all', 'all-each', 'windows-chrome', 'windows-edge', 'mac-safari')]
    [string]$BrowserProfile = 'all'
)

$ErrorActionPreference = "Stop"

if ($TestFilter -eq "Category=SearchAndBook-UI-Regression") {
    $TestFilter = "$TestFilter&Category!=SearchAndBook-UI-Validation-NoSmoke"
}

$listOutput = & dotnet test $TestAssembly --list-tests --filter $TestFilter 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Unable to discover Search & Book UI tests.`n$($listOutput -join [Environment]::NewLine)"
}

$tests = @($listOutput |
    ForEach-Object { $_.ToString().Trim() } |
    Where-Object { $_ -match '^VAA\.EAPI\.Automation\.SearchAndBook\.UI\.Tests\.' } |
    ForEach-Object { $_ -replace '\(.*$', '' } |
    Sort-Object -Unique)

if ($tests.Count -eq 0) {
    throw "No tests were discovered for filter '$TestFilter'."
}

$random = [Random]::new($Seed)
for ($index = $tests.Count - 1; $index -gt 0; $index--) {
    $swapIndex = $random.Next($index + 1)
    $temporary = $tests[$index]
    $tests[$index] = $tests[$swapIndex]
    $tests[$swapIndex] = $temporary
}

$profiles = @(
    @{ profile = 'windows-chrome'; os = 'Windows'; os_version = '11'; browser = 'chrome'; browser_version = 'latest' },
    @{ profile = 'windows-edge'; os = 'Windows'; os_version = '11'; browser = 'edge'; browser_version = 'latest' },
    @{ profile = 'mac-safari'; os = 'OS X'; os_version = 'Tahoe'; browser = 'playwright-webkit'; browser_version = '' }
)
if ($BrowserProfile -notin @('all', 'all-each')) {
    $profiles = @($profiles | Where-Object profile -eq $BrowserProfile)
}

$assignments = if ($BrowserProfile -eq 'all-each') {
    foreach ($test in $tests) {
        foreach ($profile in $profiles) {
            @{ test = $test; profile = $profile }
        }
    }
} else {
    for ($index = 0; $index -lt $tests.Count; $index++) {
        @{ test = $tests[$index]; profile = $profiles[$index % $profiles.Count] }
    }
}

$profileIndexes = @{}
$include = foreach ($assignment in $assignments) {
    $test = $assignment.test
    $profile = $assignment.profile
    $profileKey = $profile.profile
    $currentIndex = if ($profileIndexes.ContainsKey($profileKey)) {
        [int]$profileIndexes[$profileKey]
    } else {
        0
    }
    $profileIndexes[$profileKey] = $currentIndex + 1
    [ordered]@{
        shard = "{0}-{1:D2}" -f $profile.profile, $profileIndexes[$profile.profile]
        profile = $profile.profile
        os = $profile.os
        os_version = $profile.os_version
        browser = $profile.browser
        browser_version = $profile.browser_version
        filter = "FullyQualifiedName=$test"
        test_name = $test.Split('.')[-1]
        test_count = 1
    }
}

$matrix = @{ include = @($include) } | ConvertTo-Json -Compress -Depth 5
if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_OUTPUT)) {
    "matrix=$matrix" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "count=$($tests.Count)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}

Write-Host "Assigned $($tests.Count) regression tests using seed ${Seed}:"
foreach ($profile in $profiles) {
    $assignedCount = @($include | Where-Object profile -eq $profile.profile).Count
    Write-Host "  $($profile.profile): $assignedCount test shards"
}

$matrix
