#Requires -Version 5.1
<#
.SYNOPSIS
    Validates sequence-diagram agent-loop metadata and status consistency.

.DESCRIPTION
    Ensures docs/sequence-diagrams.md index rows match per-file Agent Loop Goal
    status, required metadata fields exist, agent-loop delivery sections are
    present, and Complete goals reference a non-planned harness sensor value
    that points to a test or script path.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath ((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())).Path
$allowedStatuses = @('Draft', 'Ready', 'InProgress', 'Complete')
$failures = New-Object System.Collections.Generic.List[string]

function Join-PathSegments {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][string[]]$Segments
    )

    $path = $Root
    foreach ($segment in $Segments) {
        $path = Join-Path $path $segment
    }

    return $path
}

$indexPath = Join-PathSegments -Root $repoRoot -Segments @('docs', 'sequence-diagrams.md')
$diagramsDir = Join-PathSegments -Root $repoRoot -Segments @('docs', 'sequence-diagrams')

function Add-Failure {
    param([Parameter(Mandatory)][string]$Message)

    $failures.Add($Message) | Out-Null
}

function Test-EvidencePathUnderRoot {
    param(
        [Parameter(Mandatory)][string]$RelativePath,
        [Parameter(Mandatory)][string[]]$AllowedRoots
    )

    if ([System.IO.Path]::IsPathRooted($RelativePath)) {
        return $false
    }

    $candidate = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $RelativePath))
    foreach ($allowedRoot in $AllowedRoots) {
        $resolvedRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $allowedRoot)).TrimEnd(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
        if ($candidate.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase) -and
            (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            return $true
        }
    }

    return $false
}

if (-not (Test-Path -LiteralPath $indexPath -PathType Leaf)) {
    Add-Failure "Missing required index: docs/sequence-diagrams.md"
}

if (-not (Test-Path -LiteralPath $diagramsDir -PathType Container)) {
    Add-Failure "Missing required diagrams folder: docs/sequence-diagrams"
}

if ($failures.Count -gt 0) {
    Write-Host 'sequence diagram status check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host "- $failure" -ForegroundColor Red
    }

    exit 1
}

$indexText = Get-Content -LiteralPath $indexPath -Raw
$indexRows = [regex]::Matches(
    $indexText,
    '(?im)^\|\s*\[(?<name>[^\]]+)\]\((?<path>sequence-diagrams\/[^)]+\.md)\)\s*\|\s*[^|]*\|\s*`(?<status>[^`]+)`\s*\|\s*$'
)

if ($indexRows.Count -eq 0) {
    Add-Failure 'No diagram rows found in docs/sequence-diagrams.md index.'
}

$indexEntries = @{}
foreach ($row in $indexRows) {
    $relativePath = $row.Groups['path'].Value
    $status = $row.Groups['status'].Value.Trim()

    if ($allowedStatuses -notcontains $status) {
        Add-Failure "Index status '$status' is invalid for $relativePath. Allowed: $($allowedStatuses -join ', ')."
        continue
    }

    if ($indexEntries.ContainsKey($relativePath)) {
        Add-Failure "Duplicate index entry found for $relativePath."
        continue
    }

    $indexEntries[$relativePath] = [PSCustomObject]@{
        Name = $row.Groups['name'].Value
        Status = $status
    }
}

$diagramFiles = @(
    Get-ChildItem -LiteralPath $diagramsDir -Filter '*.md' -File |
    Where-Object {
        $_.Name -notmatch '^(?i)(_template|template)'
    }
)
foreach ($diagramFile in $diagramFiles) {
    $key = "sequence-diagrams/$($diagramFile.Name)"
    if (-not $indexEntries.ContainsKey($key)) {
        Add-Failure "Diagram file docs/$key is not listed in docs/sequence-diagrams.md index."
    }
}

