param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,

    [string]$WebhookUrl,

    [Parameter(Mandatory = $true)]
    [string]$RunUrl,

    [Parameter(Mandatory = $false)]
    [string]$WorkflowName = "Search & Book UI BrowserStack smoke",

    [Parameter(Mandatory = $false)]
    [string]$TargetEnvironment = "stg",
    [switch]$MobileReport,
    [int]$ExpectedTotal = 0,
    [string]$JobStatus = "unknown",
    [string]$PayloadPath,
    [string]$MobileCoverageText,
    [switch]$PreviewOnly
)

$summaryFiles = @(Get-ChildItem -LiteralPath $ArtifactRoot -Recurse -Filter "run-summary.json" -File -ErrorAction SilentlyContinue)
$rows = @($summaryFiles | ForEach-Object {
    try {
        Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
    }
    catch {
        Write-Warning "Could not parse '$($_.FullName)': $($_.Exception.Message)"
    }
} | Where-Object { $null -ne $_ })

$total = ($rows | Measure-Object -Property total -Sum).Sum
$passed = ($rows | Measure-Object -Property passed -Sum).Sum
$failed = ($rows | Measure-Object -Property failed -Sum).Sum
$skipped = ($rows | Measure-Object -Property skipped -Sum).Sum
foreach ($name in @("total", "passed", "failed", "skipped")) {
    if ($null -eq (Get-Variable -Name $name -ValueOnly)) {
        Set-Variable -Name $name -Value 0
    }
}

$passRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }
$statusEmoji = if ($failed -gt 0 -or $rows.Count -eq 0) { ":x:" } elseif ($skipped -gt 0) { ":warning:" } else { ":white_check_mark:" }
$statusText = if ($failed -gt 0 -or $rows.Count -eq 0) { "Failed" } elseif ($skipped -gt 0) { "Passed with skips" } else { "Passed" }
if ($MobileReport -and ($total -eq 0 -or $total -ne $ExpectedTotal -or $JobStatus -ne "success" -or $skipped -gt 0)) {
    $statusEmoji = ":warning:"
    $statusText = "Incomplete / requires review"
}
if ($failed -gt 0) { $statusEmoji = ":x:"; $statusText = "Failed" }

$failureRows = @($rows | ForEach-Object {
    $row = $_
    @($row.failures) | ForEach-Object {
        [pscustomobject]@{
            Bucket = $_.bucket
        }
    }
})

$bucketText = if ($failureRows.Count -eq 0) {
    "None"
}
else {
    (@($failureRows |
        Group-Object Bucket |
        Sort-Object Count -Descending |
        ForEach-Object { "- $($_.Name): $($_.Count)" }) -join "`n")
}

$bookingRows = @($rows | ForEach-Object { @($_.bookings) })
$bookingText = if ($bookingRows.Count -eq 0) {
    "None reported"
}
else {
    (@($bookingRows |
        Select-Object -First 8 |
        ForEach-Object {
            $surnameText = if ([string]::IsNullOrWhiteSpace($_.surname)) { "" } else { " / $($_.surname)" }
            "- $($_.bookingReference)$surnameText / $($_.scenario)"
        }) -join "`n")
}

$blocks = @(
    @{
        type = "section"
        text = @{ type = "mrkdwn"; text = "$statusEmoji *$WorkflowName* - $statusText (Env: $TargetEnvironment)" }
    },
    @{
        type = "section"
        fields = @(
            @{ type = "mrkdwn"; text = "*Total:*`n$total" },
            @{ type = "mrkdwn"; text = "*Passed:*`n$passed" },
            @{ type = "mrkdwn"; text = "*Failed:*`n$failed" },
            @{ type = "mrkdwn"; text = "*Skipped:*`n$skipped" },
            @{ type = "mrkdwn"; text = "*Pass rate:*`n$passRate%" },
            @{ type = "mrkdwn"; text = "*Shards:*`n$($rows.Count)" }
        )
    }
)

if ($failed -gt 0 -or $rows.Count -eq 0) {
    $blocks += @{
        type = "section"
        text = @{ type = "mrkdwn"; text = "*Failure buckets:*`n$bucketText" }
    }
}

$blocks += @{
    type = "section"
    text = @{ type = "mrkdwn"; text = "*Confirmed bookings:*`n$bookingText" }
}
if ($MobileReport) {
    if ($MobileCoverageText) {
        $blocks += @{ type = 'section'; text = @{ type = 'plain_text'; text = $MobileCoverageText } }
        $blocks += @{ type = 'section'; text = @{ type = 'mrkdwn'; text = '<https://github.com/VAAD1ST/eapi/blob/main/VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI/E2E-COVERAGE-MATRIX.md|View user-facing component coverage matrix>' } }
    }
    $names = @($rows | ForEach-Object { @($_.failures) } | ForEach-Object { $_.name } | Where-Object { $_ } | Select-Object -First 10)
    $failureNames = if ($names.Count) { ($names | ForEach-Object { "- $_" }) -join "`n" } else { "None recorded; inspect workflow if incomplete." }
    $blocks += @{
        type = "section"
        text = @{ type = "mrkdwn"; text = "*Expected tests:* $ExpectedTotal | *Job:* $JobStatus`n*Failed scenarios (up to 10):*`n$failureNames" }
    }
}
$blocks += @{
    type = "actions"
    elements = @(
        @{ type = "button"; text = @{ type = "plain_text"; text = "View workflow run" }; url = $RunUrl }
    )
}
$blocks += @{
    type = "context"
    elements = @(
        @{ type = "mrkdwn"; text = "Detailed results: <$RunUrl|View workflow run in GitHub Actions>" }
    )
}

$payload = @{
    text = "$statusEmoji $WorkflowName ($statusText)"
    blocks = $blocks
} | ConvertTo-Json -Depth 8

if ($PayloadPath) { $payload | Set-Content -LiteralPath $PayloadPath -Encoding utf8 }
if (-not $PreviewOnly) {
    try {
        if ([string]::IsNullOrWhiteSpace($WebhookUrl)) { throw "Slack webhook is not configured." }
        Invoke-RestMethod -Method Post -Uri $WebhookUrl -ContentType "application/json" -Body $payload -TimeoutSec 30 -ErrorAction Stop | Out-Null
    }
    catch {
        if ($MobileReport) { throw "Slack notification failed; webhook and response details withheld." }
        Write-Warning "Optional Slack notification unavailable; webhook and response details withheld."
    }
}

# Preview fixtures may write their requested payload file, but must not publish
# synthetic results to either Slack or the live GitHub job summary.
if (-not $PreviewOnly -and -not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    @(
        "### $WorkflowName",
        "",
        "| Total | Passed | Failed | Skipped | Pass rate | Shards |",
        "| ---: | ---: | ---: | ---: | ---: | ---: |",
        "| $total | $passed | $failed | $skipped | $passRate% | $($rows.Count) |",
        "",
        "#### Failure buckets",
        $bucketText,
        "",
        "#### Confirmed bookings`n$bookingText",
        $(if ($MobileReport) { "#### Failed scenarios`n$failureNames`nExpected: $ExpectedTotal; status: $statusText" }),
        $(if ($MobileReport -and $MobileCoverageText) { "#### iOS source mapping`n$MobileCoverageText" }),
        "",
        "[View workflow run]($RunUrl)"
    ) -join "`n" | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8 -Append
}
