# Knowledge Repo Updates

This root-level docs folder describes how commits are captured into a local, project/area-aware knowledge log. The log complements, but does not replace, execution plans, decision records, and the technical debt tracker.

## Purpose

Keep a lightweight engineering trail in the same spirit as the broader automation documentation style: explicit scope, impacted projects/areas, validation, risk context, and traceable changed files.

Each captured change writes one entry to:

- [`knowledge-repo.md`](knowledge-repo.md)

In pre-commit mode, the script refreshes the latest matching `pending` entry instead of appending duplicate entries when the same staged file set is committed more than once.

## Entry model

Each entry captures:

- UTC timestamp
- commit SHA (or `pending` in pre-commit mode)
- commit summary
- impacted projects inferred from changed `.csproj` ownership
- impacted non-project areas
- validation command(s)
- automatic and manual risk notes
- changed files under the tracked pathspecs

## Automatic flow

After installing hooks:

```pwsh
scripts/verify.ps1 -InstallHooks
```

`pre-commit` runs:

```pwsh
scripts/verify-staged.ps1 -SkipBuild
scripts/update-knowledge-repo.ps1 -FromStaged -ValidationCommand "central package management guard (no inline PackageReference Version= in staged .csproj)","<resolved scripts/verify.ps1 command(s) from scripts/verify-staged.ps1 -SkipBuild -ListCommands>"
```

The entry is appended or refreshed and staged so it lands in the same commit.

## Manual usage

Run manually if hooks were bypassed or temporarily disabled:

```pwsh
scripts/update-knowledge-repo.ps1
```

Record a specific commit:

```pwsh
scripts/update-knowledge-repo.ps1 -Commit HEAD~2
```

Record commit context explicitly:

```pwsh
scripts/update-knowledge-repo.ps1 -Commit HEAD -ValidationCommand "dotnet build <solution>.sln -c Release","dotnet test <test-project>.csproj -c Release" -RiskNote "No runtime behavior change; doc-only update"
```

## Operational behavior

- The update is local and file-based.
- If no files in the tracked pathspecs are detected, no entry is appended.
- The knowledge log file itself is excluded from the changed-file list to avoid self-referential entries.
- Project names are inferred by walking changed paths up to the nearest owning `.csproj`.
- Non-project areas are inferred from top-level folders/files such as `(root)`, `docs`, `scripts`, `.githooks`, and `.github`.
- Risk notes are generated automatically from changed paths, including docs-only, startup/DI, deployment, trigger/subscriber, behavior, project-file, and hook/harness changes. Optional `-RiskNote` values are appended to those automatic notes.
- Pre-commit now records both checks in the entry validation section:
	- central package management guard for staged `.csproj`
	- lightweight staged harness command(s), resolved from changed files (`scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild`, `scripts/verify.ps1 -ProjectPath <project>.csproj -SkipBuild`, or `scripts/verify.ps1 -ScopePath . -SkipBuild` for docs/root-only changes)

## Sequence Diagram Change Capture

When a commit changes `docs/sequence-diagrams/`:

- use `docs/sequence-diagrams/_template.sequence-diagram.md` for new diagrams,
- keep `docs/sequence-diagrams.md` index status aligned with diagram-file status,
- keep `docs/behavior-harness.md` sensor mapping aligned,
- ensure `scripts/check-sequence-diagram-status.ps1` passes (it is part of `scripts/verify.ps1`).

Recommended validation notes for sequence-diagram commits:

- `scripts/check-sequence-diagram-status.ps1`
- `scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild` (or `-ProjectPath <project>.csproj -SkipBuild`)
