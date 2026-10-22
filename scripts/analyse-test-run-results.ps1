param(
    [Parameter(Mandatory = $false)]
    [string]$ResultsDirectory = "TestResults",

    [Parameter(Mandatory = $false)]
    [string]$TrxPath = "",

    [Parameter(Mandatory = $false)]
    [string]$SuiteName = "Test Run",

    [Parameter(Mandatory = $false)]
    [string]$EnvironmentName = "",

    [Parameter(Mandatory = $false)]
    [int]$Top = 10,

    [Parameter(Mandatory = $false)]
    [string]$TraceManifestPath = "",

    [Parameter(Mandatory = $false)]
    [string]$WorkflowRunUrl = ""
)

$AllowedTraceEnvironments = @("dev", "sit", "uat")

function Get-InferredSignal {
    param([string]$Text)

    if ($Text -match "(?:\[browserstack-failure\].*)?Application failure while") {
        return [pscustomobject]@{
            Label = "application failure"
            Owner = "Application/Downstream"
            Action = "Use the reported action, URL, checkout statuses, basket ID and correlation/request IDs to trace the failing application call."
        }
    }

    if ($Text -match "(?:\[browserstack-failure\].*)?Automation/technical failure while") {
        return [pscustomobject]@{
            Label = "browser automation or transport"
            Owner = "Automation/BrowserStack"
            Action = "Inspect the reported action, URL, browser session and network evidence before changing application assertions."
        }
    }

    $signals = @(
        [pscustomobject]@{
            Label = "response-shape/parser"
            Pattern = "RuntimeBinderException.*does not contain a definition|does not contain a definition for|Cannot perform runtime binding on a null reference|JsonReaderException|JsonSerializationException"
            Owner = "Automation"
            Action = "Inspect the raw response at the failing helper and replace brittle dynamic/path access with safe checks that print missing paths and payload excerpts."
        },
        [pscustomobject]@{
            Label = "null handling"
            Pattern = "NullReferenceException"
            Owner = "Automation"
            Action = "Add explicit null checks around the failing helper input and include the relevant request/response excerpt in the assertion message."
        },
        [pscustomobject]@{
            Label = "configuration or secret"
            Pattern = "Key Vault preflight failed|authKeyOrResourceToken|Vault\.Connect|KeyVaultUrl|local\.settings|configuration"
            Owner = "Environment/Config"
            Action = "Verify local settings, secret access, identity, and environment endpoint values."
        },
        [pscustomobject]@{
            Label = "auth or token"
            Pattern = "Required token '.+' was not available or was expired|GenerateLoginB2CToken|Account member SignIn failed|unauthorized|forbidden"
            Owner = "Environment/Config or Test Data"
            Action = "Refresh local tokens/test accounts and confirm configured environment access."
        },
        [pscustomobject]@{
            Label = "service availability"
            Pattern = "No Instance\(s\) Available|DNS|timed out|timeout|connection refused|StatusCode:\s*(NotFound|ServiceUnavailable|BadGateway|GatewayTimeout)|\b(404|503|502|504)\b"
            Owner = "Downstream/Environment"
            Action = "Verify environment routing/service health before changing automation."
        },
        [pscustomobject]@{
            Label = "HPP or UI"
            Pattern = "Failed to locate the Card expander|Card expander|Expected URL 'captured' was not loaded|HPP|HostedPayment"
            Owner = "HPP/Downstream or UI Automation"
            Action = "Capture browser/HPP artifacts and confirm redirect timing, page source, and selectors."
        },
        [pscustomobject]@{
            Label = "passenger or PTC"
            Pattern = "PASSENGER TYPE CODES ARE NOT EQUAL TO NUMBER IN PARTY BOOKED|DownstreamCode=2102|PTC|passenger type"
            Owner = "Test Data or Downstream"
            Action = "Compare scenario passenger counts with basket/order passenger diagnostics, ages, and PTC mapping."
        },
        [pscustomobject]@{
            Label = "downstream order"
            Pattern = "NEW PNR - SELECT END/REDISPLAY|DownstreamCode=9002|PNR or Error data is missing in BDS Response|NO RESPONSE TO DECREMENT MILES REQUEST|PAYMENTENGINE"
            Owner = "Downstream"
            Action = "Capture basket, transaction/correlation IDs, downstream codes, and hand to the owning service."
        },
        [pscustomobject]@{
            Label = "assertion"
            Pattern = "Assert\.|Assertion|Expected:|Actual:"
            Owner = "Automation or Product Behaviour"
            Action = "Inspect assertion inputs and include actual request/response evidence in the failure message."
        },
        [pscustomobject]@{
            Label = "GraphQL error"
            Pattern = "GraphQL errors|Error code was returned in GQL response|`"errors`""
            Owner = "GraphQL/Downstream or Automation"
            Action = "Inspect the GraphQL response body and extensions to separate expected product errors from automation assumptions."
        }
    )

    $matchedSignals = @()
    $owner = "Manual Review"
    $action = "Inspect the raw artifact around the failure and add stronger evidence capture if this mode repeats."

    foreach ($signal in $signals) {
        if ($Text -match $signal.Pattern) {
            $matchedSignals += $signal.Label
            if ($owner -eq "Manual Review") {
                $owner = $signal.Owner
                $action = $signal.Action
            }
        }
    }

    if ($matchedSignals.Count -eq 0) {
        $matchedSignals += "unclassified"
    }

    [pscustomobject]@{
        Label = ($matchedSignals -join ", ")
        Owner = $owner
        Action = $action
    }
}

function Get-FirstMatch {
    param(
        [string]$Text,
        [string[]]$Patterns
    )

    foreach ($pattern in $Patterns) {
        $match = [regex]::Match($Text, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($match.Success) {
            return $match.Groups[1].Value.Trim()
        }
    }

    return ""
}

function Get-TruncatedSingleLine {
    param(
        [string]$Text,
        [int]$MaxLength = 260
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    $singleLine = ([regex]::Replace($Text, "\s+", " ")).Trim()
    if ($singleLine.Length -gt $MaxLength) {
        return $singleLine.Substring(0, $MaxLength) + "..."
    }

    return $singleLine
}

function Get-AllMatches {
    param(
        [string]$Text,
        [string[]]$Patterns
    )

    $values = [System.Collections.Generic.List[string]]::new()
    foreach ($pattern in $Patterns) {
        foreach ($match in [regex]::Matches($Text, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            $value = $match.Groups[1].Value.Trim()
            if (-not [string]::IsNullOrWhiteSpace($value) -and -not $values.Contains($value)) {
                $values.Add($value)
            }
        }
    }

    return @($values)
}

function Get-TraceIdentifiers {
    param([string]$Text)

    $identifierPatterns = @(
        '(?i)(?:\\*["''])?\b(?:api[- ]?correlation[- ]?id|correlation(?:[- ]?id)?|error(?:[- ]?id)?|request(?:[- ]?id)?|operation(?:[- ]?id)?)\b(?:\\*["''])?\s*[:=\-]\s*(?:\\*["''])?([0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12})(?:\\*["''])?'
    )

    return @(Get-AllMatches -Text $Text -Patterns $identifierPatterns)
}

function Get-TraceOperationName {
    param([string]$Text)

    return Get-FirstMatch -Text $Text -Patterns @(
        '"operationName"\s*:\s*"([^"]+)"',
        'operationName[:=]\s*([A-Za-z0-9_.-]+)',
        '"data"\s*:\s*\{\s*"([A-Za-z0-9_]+)"',
        'GraphQL:(?:query|mutation)\s+([A-Za-z0-9_]+)'
    )
}

function Get-TraceIncidentKey {
    param(
        [string]$Text,
        [string]$OperationName,
        [string]$Signal,
        [string]$Message
    )

    $codes = @(Get-AllMatches -Text $Text -Patterns @(
        '"code"\s*:\s*"([^"]+)"',
        '\\?"code\\?"\s*:\s*\\?"([^"]+)\\?"'
    ))
    $errorSource = Get-FirstMatch -Text $Text -Patterns @(
        '"errorSource"\s*:\s*"([^"]+)"',
        '\\?"errorSource\\?"\s*:\s*\\?"([^"]+)\\?"'
    )
    $stableMessage = ($Message -split '(?i)responsepreview=')[0].Trim()
    $parts = @($OperationName, ($codes -join ","), $errorSource, $Signal, $stableMessage) |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    return (($parts -join " | ").ToLowerInvariant())
}

function Get-ScenarioName {
    param([string]$TestName)

    $match = [regex]::Match($TestName, "scenario:\s*([^\)\r\n]+)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($match.Success) {
        return $match.Groups[1].Value.Trim()
    }

    return $TestName
}

function Get-MessageFirstLine {
    param(
        [string]$Message,
        [string]$Combined
    )

    $structuredFailure = Get-FirstMatch -Text $Combined -Patterns @(
        "\[browserstack-failure\]\s*([^\r\n]+)"
    )
    if (-not [string]::IsNullOrWhiteSpace($structuredFailure)) {
        return Get-TruncatedSingleLine -Text $structuredFailure -MaxLength 1000
    }

    $graphQlErrorMessage = Get-FirstMatch -Text $Combined -Patterns @(
        '"message"\s*:\s*"([^"]+)"',
        "Search result is invalid:\s*(\{[\s\S]+)"
    )
    $graphQlErrorCode = Get-FirstMatch -Text $Combined -Patterns @(
        '"code"\s*:\s*"([^"]+)"',
        "extensions\.code[:=]\s*([A-Z0-9_.-]+)"
    )
    if (-not [string]::IsNullOrWhiteSpace($graphQlErrorMessage)) {
        if ($graphQlErrorMessage -match '^\{') {
            $nestedMessage = Get-FirstMatch -Text $graphQlErrorMessage -Patterns @('"message"\s*:\s*"([^"]+)"')
            if (-not [string]::IsNullOrWhiteSpace($nestedMessage)) {
                $graphQlErrorMessage = $nestedMessage
            }
        }

        $graphQlErrorMessage = Get-TruncatedSingleLine -Text $graphQlErrorMessage
        if (-not [string]::IsNullOrWhiteSpace($graphQlErrorCode)) {
            return "GraphQL error $graphQlErrorCode`: $graphQlErrorMessage"
        }

        return "GraphQL error: $graphQlErrorMessage"
    }

    if ([string]::IsNullOrWhiteSpace($Message)) {
        $exception = Get-FirstMatch -Text $Combined -Patterns @(
            "([A-Za-z0-9_.]+Exception\s*:\s*[^\r\n]+)",
            "(Assert\.[^\r\n]+)"
        )

        if (-not [string]::IsNullOrWhiteSpace($exception)) {
            return $exception
        }

        return "No error message"
    }

    $firstLine = ($Message -split "`r?`n")[0].Trim()
    if ($firstLine -match "Application failure while|Automation/technical failure while") {
        return Get-TruncatedSingleLine -Text $firstLine -MaxLength 1000
    }

    return Get-TruncatedSingleLine -Text $firstLine
}

