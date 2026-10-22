<#
.SYNOPSIS
    Renders Mermaid blocks from Markdown files to prove that Mermaid can parse them.

.DESCRIPTION
    Uses the pinned Mermaid CLI through npx and renders every Mermaid fenced block
    into a temporary SVG. Rendering each block separately makes failures actionable
    by reporting the source file and one-based block number.
#>
[CmdletBinding()]
param(
    [string[]]$Path,

    [string]$BaseRef = 'origin/main',

    [switch]$All,

    # Authoring checks only. This never establishes parser/render success.
    [switch]$LintOnly,

    [ValidateNotNullOrEmpty()]
    [string]$MermaidCliVersion = '11.12.0'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Get-RepoRelativeMarkdownFiles {
    $candidates = [System.Collections.Generic.List[string]]::new()

    if ($Path -and $Path.Count -gt 0) {
        foreach ($item in $Path) {
            if (-not [string]::IsNullOrWhiteSpace($item)) {
                $candidates.Add($item)
            }
        }
    } elseif ($All) {
        foreach ($item in (& git -C $repoRoot ls-files -- '*.md')) {
            $candidates.Add($item)
        }
    } else {
        & git -C $repoRoot rev-parse --verify $BaseRef *> $null
        if ($LASTEXITCODE -ne 0) {
            throw "Cannot resolve Mermaid validation base ref '$BaseRef'. Pass -Path, -All, or a valid -BaseRef."
        }

        foreach ($item in (& git -C $repoRoot diff --name-only --diff-filter=ACMR "${BaseRef}...HEAD" -- '*.md')) {
            $candidates.Add($item)
        }
        foreach ($item in (& git -C $repoRoot diff --name-only --diff-filter=ACMR -- '*.md')) {
            $candidates.Add($item)
        }
        foreach ($item in (& git -C $repoRoot diff --cached --name-only --diff-filter=ACMR -- '*.md')) {
            $candidates.Add($item)
        }
        foreach ($item in (& git -C $repoRoot ls-files --others --exclude-standard -- '*.md')) {
            $candidates.Add($item)
        }
    }

    $resolved = [System.Collections.Generic.List[object]]::new()
    foreach ($candidate in ($candidates | Sort-Object -Unique)) {
        $normalized = ([string]$candidate).Replace('\', '/')
        if ([IO.Path]::IsPathRooted($normalized) -or $normalized.Split('/') -contains '..') {
            throw "Mermaid validation path must be repository-relative and cannot traverse parent directories: '$candidate'."
        }

        $fullPath = Join-Path $repoRoot $normalized
        if ($Path -and $Path.Count -gt 0 -and -not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "Explicit Mermaid validation path does not exist or is not a file: '$candidate'."
        }
        if (-not $normalized.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase)) {
            if ($Path -and $Path.Count -gt 0) {
                throw "Explicit Mermaid validation path must identify a Markdown (.md) file: '$candidate'."
            }
            continue
        }

        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }

        $resolved.Add([PSCustomObject]@{ RelativePath = $normalized; FullPath = $fullPath })
    }

    return @($resolved)
}

$markdownFiles = @(Get-RepoRelativeMarkdownFiles)
if ($markdownFiles.Count -eq 0) {
    Write-Host 'Mermaid render validation passed: no Markdown files require validation.'
    return
}

$fencePattern = '(?ms)^[ \t]*```mermaid[^\r\n]*\r?\n(?<body>.*?)^[ \t]*```[ \t]*\r?$'
$mermaidBlocks = [System.Collections.Generic.List[object]]::new()
foreach ($markdownFile in $markdownFiles) {
    $text = Get-Content -LiteralPath $markdownFile.FullPath -Raw
    $matches = [regex]::Matches($text, $fencePattern)
    for ($index = 0; $index -lt $matches.Count; $index++) {
        $mermaidBlocks.Add([PSCustomObject]@{
            RelativePath = $markdownFile.RelativePath
            BlockNumber = $index + 1
            Body = $matches[$index].Groups['body'].Value
            StartLine = 1 + ([regex]::Matches($text.Substring(0, $matches[$index].Groups['body'].Index), "`n")).Count
        })
    }
}

if ($mermaidBlocks.Count -eq 0) {
    Write-Host "Mermaid render validation passed: no Mermaid fences found in $($markdownFiles.Count) Markdown file(s)."
    return
}

$lintFailures = [System.Collections.Generic.List[string]]::new()
foreach ($block in $mermaidBlocks) {
    if ($block.Body -notmatch '(?m)^\s*sequenceDiagram\b') { continue }
    $lines = $block.Body -split '\r?\n'
    for ($lineIndex = 0; $lineIndex -lt $lines.Count; $lineIndex++) {
        $line = $lines[$lineIndex]
        if ($line -match '^\s*%%') { continue }
        # Mermaid entity codes include their own terminator. Preserve those.
        $withoutEntities = $line -replace '#(?:[0-9]+|[a-zA-Z][a-zA-Z0-9]*);', ''
        if ($withoutEntities.Contains(';')) {
            $lintFailures.Add("$($block.RelativePath):$($block.StartLine + $lineIndex) (Mermaid block $($block.BlockNumber)): raw semicolon in sequence diagram. Use a comma, <br/>, or Mermaid's #59; entity for a literal semicolon. Put statements on separate lines.")
        }
    }
}
if ($lintFailures.Count -gt 0) {
    throw ("Mermaid authoring lint failed:`n{0}" -f ($lintFailures -join "`n"))
}
if ($LintOnly) {
    Write-Host "Mermaid authoring lint passed: checked $($mermaidBlocks.Count) block(s). Parser/render validation was not run."
    return
}

$npx = Get-Command npx -ErrorAction SilentlyContinue
if ($null -eq $npx) {
    throw 'Mermaid render validation requires Node.js with npx on PATH. Install Node.js, then rerun this check.'
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("vaa-eapi-mermaid-{0}" -f [guid]::NewGuid().ToString('N'))
$puppeteerConfigPath = Join-Path $temporaryRoot 'puppeteer-config.json'
$failures = [System.Collections.Generic.List[string]]::new()
$blockCount = 0

New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
try {
    '{"args":["--no-sandbox","--disable-setuid-sandbox"]}' | Set-Content -LiteralPath $puppeteerConfigPath -Encoding UTF8
    foreach ($mermaidBlock in $mermaidBlocks) {
        $blockCount++
        $inputPath = Join-Path $temporaryRoot ("diagram-{0}.mmd" -f $blockCount)
        $outputPath = Join-Path $temporaryRoot ("diagram-{0}.svg" -f $blockCount)
        Set-Content -LiteralPath $inputPath -Value $mermaidBlock.Body -Encoding UTF8

        $cliOutput = @(& $npx.Source --yes "@mermaid-js/mermaid-cli@$MermaidCliVersion" --input $inputPath --output $outputPath --puppeteerConfigFile $puppeteerConfigPath --quiet 2>&1)
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath -PathType Leaf)) {
            $details = ($cliOutput | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
            $failures.Add("$($mermaidBlock.RelativePath) (Mermaid block $($mermaidBlock.BlockNumber)) failed to render.$([Environment]::NewLine)$details")
        }
    }
} finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}

if ($failures.Count -gt 0) {
    throw ("Mermaid render validation failed:`n`n{0}" -f ($failures -join "`n`n"))
}

Write-Host "Mermaid render validation passed: rendered $blockCount block(s) from $($markdownFiles.Count) Markdown file(s) with Mermaid CLI $MermaidCliVersion."
