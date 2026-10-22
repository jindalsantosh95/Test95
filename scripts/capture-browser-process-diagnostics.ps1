[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SnapshotName,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [string]$BaselinePath
)

$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$capturedAtUtc = [DateTime]::UtcNow
$allProcesses = @(Get-CimInstance -ClassName Win32_Process -ErrorAction Stop)
$processById = @{}
foreach ($process in $allProcesses) {
    $processById[[int]$process.ProcessId] = $process
}

function Get-ProcessOwner([int]$processId) {
    try {
        $process = Get-Process -Id $processId -IncludeUserName -ErrorAction Stop
        return [string]$process.UserName
    }
    catch {
        return "unavailable"
    }
}

function Get-UserDataDirectory([string]$commandLine) {
    if ([string]::IsNullOrWhiteSpace($commandLine)) {
        return ""
    }

    $match = [regex]::Match($commandLine, '(?i)--user-data-dir(?:=|\s+)(?:"(?<quoted>[^"]+)"|(?<plain>\S+))')
    if (-not $match.Success) {
        return ""
    }

    if ($match.Groups["quoted"].Success) {
        return $match.Groups["quoted"].Value
    }

    return $match.Groups["plain"].Value
}

function Get-AutomationIndicator(
    [string]$processName,
    [string]$commandLine,
    [string]$parentName,
    [string]$parentCommandLine
) {
    $combinedCommandLine = "$commandLine $parentCommandLine"

    if ($processName -ieq "chromedriver.exe" -or $parentName -ieq "chromedriver.exe") {
        return "Selenium/ChromeDriver"
    }

    if ($combinedCommandLine -match '(?i)playwright|playwright_chromiumdev_profile') {
        return "Playwright"
    }

    if ($commandLine -match '(?i)--test-type=webdriver') {
        return "WebDriver"
    }

    return "Unclassified"
}

$baselineProcessIds = @()
if (-not [string]::IsNullOrWhiteSpace($BaselinePath) -and (Test-Path -LiteralPath $BaselinePath)) {
    $baselineProcessIds = @(Import-Csv -LiteralPath $BaselinePath | ForEach-Object { [int]$_.ProcessId })
}

$browserProcesses = @($allProcesses | Where-Object {
    $_.Name -in @("chrome.exe", "chromedriver.exe")
})

$rows = @($browserProcesses | ForEach-Object {
    $processId = [int]$_.ProcessId
    $parentProcessId = [int]$_.ParentProcessId
    $parent = $processById[$parentProcessId]
    $creationTimeUtc = if ($null -ne $_.CreationDate) {
        ([DateTime]$_.CreationDate).ToUniversalTime()
    }
    else {
        $null
    }
    $ageMinutes = if ($null -ne $creationTimeUtc) {
        [Math]::Round(($capturedAtUtc - $creationTimeUtc).TotalMinutes, 1)
    }
    else {
        $null
    }
    $commandLine = [string]$_.CommandLine
    $parentCommandLine = if ($null -ne $parent) { [string]$parent.CommandLine } else { "" }
    $parentName = if ($null -ne $parent) { [string]$parent.Name } else { "unavailable" }

    [pscustomobject]@{
        Snapshot = $SnapshotName
        CapturedAtUtc = $capturedAtUtc.ToString("o")
        ProcessId = $processId
        ParentProcessId = $parentProcessId
        Name = [string]$_.Name
        CreationTimeUtc = if ($null -ne $creationTimeUtc) { $creationTimeUtc.ToString("o") } else { "" }
        AgeMinutes = $ageMinutes
        Owner = Get-ProcessOwner $processId
        ExecutablePath = [string]$_.ExecutablePath
        UserDataDirectory = Get-UserDataDirectory $commandLine
        AutomationIndicator = Get-AutomationIndicator $_.Name $commandLine $parentName $parentCommandLine
        PresentInBaseline = $processId -in $baselineProcessIds
        ParentName = $parentName
        ParentOwner = if ($null -ne $parent) { Get-ProcessOwner $parentProcessId } else { "unavailable" }
        ParentCommandLine = $parentCommandLine
        CommandLine = $commandLine
    }
})

$snapshotPath = Join-Path $OutputDirectory "$SnapshotName.csv"
$rows | Export-Csv -LiteralPath $snapshotPath -NoTypeInformation -Encoding UTF8

$newProcessCount = @($rows | Where-Object { -not $_.PresentInBaseline }).Count
$oldestAge = $rows | Where-Object { $null -ne $_.AgeMinutes } | Measure-Object -Property AgeMinutes -Maximum
$oldestAgeText = if ($null -ne $oldestAge.Maximum) { "$($oldestAge.Maximum) minutes" } else { "unavailable" }

Write-Host "Browser process snapshot '$SnapshotName': $($rows.Count) process(es); $newProcessCount not present in baseline; oldest age $oldestAgeText."
$rows |
    Group-Object -Property AutomationIndicator |
    Sort-Object -Property Count -Descending |
    ForEach-Object { Write-Host "  $($_.Name): $($_.Count)" }

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    $summaryLines = @(
        "### Browser process diagnostics: $SnapshotName",
        "",
        "| Metric | Value |",
        "| --- | ---: |",
        "| Chrome/ChromeDriver processes | $($rows.Count) |",
        "| Not present in pre-test baseline | $newProcessCount |",
        "| Oldest process age | $oldestAgeText |",
        "",
        "The full PID, parent, owner, start time, profile and command-line snapshot is attached as a workflow artifact. No processes were stopped by this diagnostic step."
    )
    $summaryLines -join "`n" | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8 -Append
}