function Get-KeyValueEvidence {
    param([string]$Text)

    $keys = @(
        "BasketId",
        "TransactionId",
        "CorrelationId",
        "RequestId",
        "OperationId",
        "SessionId",
        "PaymentReferenceId",
        "ScenarioPax\(A/T/C/I\)",
        "Summary\(A/T/C/I\)",
        "BasketPeopleCount",
        "BasketTypeCounts",
        "BasketPassengers",
        "DownstreamCode",
        "RorCode"
    )

    $evidence = [System.Collections.Generic.List[string]]::new()

    $methodUrlMatch = [regex]::Match($Text, "GraphQL search endpoint\s+(GET|POST|PUT|PATCH|DELETE):\s*(https?://[^\s\r\n]+)|Endpoint=(GET|POST|PUT|PATCH|DELETE)\s+(https?://[^\s\r\n]+)|(GET|POST|PUT|PATCH|DELETE):\s*(https?://[^\s\r\n]+)|\b(GET|POST|PUT|PATCH|DELETE)\s+(https?://[^\s\r\n]+)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($methodUrlMatch.Success) {
        $method = @($methodUrlMatch.Groups[1].Value, $methodUrlMatch.Groups[3].Value, $methodUrlMatch.Groups[5].Value, $methodUrlMatch.Groups[7].Value) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1
        $url = @($methodUrlMatch.Groups[2].Value, $methodUrlMatch.Groups[4].Value, $methodUrlMatch.Groups[6].Value, $methodUrlMatch.Groups[8].Value) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1
        $evidence.Add("Endpoint=$method $url")
    }

    $statusCode = Get-FirstMatch -Text $Text -Patterns @(
        "StatusCode:\s*([A-Za-z0-9_ -]+)",
        "\bHTTP\s+([0-9]{3}(?:\s+[A-Za-z]+)?)",
        '"statusCode"\s*:\s*([0-9]{3})'
    )
    if (-not [string]::IsNullOrWhiteSpace($statusCode)) {
        $evidence.Add("StatusCode=$statusCode")
    }

    $operationName = Get-FirstMatch -Text $Text -Patterns @(
        '"operationName"\s*:\s*"([^"]+)"',
        "operationName[:=]\s*([A-Za-z0-9_.-]+)"
    )
    if (-not [string]::IsNullOrWhiteSpace($operationName)) {
        $evidence.Add("OperationName=$operationName")
    }

    $origin = Get-FirstMatch -Text $Text -Patterns @('"origin"\s*:\s*"([^"]+)"')
    $destination = Get-FirstMatch -Text $Text -Patterns @('"destination"\s*:\s*"([^"]+)"')
    $departureDate = Get-FirstMatch -Text $Text -Patterns @('"departureDate"\s*:\s*"([^"]+)"')
    if (-not [string]::IsNullOrWhiteSpace($origin) -and -not [string]::IsNullOrWhiteSpace($destination)) {
        $routeEvidence = "Route=$origin-$destination"
        if (-not [string]::IsNullOrWhiteSpace($departureDate)) {
            $routeEvidence += " $departureDate"
        }
        $evidence.Add($routeEvidence)
    }

    $awardSearch = Get-FirstMatch -Text $Text -Patterns @('"awardSearch"\s*:\s*(true|false)')
    if (-not [string]::IsNullOrWhiteSpace($awardSearch)) {
        $evidence.Add("AwardSearch=$awardSearch")
    }

    $ptcs = Get-AllMatches -Text $Text -Patterns @('"ptc"\s*:\s*"([^"]+)"')
    if ($ptcs.Count -gt 0) {
        $evidence.Add("PTCs=$($ptcs -join ',')")
    }

    $responseSummary = Get-FirstMatch -Text $Text -Patterns @(
        'Response:\s*(\{[^\r\n]{1,500})',
        '"message"\s*:\s*"([^"]+)"'
    )
    if (-not [string]::IsNullOrWhiteSpace($responseSummary)) {
        if ($responseSummary.Length -gt 260) {
            $responseSummary = $responseSummary.Substring(0, 260) + "..."
        }
        $evidence.Add("Response=$responseSummary")
    }

    foreach ($key in $keys) {
        $match = [regex]::Match($Text, "$key=([^\r\n]+)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($match.Success) {
            $displayKey = $key.Replace("\(", "(").Replace("\)", ")")
            $evidence.Add("$displayKey=$($match.Groups[1].Value.Trim())")
        }
    }

    $codes = Get-AllMatches -Text $Text -Patterns @(
        '"code"\s*:\s*"([^"]+)"',
        '\\?"code\\?"\s*:\s*\\?"([^"]+)\\?"',
        "extensions\.code[:=]\s*([A-Z0-9_.-]+)"
    )

    if ($codes.Count -gt 0) {
        $evidence.Add("Codes=$($codes -join ', ')")
    }

    return @($evidence)
}

