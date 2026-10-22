function Test-SequenceGoalDocumentationMaintenance {
    param(
        [object[]]$PullRequests,
        [string]$Branch,
        [string[]]$ChangedPaths
    )

    $maintenancePr = @($PullRequests | Where-Object {
        -not $_.isCrossRepository -and $_.headRefName -ceq $Branch -and
        $_.baseRefName -ceq 'main' -and
        @($_.labels | Where-Object { $_.name -ceq 'sequence-goal:documentation-only' }).Count -gt 0
    })
    if ($maintenancePr.Count -eq 0) { return $false }
    if ($maintenancePr.Count -ne 1) { throw 'Documentation maintenance requires exactly one matching open PR.' }

    # The explicit label suppresses delivery, never normal PR validation. Limit it
    # to documentation and the scripts/workflows that maintain that documentation.
    $unsupported = @($ChangedPaths | Where-Object {
        $_ -cnotmatch '^(docs/.*\.md|scripts/[^/]+\.ps1|\.github/workflows/[^/]+\.ya?ml)$'
    })
    if ($unsupported.Count -gt 0) {
        throw "Documentation-only delivery routing cannot include application or other unsupported files: $($unsupported -join ';')"
    }
    return $true
}
