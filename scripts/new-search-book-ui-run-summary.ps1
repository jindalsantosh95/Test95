param(
    [Parameter(Mandatory = $true)]
    [string]$TrxPath,

    [Parameter(Mandatory = $true)]
    [string]$Profile,

    [Parameter(Mandatory = $true)]
    [string]$Scenario,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $false)]
    [string]$NetworkDiagnosticsPath = "",

    [Parameter(Mandatory = $false)]
    [string]$JobStatus = "unknown"
)

function Get-FailureBucket {
    param([string]$Text)

    $rules = [ordered]@{
        "configuration-secrets" = "RetrieveFromStorageTable|KeyVault|Key Vault|uriString|AzureStoragePnrCreationsConnectionString"
        "confirmation-processing" = "book/confirmation|payment did not confirm|paymentengine|payment processing|order creation|confirmation.*(?:422|500)"
        "summary-upgrade" = "summary-page cabin upgrade|summary upgrade|upgrade was not confirmed|Upgrade did not increase"
        "fare-selection" = "Fare options opened|stable Select button|selecting the requested carrier, cabin and fare"
        "seat-selection" = "seat map|seat-selection|seat selection|select seats|no advance action could be targeted"
        "passenger-submit" = "pax-submit|book/terms|passenger details|submitting passenger"
        "authentication-loyalty" = "loyalty|flying club|sign-in|login|b2c|unauthorized|forbidden"
        "hpp-payment" = "hpp|paymentFrame|card expander|cardholder offer|Synchrony|pay with points"
        "search-availability" = "flight results|no enabled priced fare|no availability|search-loader|flight card"
        "environment-network" = "timed out|timeout|connection refused|dns|service unavailable|bad gateway|gateway timeout|HTTP (?:502|503|504)"
        "automation" = "PlaywrightException|TargetClosedException|locator|selector|strict mode|element"
    }

    foreach ($rule in $rules.GetEnumerator()) {
        if ($Text -match $rule.Value) {
            return $rule.Key
        }
    }

    return "other"
}

function Get-FirstLine {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return "No error message captured."
    }

    $line = (($Text -split "`r?`n") | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1).Trim()
    if ($line.Length -gt 300) {
        return $line.Substring(0, 300) + "..."
    }

    return $line
}

function Get-FailureMessage {
    param(
        [string]$Text,
        [string]$EvidenceText = ""
    )

    $message = Get-FirstLine -Text $Text
    $evidence = if ([string]::IsNullOrWhiteSpace($EvidenceText)) { $Text } else { $EvidenceText }
    $innerFailure = [regex]::Matches(
        $evidence,
        "(?im)^\s*(?:-+>|-+)\s+(?<detail>[A-Za-z_][\w.]+(?:Exception|Error)\s*:.+)$") |
        Select-Object -Last 1
    if ($null -eq $innerFailure) {
        $innerFailure =
        [regex]::Matches(
            $evidence,
            "(?im)Inner failure:\s*(?<detail>[^\r\n]+)") |
            Select-Object -Last 1
    }
    if ($null -ne $innerFailure) {
        $innerFailure = $innerFailure.Groups["detail"].Value.Trim()
        $message = "$message Inner failure: $innerFailure"
    }

    return $message.Length -le 500 ? $message : $message.Substring(0, 500) + "..."
}

$trxFile = Get-Item -LiteralPath $TrxPath -ErrorAction SilentlyContinue
if ($null -eq $trxFile) {
    $summary = [ordered]@{
        profile = $Profile
        scenario = $Scenario
        jobStatus = $JobStatus
        total = 1
        passed = 0
        failed = 1
        skipped = 0
        duration = ""
        failures = @([ordered]@{
            test = $Scenario
            bucket = "workflow-setup"
            message = "No TRX result was produced. Review credential validation, restore and test-start steps."
        })
        bookings = @()
    }

    $directory = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $OutputPath -Encoding utf8
    $summary | ConvertTo-Json -Depth 6
    return
}