foreach ($entry in $indexEntries.GetEnumerator()) {
    $relativePath = $entry.Key
    $diagramPath = Join-PathSegments -Root $repoRoot -Segments (@('docs') + ($relativePath -split '/'))
    if (-not (Test-Path -LiteralPath $diagramPath -PathType Leaf)) {
        Add-Failure "Indexed diagram does not exist: docs/$relativePath"
        continue
    }

    $diagramText = Get-Content -LiteralPath $diagramPath -Raw
    $goalSection = [regex]::Match(
        $diagramText,
        '(?ims)^##\s+Agent Loop Goal\s*$\r?\n(?<section>.*?)(?=^##\s+|\z)'
    )
    if (-not $goalSection.Success) {
        Add-Failure "docs/$relativePath is missing an 'Agent Loop Goal' section."
        continue
    }

    $section = $goalSection.Groups['section'].Value
    $requiredFields = @(
        'Goal',
        'Status',
        'Owner',
        'Review cadence',
        'Last reviewed',
        'Harness Sensor',
        'Promotion verification',
        'Acceptance evidence',
        'Done when'
    )
    foreach ($field in $requiredFields) {
        if (-not [regex]::IsMatch($section, "(?im)^\|\s*$([regex]::Escape($field))\s*\|\s*[^|\s][^|]*\|\s*$")) {
            Add-Failure "docs/$relativePath Agent Loop Goal is missing required field '$field' or has an empty value."
        }
    }

    $statusMatch = [regex]::Match($section, '(?im)^\|\s*Status\s*\|\s*`(?<status>[^`]+)`\s*\|\s*$')
    if (-not $statusMatch.Success) {
        Add-Failure "docs/$relativePath Agent Loop Goal has no parsable Status row."
        continue
    }

    $fileStatus = $statusMatch.Groups['status'].Value.Trim()
    if ($allowedStatuses -notcontains $fileStatus) {
        Add-Failure "docs/$relativePath has invalid status '$fileStatus'. Allowed: $($allowedStatuses -join ', ')."
        continue
    }

    $indexStatus = $entry.Value.Status
    if ($fileStatus -ne $indexStatus) {
        Add-Failure "Status mismatch for docs/${relativePath}: index=$indexStatus file=$fileStatus."
    }

    $deliverySurfaces = @('Backend')
    $deliverySurfaceMatch = [regex]::Match($section, '(?im)^\|\s*Delivery surface\s*\|(?<value>[^|]*)\|\s*$')
    if ($deliverySurfaceMatch.Success) {
        $deliverySurfaceValue = $deliverySurfaceMatch.Groups['value'].Value.Trim().Trim('`').Trim()
        $deliverySurfaces = @($deliverySurfaceValue -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
        $allowedDeliverySurfaces = @('Backend', 'MobileIos', 'MobileAndroid', 'WebUi')
        if ($deliverySurfaces.Count -eq 0) {
            Add-Failure "docs/$relativePath has an empty Delivery surface. Use one or more of: $($allowedDeliverySurfaces -join ', ')."
        }
        foreach ($deliverySurface in $deliverySurfaces) {
            if ($allowedDeliverySurfaces -notcontains $deliverySurface) {
                Add-Failure "docs/$relativePath has invalid Delivery surface '$deliverySurface'. Allowed: $($allowedDeliverySurfaces -join ', ')."
            }
        }
    }

    $requiredSections = @(
        'Agent delivery constraints',
        'Expected harness',
        'Open points to finalize'
    )
    foreach ($requiredSection in $requiredSections) {
        if (-not [regex]::IsMatch($diagramText, "(?im)^##\s+$([regex]::Escape($requiredSection))\s*$")) {
            Add-Failure "docs/$relativePath is missing required section '$requiredSection'."
        }
    }

    $suppliedUxField = [regex]::Match($diagramText, '(?im)^\|\s*Supplied UX\s*\|\s*`?(?<value>[^|`]+)`?\s*\|\s*$')
    if ($suppliedUxField.Success -and $suppliedUxField.Groups['value'].Value.Trim() -match '(?i)^not applicable$') {
        Add-Failure "docs/$relativePath has invalid Supplied UX evidence. Use None only when no UX artefact was supplied."
    }
    $hasSuppliedUx = [regex]::IsMatch(
        $diagramText,
        '(?im)(?:https?://[^\s|)]*|(?:\.{0,2}/)?[^\s|()]+)\.(?:png|jpe?g|webp|gif|svg|pdf|mp4|mov|webm)(?:[?#][^\s|)]*)?|https?://[^\s)]*(?:figma\.com|invisionapp\.com|sketch\.cloud|adobe\.com/(?:xd|products/xd)|marvelapp\.com|proto\.io|framer\.com)/'
    ) -or ($suppliedUxField.Success -and $suppliedUxField.Groups['value'].Value.Trim() -notmatch '(?i)^(?:none|n/a)$')
    if ($hasSuppliedUx) {
        $uxContract = [regex]::Match($diagramText, '(?is)## UX contract\s*(?<body>.*?)(?:\r?\n## |\z)')
        if (-not $uxContract.Success) {
            Add-Failure "docs/$relativePath supplies UX but has no UX contract section."
        } else {
            $uxBody = $uxContract.Groups['body'].Value
            $authority = [regex]::Match($uxBody, '(?im)^\|\s*Implementation authority\s*\|\s*`?(?<value>[^|`]+)`?\s*\|\s*$')
            if (-not $authority.Success -or $authority.Groups['value'].Value.Trim() -ne 'Direct instruction') {
                Add-Failure "docs/$relativePath supplies UX but does not mark it as a Direct instruction."
            }
            $flowAuthority = [regex]::Match($uxBody, '(?im)^\|\s*Flow and behaviour authority\s*\|\s*`?(?<value>[^|`]+)`?\s*\|\s*$')
            if (-not $flowAuthority.Success -or $flowAuthority.Groups['value'].Value.Trim() -ne 'Sequence (detailed)') {
                Add-Failure "docs/$relativePath supplies UX but does not make Sequence (detailed) authoritative for flow and behaviour."
            }
            foreach ($uxField in @('Supplied UX', 'Screen/state mapping', 'Design inspection matrix', 'Visual verification', 'UI/UX acceptance test', 'Rendered evidence', 'Comparison result')) {
                $uxFieldMatch = [regex]::Match($uxBody, "(?im)^\|\s*$([regex]::Escape($uxField))\s*\|\s*(?<value>.*?)\s*\|\s*$")
                if (-not $uxFieldMatch.Success -or
                    [string]::IsNullOrWhiteSpace($uxFieldMatch.Groups['value'].Value) -or
                    $uxFieldMatch.Groups['value'].Value -match '(?i)^\s*`?(?:none|not applicable|n/a|tbd|todo)\b|<[^>]+>') {
                    Add-Failure "docs/$relativePath supplies UX but has no concrete $uxField value."
                }
            }
            $visualVerification = [regex]::Match($uxBody, '(?im)^\|\s*Visual verification\s*\|\s*(?<value>.*?)\s*\|\s*$')
            if ($fileStatus -eq 'Complete' -and $visualVerification.Success -and $visualVerification.Groups['value'].Value -match '(?i)\bplanned\b') {
                Add-Failure "docs/$relativePath is Complete but UX visual verification remains planned."
            }
            $screenStateMapping = [regex]::Match($uxBody, '(?im)^\|\s*Screen/state mapping\s*\|\s*(?<value>.*?)\s*\|\s*$')
            if ($fileStatus -eq 'Complete' -and $screenStateMapping.Success -and $screenStateMapping.Groups['value'].Value -match '(?i)\bplanned\b') {
                Add-Failure "docs/$relativePath is Complete but UX screen/state mapping remains planned."
            }
            $designInspectionMatrix = [regex]::Match($uxBody, '(?im)^\|\s*Design inspection matrix\s*\|\s*(?<value>.*?)\s*\|\s*$')
            if ($fileStatus -eq 'Complete' -and $designInspectionMatrix.Success -and $designInspectionMatrix.Groups['value'].Value -match '(?i)\bplanned\b') {
                Add-Failure "docs/$relativePath is Complete but its UX design inspection matrix remains planned."
            }
            if ($fileStatus -eq 'Complete') {
                $uiAcceptanceTest = [regex]::Match($uxBody, '(?im)^\|\s*UI/UX acceptance test\s*\|\s*(?<value>.*?)\s*\|\s*$')
                if (-not $uiAcceptanceTest.Success -or $uiAcceptanceTest.Groups['value'].Value -notmatch 'VAA\.EAPI\.Automation/[A-Za-z0-9_./-]+\.(?:cs|feature)') {
                    Add-Failure "docs/$relativePath is Complete but has no concrete UI/UX acceptance test under VAA.EAPI.Automation/."
                } else {
                    foreach ($testPath in @([regex]::Matches($uiAcceptanceTest.Groups['value'].Value, 'VAA\.EAPI\.Automation/[A-Za-z0-9_./-]+\.(?:cs|feature)') | ForEach-Object { $_.Value })) {
                        if (-not (Test-EvidencePathUnderRoot -RelativePath $testPath -AllowedRoots @('VAA.EAPI.Automation'))) {
                            Add-Failure "docs/$relativePath UI/UX acceptance test is missing or outside VAA.EAPI.Automation/: $testPath"
                        }
                    }
                }
                $renderedEvidence = [regex]::Match($uxBody, '(?im)^\|\s*Rendered evidence\s*\|\s*(?<value>.*?)\s*\|\s*$')
                $imagePaths = if ($renderedEvidence.Success) { @([regex]::Matches($renderedEvidence.Groups['value'].Value, '(?:docs|TestResults)/[A-Za-z0-9_./-]+\.(?:png|jpe?g|webp)') | ForEach-Object { $_.Value }) } else { @() }
                if ($imagePaths.Count -lt 2) {
                    Add-Failure "docs/$relativePath must retain at least an actual render and comparison/diff image for UI/UX acceptance."
                }
                foreach ($imagePath in $imagePaths) {
                    if (-not (Test-EvidencePathUnderRoot -RelativePath $imagePath -AllowedRoots @('docs', 'TestResults'))) {
                        Add-Failure "docs/$relativePath rendered UI/UX evidence is missing or outside docs/ and TestResults/: $imagePath"
                    }
                }
                $comparisonResult = [regex]::Match($uxBody, '(?im)^\|\s*Comparison result\s*\|\s*(?<value>.*?)\s*\|\s*$')
                if (-not $comparisonResult.Success -or $comparisonResult.Groups['value'].Value -notmatch '(?i)^\s*`?Passed\b') {
                    Add-Failure "docs/$relativePath is Complete but its UI/UX comparison result is not Passed."
                }
            }
        }
    }

    $lastReviewedMatch = [regex]::Match($section, '(?im)^\|\s*Last reviewed\s*\|\s*(?<value>.+?)\s*\|\s*$')
    if ($lastReviewedMatch.Success) {
        $lastReviewed = $lastReviewedMatch.Groups['value'].Value.Trim()
        if (-not [regex]::IsMatch($lastReviewed, '^\d{4}-\d{2}-\d{2}$')) {
            Add-Failure "docs/$relativePath Last reviewed must use YYYY-MM-DD."
        }
    }

    if ($fileStatus -eq 'Complete') {
        $remoteSurfaces = @($deliverySurfaces | Where-Object { $_ -in @('MobileIos', 'MobileAndroid', 'WebUi') })
        $associatedSection = [regex]::Match($diagramText, '(?is)## Associated delivery pull requests\s*(?<body>.*?)(?:\r?\n## |\z)')
        if ($remoteSurfaces.Count -gt 0) {
            foreach ($remoteSurface in $remoteSurfaces) {
                if (-not $associatedSection.Success -or $associatedSection.Groups['body'].Value -notmatch "(?im)^\|\s*``?$([regex]::Escape($remoteSurface))``?\s*\|.*\|\s*``?Merged``?\s*\|") {
                    Add-Failure "docs/$relativePath is Complete but associated $remoteSurface delivery PR evidence is absent or not Merged."
                }
            }
        }
        if ($associatedSection.Success -and $associatedSection.Groups['body'].Value -match '(?im)^\|\s*``?QA-') {
            foreach ($deliverySurface in $deliverySurfaces) {
                foreach ($requiredRow in @($deliverySurface, "QA-$deliverySurface")) {
                    if ($associatedSection.Groups['body'].Value -notmatch "(?im)^\|\s*``?$([regex]::Escape($requiredRow))``?\s*\|.*\|\s*``?Merged``?\s*\|") {
                        Add-Failure "docs/$relativePath is Complete but associated $requiredRow delivery PR evidence is absent or not Merged."
                    }
                }
            }
        }
        if ($associatedSection.Success -and
            $associatedSection.Groups['body'].Value -match '(?im)^\|\s*``?Coordinator``?\s*\|' -and
            $associatedSection.Groups['body'].Value -notmatch '(?im)^\|\s*``?Coordinator``?\s*\|.*\|\s*``?(?:Open|Closed|Merged)``?\s*\|') {
            Add-Failure "docs/$relativePath is Complete but its coordinator PR evidence is not an Open, Closed-recovery, or Merged checkpoint."
        }
        $sensorMatch = [regex]::Match($section, '(?im)^\|\s*Harness Sensor\s*\|\s*(?<value>.+?)\s*\|\s*$')
        if (-not $sensorMatch.Success) {
            Add-Failure "docs/$relativePath is Complete but missing Harness Sensor."
            continue
        }

        $sensorValue = $sensorMatch.Groups['value'].Value.Trim()
        $sensorValuePlain = $sensorValue.Trim('`').Trim()
        if ($sensorValuePlain -match '^(?i)planned[:\s]') {
            Add-Failure "docs/$relativePath is Complete but Harness Sensor is marked planned."
            continue
        }

        if ($sensorValuePlain -notmatch '\.cs$' -and $sensorValuePlain -notmatch '\.ps1$') {
            Add-Failure "docs/$relativePath is Complete but Harness Sensor should point to a test/script path."
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'sequence diagram status check failed:' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host "- $failure" -ForegroundColor Red
    }

    exit 1
}

Write-Host 'sequence diagram status check passed' -ForegroundColor Green
exit 0