function Get-FirstRepoStackFrame {
    param([string]$Text)

    foreach ($match in [regex]::Matches($Text, "\s+at\s+(.+?\.cs:line\s+\d+)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $frame = $match.Groups[1].Value.Trim()
        if ($frame -match "VAA\.EAPI\.|[/\\]scripts[/\\]") {
            return $frame
        }
    }

    foreach ($match in [regex]::Matches($Text, "in\s+(.+?\.cs):line\s+(\d+)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $path = $match.Groups[1].Value.Trim()
        if ($path -match "VAA\.EAPI\.|[/\\]scripts[/\\]") {
            return "$path`:line $($match.Groups[2].Value.Trim())"
        }
    }

    return ""
}

function Get-StdOutExcerpt {
    param([string]$StdOut)

    if ([string]::IsNullOrWhiteSpace($StdOut)) {
        return ""
    }

    $allLines = @($StdOut -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $diagnosticLines = @($allLines | Where-Object {
        $_ -match "\[browserstack-failure\]|\[browserstack-failure-reporting\]|Latest checkout/basket responses|Checkout/basket responses captured|Passenger checkout API failed|Uh oh|something.?s not right|unable to book|encountered an error"
    })
    $lines = if ($diagnosticLines.Count -gt 0) {
        @($diagnosticLines | Select-Object -Last 10)
    }
    else {
        # Preserve the established API-test report behaviour when no browser diagnostic exists.
        @($allLines | Select-Object -First 10)
    }
    $excerpt = ($lines -join " | ").Trim()
    if ($excerpt.Length -gt 1000) {
        return $excerpt.Substring(0, 1000) + "..."
    }

    return $excerpt
}

function Read-TrxFile {
    param([System.IO.FileInfo]$TrxFile)

    try {
        [xml]$trx = Get-Content -LiteralPath $TrxFile.FullName -Raw -ErrorAction Stop
    }
    catch {
        return @([pscustomobject]@{
            Source = $TrxFile.Name
            TestName = "TRX parse"
            Scenario = "TRX parse"
            Signal = "parse error"
            Owner = "Automation"
            Action = "Check whether the TRX file was partially written or malformed."
            Message = "Unable to parse TRX file: $($_.Exception.Message)"
            Duration = ""
            Evidence = @()
            StackFrame = ""
            StdOutExcerpt = ""
            StartTimeUtc = ""
            EndTimeUtc = ""
            TraceIdentifiers = @()
            OperationName = ""
            IncidentKey = "trx parse"
        })
    }

    if ($null -eq $trx.TestRun -or $null -eq $trx.TestRun.Results -or $null -eq $trx.TestRun.Results.UnitTestResult) {
        return @()
    }

    $failures = @()
    foreach ($result in @($trx.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq "Failed" -or $_.outcome -eq "Error" -or $_.outcome -eq "Timeout" })) {
        $message = [string]$result.Output.ErrorInfo.Message
        $stackTrace = [string]$result.Output.ErrorInfo.StackTrace
        $stdOut = [string]$result.Output.StdOut
        $stdErr = [string]$result.Output.StdErr
        $debugTrace = [string]$result.Output.DebugTrace
        $combined = "$($result.testName)`n$message`n$stackTrace`n$stdOut`n$stdErr`n$debugTrace"
        $signal = Get-InferredSignal -Text $combined
        $messageSummary = Get-MessageFirstLine -Message $message -Combined $combined
        $operationName = Get-TraceOperationName -Text $combined

        $failures += [pscustomobject]@{
            Source = $TrxFile.Name
            TestName = [string]$result.testName
            Scenario = Get-ScenarioName -TestName ([string]$result.testName)
            Signal = $signal.Label
            Owner = $signal.Owner
            Action = $signal.Action
            Message = $messageSummary
            Duration = [string]$result.duration
            Evidence = @(Get-KeyValueEvidence -Text $combined)
            StackFrame = Get-FirstRepoStackFrame -Text $combined
            StdOutExcerpt = Get-StdOutExcerpt -StdOut $stdOut
            StartTimeUtc = [string]$result.startTime
            EndTimeUtc = [string]$result.endTime
            TraceIdentifiers = @(Get-TraceIdentifiers -Text $combined)
            OperationName = $operationName
            IncidentKey = Get-TraceIncidentKey -Text $combined -OperationName $operationName -Signal $signal.Label -Message $messageSummary
        }
    }

    return $failures
}

function Convert-CoverageValueToPercent {
    param(
        [string]$Covered,
        [string]$Valid,
        [string]$Rate
    )

    $coveredValue = 0.0
    $validValue = 0.0
    if ([double]::TryParse($Covered, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$coveredValue) -and
        [double]::TryParse($Valid, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$validValue) -and
        $validValue -gt 0) {
        return [math]::Round(($coveredValue / $validValue) * 100, 2)
    }

    $rateValue = 0.0
    if ([double]::TryParse($Rate, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$rateValue)) {
        return [math]::Round($rateValue * 100, 2)
    }

    return $null
}

function Get-CoverageSourceName {
    param([System.IO.FileInfo]$CoverageFile)

    $directory = $CoverageFile.Directory
    while ($null -ne $directory) {
        $metadataPath = Join-Path $directory.FullName 'project-name.txt'
        if (Test-Path -LiteralPath $metadataPath -PathType Leaf) {
            $metadataValue = (Get-Content -LiteralPath $metadataPath -Raw).Trim()
            if (-not [string]::IsNullOrWhiteSpace($metadataValue)) {
                return $metadataValue
            }
        }

        $directory = $directory.Parent
    }

    if ($null -ne $CoverageFile.Directory) {
        $collectorDirectoryId = [guid]::Empty
        if ([guid]::TryParse($CoverageFile.Directory.Name, [ref]$collectorDirectoryId) -and $null -ne $CoverageFile.Directory.Parent) {
            return $CoverageFile.Directory.Parent.Name
        }

        return $CoverageFile.Directory.Name
    }

    return $CoverageFile.Name
}

function Read-CoverageFile {
    param([System.IO.FileInfo]$CoverageFile)

    try {
        [xml]$coverage = Get-Content -LiteralPath $CoverageFile.FullName -Raw -ErrorAction Stop
    }
    catch {
        return [pscustomobject]@{
            Source = Get-CoverageSourceName -CoverageFile $CoverageFile
            LineCoverage = "parse error"
            BranchCoverage = "parse error"
            Lines = ""
            Branches = ""
            LinesCovered = ""
            LinesValid = ""
            BranchesCovered = ""
            BranchesValid = ""
            Error = "Unable to parse coverage file: $($_.Exception.Message)"
        }
    }

    if ($null -eq $coverage.coverage) {
        return [pscustomobject]@{
            Source = Get-CoverageSourceName -CoverageFile $CoverageFile
            LineCoverage = "unknown"
            BranchCoverage = "unknown"
            Lines = ""
            Branches = ""
            LinesCovered = ""
            LinesValid = ""
            BranchesCovered = ""
            BranchesValid = ""
            Error = "Coverage root element was not found."
        }
    }

    $root = $coverage.coverage
    $linesCovered = [string]$root.'lines-covered'
    $linesValid = [string]$root.'lines-valid'
    $branchesCovered = [string]$root.'branches-covered'
    $branchesValid = [string]$root.'branches-valid'
    $lineCoverage = Convert-CoverageValueToPercent -Covered $linesCovered -Valid $linesValid -Rate ([string]$root.'line-rate')
    $branchCoverage = Convert-CoverageValueToPercent -Covered $branchesCovered -Valid $branchesValid -Rate ([string]$root.'branch-rate')
    $lineText = if ($null -eq $lineCoverage) { "unknown" } else { "$lineCoverage%" }
    $branchText = if ($null -eq $branchCoverage) { "unknown" } else { "$branchCoverage%" }
    $linesText = if ((Convert-CoverageMetricToDouble -Value $linesValid) -gt 0) { "$linesCovered/$linesValid" } else { "" }
    $branchesText = if ((Convert-CoverageMetricToDouble -Value $branchesValid) -gt 0) { "$branchesCovered/$branchesValid" } else { "" }

    return [pscustomobject]@{
        Source = Get-CoverageSourceName -CoverageFile $CoverageFile
        LineCoverage = $lineText
        BranchCoverage = $branchText
        Lines = $linesText
        Branches = $branchesText
        LinesCovered = $linesCovered
        LinesValid = $linesValid
        BranchesCovered = $branchesCovered
        BranchesValid = $branchesValid
        Error = ""
    }
}

function Convert-CoverageMetricToDouble {
    param([string]$Value)

    $parsedValue = 0.0
    if ([double]::TryParse($Value, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$parsedValue)) {
        return $parsedValue
    }

    return 0.0
}

function New-AggregatedCoverageSummary {
    param([object[]]$CoverageSummaries)

    $validCoverageSummaries = @($CoverageSummaries | Where-Object { [string]::IsNullOrWhiteSpace($_.Error) })
    if ($validCoverageSummaries.Count -eq 0) {
        return $null
    }

    $linesCovered = 0.0
    $linesValid = 0.0
    $branchesCovered = 0.0
    $branchesValid = 0.0

    foreach ($summary in $validCoverageSummaries) {
        $linesCovered += Convert-CoverageMetricToDouble -Value ([string]$summary.LinesCovered)
        $linesValid += Convert-CoverageMetricToDouble -Value ([string]$summary.LinesValid)
        $branchesCovered += Convert-CoverageMetricToDouble -Value ([string]$summary.BranchesCovered)
        $branchesValid += Convert-CoverageMetricToDouble -Value ([string]$summary.BranchesValid)
    }

    $lineCoverage = Convert-CoverageValueToPercent -Covered ($linesCovered.ToString([System.Globalization.CultureInfo]::InvariantCulture)) -Valid ($linesValid.ToString([System.Globalization.CultureInfo]::InvariantCulture)) -Rate ""
    $branchCoverage = Convert-CoverageValueToPercent -Covered ($branchesCovered.ToString([System.Globalization.CultureInfo]::InvariantCulture)) -Valid ($branchesValid.ToString([System.Globalization.CultureInfo]::InvariantCulture)) -Rate ""
    $lineText = if ($null -eq $lineCoverage) { "unknown" } else { "$lineCoverage%" }
    $branchText = if ($null -eq $branchCoverage) { "unknown" } else { "$branchCoverage%" }

    return [pscustomobject]@{
        Source = "Total"
        LineCoverage = $lineText
        BranchCoverage = $branchText
        Lines = if ($linesValid -gt 0) { "$linesCovered/$linesValid" } else { "" }
        Branches = if ($branchesValid -gt 0) { "$branchesCovered/$branchesValid" } else { "" }
        Error = ""
        Notes = "Aggregated from $($validCoverageSummaries.Count) coverage file(s)."
    }
}

function Add-CoverageSummaryLines {
    param([object[]]$CoverageSummaries)

    if ($CoverageSummaries.Count -eq 0) {
        return @()
    }

    $coverageLines = @("", "#### Coverage", "| Source | Line Coverage | Branch Coverage | Lines | Branches | Notes |", "| --- | ---: | ---: | ---: | ---: | --- |")
    $aggregatedCoverage = New-AggregatedCoverageSummary -CoverageSummaries $CoverageSummaries
    if ($null -ne $aggregatedCoverage) {
        $coverageLines += "| $($aggregatedCoverage.Source) | $($aggregatedCoverage.LineCoverage) | $($aggregatedCoverage.BranchCoverage) | $($aggregatedCoverage.Lines) | $($aggregatedCoverage.Branches) | $($aggregatedCoverage.Notes) |"
    }

    foreach ($summary in $CoverageSummaries) {
        $notes = if ([string]::IsNullOrWhiteSpace($summary.Error)) { "" } else { $summary.Error.Replace("|", "/") }
        $coverageLines += "| $($summary.Source) | $($summary.LineCoverage) | $($summary.BranchCoverage) | $($summary.Lines) | $($summary.Branches) | $notes |"
    }

    return $coverageLines
}

$trxFiles = @()
if (-not [string]::IsNullOrWhiteSpace($TrxPath)) {
    $trxItem = Get-Item -Path $TrxPath -ErrorAction SilentlyContinue
    if ($null -ne $trxItem -and -not $trxItem.PSIsContainer) {
        $trxFiles = @($trxItem)
    }
}
else {
    $trxFiles = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter "*.trx" -File -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending)
}

$coverageSearchRoot = $ResultsDirectory
if (-not [string]::IsNullOrWhiteSpace($TrxPath) -and
    -not $PSBoundParameters.ContainsKey("ResultsDirectory") -and
    $trxFiles.Count -gt 0) {
    $coverageSearchRoot = $trxFiles[0].DirectoryName
}
$coverageFileHashes = @{}
$coverageFiles = @(
    Get-ChildItem -LiteralPath $coverageSearchRoot -Filter "coverage.cobertura.xml" -File -Recurse -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc -Descending |
        Where-Object {
            $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            if ($coverageFileHashes.ContainsKey($hash)) {
                return $false
            }

            $coverageFileHashes[$hash] = $true
            return $true
        }
)
$coverageSummaries = @()
foreach ($coverageFile in $coverageFiles) {
    $coverageSummaries += @(Read-CoverageFile -CoverageFile $coverageFile)
}

$heading = if ([string]::IsNullOrWhiteSpace($EnvironmentName)) { "### Post-Test Failure Analysis - $SuiteName" } else { "### Post-Test Failure Analysis - $SuiteName ($EnvironmentName)" }
$lines = @($heading, "")

if ($trxFiles.Count -eq 0) {
    if (-not [string]::IsNullOrWhiteSpace($TrxPath)) {
        $lines += "No TRX file was found at ``$TrxPath``."
    }
    else {
        $lines += "No TRX files were found under ``$ResultsDirectory``."
    }
    $lines += @(Add-CoverageSummaryLines -CoverageSummaries $coverageSummaries)
    $lines -join "`n"
    return
}

$failures = @()
foreach ($trxFile in $trxFiles) {
    $failures += @(Read-TrxFile -TrxFile $trxFile)
}

if (-not [string]::IsNullOrWhiteSpace($TraceManifestPath) -and $failures.Count -gt 0) {
    $traceEnvironment = $EnvironmentName.Trim().ToLowerInvariant()
    if ($AllowedTraceEnvironments -notcontains $traceEnvironment) {
        throw "Trace manifests are only permitted for dev, sit, and uat. Environment '$EnvironmentName' is not permitted."
    }

    $manifestDirectory = Split-Path -Parent $TraceManifestPath
    if (-not [string]::IsNullOrWhiteSpace($manifestDirectory)) {
        New-Item -ItemType Directory -Path $manifestDirectory -Force | Out-Null
    }

    $manifestFailures = @($failures | ForEach-Object {
        [pscustomobject][ordered]@{
            source = $_.Source
            testName = $_.TestName
            scenario = $_.Scenario
            startTimeUtc = $_.StartTimeUtc
            endTimeUtc = $_.EndTimeUtc
            duration = $_.Duration
            operationName = $_.OperationName
            signal = $_.Signal
            owner = $_.Owner
            message = $_.Message
            identifiers = @($_.TraceIdentifiers)
            incidentKey = $_.IncidentKey
        }
    })
    $incidentGroups = @($manifestFailures |
        Group-Object incidentKey |
        Sort-Object Count -Descending |
        ForEach-Object {
            $groupFailures = @($_.Group)
            [ordered]@{
                key = $_.Name
                count = $_.Count
                scenarios = @($groupFailures | ForEach-Object { $_.scenario } | Select-Object -Unique)
                identifiers = @($groupFailures | ForEach-Object { @($_.identifiers) } | Select-Object -Unique)
            }
        })
    $manifest = [ordered]@{
        schemaVersion = 1
        generatedAtUtc = [DateTime]::UtcNow.ToString("o")
        suite = $SuiteName
        environment = $traceEnvironment
        repository = [string]$env:GITHUB_REPOSITORY
        workflowRunId = [string]$env:GITHUB_RUN_ID
        workflowRunUrl = $WorkflowRunUrl
        failureCount = $manifestFailures.Count
        incidents = $incidentGroups
        failures = $manifestFailures
    }
    $manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $TraceManifestPath -Encoding utf8
}

if ($failures.Count -eq 0) {
    $lines += "No failed tests were found in the TRX output."
    $lines += @(Add-CoverageSummaryLines -CoverageSummaries $coverageSummaries)
    $lines -join "`n"
    return
}

$lines += "Total failures analysed: **$($failures.Count)**"
$lines += ""

if (-not [string]::IsNullOrWhiteSpace($TraceManifestPath)) {
    $manifestName = Split-Path -Leaf $TraceManifestPath
    $lines += "A failure trace manifest was generated as ``$manifestName`` for optional local Azure enrichment."
    if (-not [string]::IsNullOrWhiteSpace([string]$env:GITHUB_RUN_ID)) {
        $lines += "Run locally: ``./scripts/trace-workflow-failures.ps1 -RunId $($env:GITHUB_RUN_ID) -Environment $($EnvironmentName.ToLowerInvariant())``"
    }
    $lines += ""
}

$repeated = @($failures | Group-Object Scenario | Where-Object { $_.Count -gt 1 } | Sort-Object Count -Descending)
if ($repeated.Count -gt 0) {
    $lines += "#### Repeated failures"
    foreach ($group in $repeated) {
        $lines += "- $($group.Name): $($group.Count) occurrences"
    }
    $lines += ""
}

$lines += "#### Failed scenarios"

foreach ($failure in @($failures | Select-Object -First $Top)) {
    $lines += ""
    $lines += "##### $($failure.Scenario)"
    $lines += "- Source: ``$($failure.Source)``"
    if (-not [string]::IsNullOrWhiteSpace($failure.Duration)) { $lines += "- Duration: $($failure.Duration)" }
    $lines += "- Inferred signal: $($failure.Signal)"
    $lines += "- Likely owner: $($failure.Owner)"
    $lines += "- Error: $($failure.Message.Replace('|', '/'))"
    if (-not [string]::IsNullOrWhiteSpace($failure.StackFrame)) { $lines += "- First repo stack frame: $($failure.StackFrame)" }
    if ($failure.Evidence.Count -gt 0) { $lines += "- Evidence: $(($failure.Evidence | ForEach-Object { $_.Replace('|', '/') }) -join '; ')" }
    if (-not [string]::IsNullOrWhiteSpace($failure.StdOutExcerpt)) { $lines += "- StdOut excerpt: $($failure.StdOutExcerpt.Replace('|', '/'))" }
    $lines += "- Recommended next action: $($failure.Action.Replace('|', '/'))"
}

if ($failures.Count -gt $Top) {
    $lines += ""
    $lines += "_Showing first $Top failures. Full TRX files are available in workflow artifacts._"
}

$lines += @(Add-CoverageSummaryLines -CoverageSummaries $coverageSummaries)

$lines -join "`n"