[xml]$trx = Get-Content -LiteralPath $trxFile.FullName -Raw
$results = @($trx.TestRun.Results.UnitTestResult)
$passed = @($results | Where-Object { $_.outcome -eq "Passed" }).Count
$failedResults = @($results | Where-Object { $_.outcome -in @("Failed", "Error", "Timeout") })
$skipped = @($results | Where-Object { $_.outcome -in @("NotExecuted", "Inconclusive", "Aborted") }).Count
$failed = $failedResults.Count
$total = $results.Count

$networkEvidence = ""
if (-not [string]::IsNullOrWhiteSpace($NetworkDiagnosticsPath) -and
    (Test-Path -LiteralPath $NetworkDiagnosticsPath)) {
    try {
        $diagnostics = @(Get-Content -LiteralPath $NetworkDiagnosticsPath -Raw | ConvertFrom-Json)
        $checkoutFailure = $diagnostics |
            Where-Object { $_.Status -ge 400 -and $_.Url -match "/flight-only/api/checkout/" } |
            Select-Object -Last 1
        if ($null -ne $checkoutFailure) {
            $endpoint = ([uri]$checkoutFailure.Url).AbsolutePath
            $body = ([string]$checkoutFailure.ErrorSummary -replace "\s+", " ").Trim()
            if ($body.Length -gt 350) {
                $body = $body.Substring(0, 350) + "..."
            }
            $networkEvidence = "$($checkoutFailure.Method) $endpoint -> HTTP $($checkoutFailure.Status)"
            if (-not [string]::IsNullOrWhiteSpace($body)) {
                $networkEvidence += ": $body"
            }
        }
    }
    catch {
        Write-Warning "Could not parse network diagnostics '$NetworkDiagnosticsPath': $($_.Exception.Message)"
    }
}

$failures = @($failedResults | ForEach-Object {
    $message = [string]$_.Output.ErrorInfo.Message
    $stackTrace = [string]$_.Output.ErrorInfo.StackTrace
    $stdOut = [string]$_.Output.StdOut
    $combined = "$message`n$stackTrace`n$stdOut`n$networkEvidence"
    [ordered]@{
        test = [string]$_.testName
        bucket = Get-FailureBucket -Text $combined
        message = Get-FailureMessage -Text $message -EvidenceText $combined
        evidence = $networkEvidence
    }
})

$bookings = @()
$standardOutput = @([string]$trx.TestRun.ResultSummary.Output.StdOut) + @($results | ForEach-Object { [string]$_.Output.StdOut })
foreach ($stdOut in $standardOutput) {
    foreach ($match in [regex]::Matches($stdOut, "\[confirmed-booking\]\s+PNR=([^;\r\n]+);\s*(?:surname=([^;\r\n]+);\s*)?(?:paymentReferenceId=([^;\r\n]+);\s*)?scenario=([^\r\n]+)", [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $bookings += [ordered]@{
            bookingReference = $match.Groups[1].Value.Trim()
            surname = $match.Groups[2].Value.Trim()
            paymentReferenceId = $match.Groups[3].Value.Trim()
            scenario = $match.Groups[4].Value.Trim()
        }
    }
}

$duration = [string]$trx.TestRun.ResultSummary.Counters.duration
if ([string]::IsNullOrWhiteSpace($duration)) {
    $start = [DateTimeOffset]::MinValue
    $finish = [DateTimeOffset]::MinValue
    if ([DateTimeOffset]::TryParse([string]$trx.TestRun.Times.start, [ref]$start) -and
        [DateTimeOffset]::TryParse([string]$trx.TestRun.Times.finish, [ref]$finish) -and
        $finish -ge $start) {
        $duration = ($finish - $start).ToString("c")
    }
}

$summary = [ordered]@{
    profile = $Profile
    scenario = $Scenario
    jobStatus = $JobStatus
    total = $total
    passed = $passed
    failed = $failed
    skipped = $skipped
    duration = $duration
    failures = $failures
    bookings = $bookings
}

$directory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($directory)) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}

$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $OutputPath -Encoding utf8
$summary | ConvertTo-Json -Depth 6
