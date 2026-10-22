param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [string]$WebhookUrl,

    [Parameter(Mandatory = $true)]
    [string]$RunUrl
)

$summaryFiles = @(Get-ChildItem -LiteralPath $ArtifactRoot -Recurse -Filter "run-summary.json" -File -ErrorAction SilentlyContinue)
$rows = @($summaryFiles | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json })
$bookings = @($rows | ForEach-Object { @($_.bookings) })
$failed = @($rows | ForEach-Object { @($_.failures) })

$status = if ($rows.Count -eq 0 -or $failed.Count -gt 0 -or @($rows | Where-Object { $_.failed -gt 0 -or $_.passed -eq 0 }).Count -gt 0) {
    "Fail"
}
else {
    "Pass"
}

$tableRows = if ($bookings.Count -gt 0) {
    @($bookings | ForEach-Object {
        $paymentReferenceId = if ([string]::IsNullOrWhiteSpace($_.paymentReferenceId) -or $_.paymentReferenceId -eq "Not captured") { "Not captured" } else { $_.paymentReferenceId }
        "| Digital First | VAA TEST 2 (SIT) | Revenue Booking | $status | $($_.bookingReference) | $paymentReferenceId |"
    })
}
else {
    @("| Digital First | VAA TEST 2 (SIT) | Revenue Booking | $status | Not available | Not available |")
}

$message = @(
    "## Daily Sanity Test",
    "",
    "| Team | Test Environment | Booking Type | Status | PNR | Payment Reference ID |",
    "| --- | --- | --- | --- | --- | --- |",
    $tableRows,
    "",
    "[View GitHub Actions run]($RunUrl)"
) -join "`n"

$payload = @{ text = $message } | ConvertTo-Json -Depth 4
Invoke-RestMethod -Method Post -Uri $WebhookUrl -ContentType "application/json" -Body $payload -TimeoutSec 30 -ErrorAction Stop | Out-Null

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    $message | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Encoding utf8 -Append
}
