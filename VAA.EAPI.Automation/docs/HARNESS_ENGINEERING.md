# Harness Engineering (VAA.EAPI.Automation)

This document records how [harness engineering](https://martinfowler.com/articles/harness-engineering.html)
is being applied to `VAA.EAPI.Automation.sln`. It is a living roadmap: update it as
threads are completed or re-prioritised.

## What harness engineering is

> **Agent = Model + Harness**

When an AI coding agent contributes to a codebase, the *model* generates changes and the
*harness* is everything around it that guides and validates those changes. A strong
harness lets the team trust agent output and direct human attention to where judgement
actually matters.

Two kinds of control make up a harness:

- **Guides (feedforward)** — anticipate and prevent unwanted output *before* it happens
  (conventions, templates, `.editorconfig`, AGENTS.md).
- **Sensors (feedback)** — observe output and enable correction *after* it happens
  (build, analyzers, formatters, tests, CI gates, LLM review).

Each control is either:

- **Computational** — deterministic and fast (linters, type checkers, tests), or
- **Inferential** — semantic and non-deterministic (prose conventions, LLM review).

Controls are organised into three regulation categories: **maintainability** (style,
quality, complexity), **architecture fitness** (structure, dependencies), and
**behavior** (functional correctness). The guiding practice is **keep quality left** —
position every check as early in the lifecycle as it can run.

## Using the harness

The harness is mostly passive — it runs itself. The only command you invoke directly
is `verify.ps1` (run from the `VAA.EAPI.Automation` folder).

**Once per clone** — enable the pre-commit hook:

```pwsh
scripts/verify.ps1 -InstallHooks
```

**While writing code** — automatic:

- The IDE applies `.editorconfig` formatting as you type.
- Package versions are central: to add a package, add a `<PackageVersion>` to
  `Directory.Packages.props` and a version-less `<PackageReference>` to the project.

**When you build** — the analyzer ratchet runs. A warning whose code is *not* in
`warnings-baseline.md` becomes a build error. Fix it; do not add it to the baseline.

**When you commit** — the `pre-commit` hook instantly blocks a `.csproj` that
reintroduces an inline package `Version=`.

**Before you push** — the `pre-push` hook runs the fast format check. Run the full verification yourself:

```pwsh
scripts/verify.ps1              # full: restore + build + offline tests + format
scripts/verify.ps1 -SkipBuild   # fast: format check only
```

**For refactor-only changes**, run a quick consistency pass before push:

- confirm style conventions still hold in touched files (for example explicit types over `var` in new/refactored test code),
- run `scripts/verify.ps1 -SkipBuild`,
- and run at least one scoped `dotnet test` command for the directly impacted test project when behavior or assertions changed.

Exit code 0 means good to push; on failure it prints what failed. The same script is
what an agent runs to self-check, and what a CI gate should run.

| Check | Catches |
|---|---|
| Ratchet build | a new analyzer / compiler warning |
| Architecture tests | a project-layering violation |
| `Core` behavior tests | a correctness regression in a shared helper |
| Format check | style drift in a new `.cs` file |

### verify.ps1 options

| Invocation | Effect |
|---|---|
| `scripts/verify.ps1` | Restore + build + offline tests + format check |
| `scripts/verify.ps1 -SkipBuild` | Format check only (fast) |
| `scripts/verify.ps1 -InstallHooks` | Set `core.hooksPath`, then exit |
| `scripts/verify.ps1 -Base <ref>` | Diff against `<ref>` instead of `main` for the format check |

### Extending the harness

- **New layering rule** — add a `[Fact]` to `ArchitectureFitnessTests`.
- **New behavior test** — add to `VAA.EAPI.Automation.Core.Tests`.
- **Burning down a warning** — when a backlog rule reaches zero occurrences, delete its
  code from both `warnings-baseline.md` and the `WarningsNotAsErrors` list in
  `Directory.Build.props`. From then on that rule is permanently gated.

## Current state assessment

*Updated 2026-08-21. The original gap assessment was recorded on 2026-05-18; all five
roadmap threads below are now complete.*

The Automation scope now has both a strong **guide** layer and operational
**computational sensors**. The main remaining limitations are the legacy warning
backlog, live functional suites that require authorised environments and secrets, and
the optional follow-ups listed after Thread 5.

Current guides include:

- `AGENTS.md` — preferred workflow, command baseline, definition of done
- `CONTRIBUTING.md` — PR checklist, branching and commit conventions
- `skills/api-ui-automation/SKILL.md` — a scoped skill with reference docs
- `docs/` — architecture, CI map, test execution, test data, troubleshooting, local setup
- `docs/principles/test-design-principles.md` — test naming and intent conventions

Current state by regulation category:

| Category | What is operational | Remaining limitation |
|---|---|---|
| **Maintainability** | `.editorconfig`, central build/package configuration, warning ratchet, scoped formatting, pre-commit checks and the required `verify` PR gate. | The frozen legacy warning backlog still requires incremental burn-down; existing files are not mass-reformatted. |
| **Architecture fitness** | `VAA.EAPI.Automation.ArchitectureTests` enforces the project-reference graph offline and participates in `verify.ps1` and CI. | Add finer assembly/namespace rules only when a concrete repeated failure justifies them. |
| **Behavior** | `VAA.EAPI.Automation.Core.Tests` protects pure framework helpers offline; authorised pipelines run the environment-dependent functional suites. | Offline Core coverage is intentionally narrower than the live API, UI and mobile journey suites. |

### Offline and live behavior sensors

Architecture and Core helper tests now provide the fast offline feedback that was
missing in the May baseline. End-to-end suites still need live environments and
gitignored settings, so they remain controlled pipeline sensors rather than universal
local checks.

## Roadmap

Ordered cheapest-leverage first. Status: `planned` / `in progress` / `done`.

| # | Thread | Category | Status |
|---|---|---|---|
| 1a | Maintainability foundation — `.editorconfig` + `Directory.Build.props` + `Directory.Packages.props` | Maintainability | done |
| 1b | Analyzer ratchet — baseline the legacy warning backlog, then gate only on *new* warnings | Maintainability | done |
| 2 | Feedback loop — a `verify` script + pre-commit hook, referenced from `AGENTS.md` | Maintainability | done |
| 3 | Architecture fitness tests — encode `FRAMEWORK_ARCHITECTURE.md` layering rules as project-graph tests | Architecture fitness | done |
| 4 | Behavior harness for `Core` — a `Core.Tests` project unit-testing shared helpers | Behavior | done |
| 5 | PR gate — run `verify.ps1` in CI on every PR + branch protection requires the check | Maintainability | done |
| 6 | UI Page Object boundary — reject direct browser primitives in functional UI tests | Architecture fitness | done |

### UI Page Object boundary sensor

Following architectural drift found in the Search & Book Playwright suite, the architecture fitness
tests now scan functional UI test source. Tests fail when non-setup UI tests import Playwright or call
locator, role-locator, frame-locator, DOM-evaluation or interactive-pause APIs directly. Browser
mechanics belong in `Pages/` or browser infrastructure; reusable multi-page orchestration belongs in
flows/support and cross-page validation belongs in `Assertions/`. This converts the Page Object Model
from review-only guidance into a deterministic feedback control.

## Finding: the build warning channel is already saturated

Measured on a clean `dotnet build -c Release` (2026-05-18):

| Source | Count | Notes |
|---|---|---|
| `CS####` compiler warnings | ~19,400 | Pre-existing. CS8602 (null deref) alone = 10,907 — fallout of `Nullable=enable` projects with un-migrated code. |
| `xUnit####` analyzer warnings | ~4,300 | Pre-existing. Ship with the `xunit` package; xUnit1031 (blocking calls) = 3,960. |
| `CA####` analyzer warnings | ~19,300 | Added by raising `AnalysisLevel` to `latest-recommended`. |
| **Total at `latest-recommended`** | **43,172** | Reverted. |
| **Total at SDK default (current config)** | **23,890** | CS + xUnit + NuGet only; 0 errors. |

The consequence for harness engineering: **`dotnet build` warning output is not a
usable sensor today.** At ~24k baseline warnings, no agent (or human) can tell a
regression it just introduced from the legacy backlog. Raising the analysis level
deepens the flood without adding signal — and some elevated rules are *incoherent*
with this codebase (CA1707 flags underscores in identifiers, but underscore-separated
test names are a deliberate convention). `AnalysisLevel` was therefore reverted to the
SDK default. Making analyzers a real sensor requires a **baseline/ratchet** — see
Thread 1b.

## Thread 1a — Maintainability foundation

A purely computational set of controls that run inside `dotnet build` / `dotnet format`,
so every agent change becomes self-verifiable offline. Three solution-root files:

**`.editorconfig`** — *computational guide.* C# formatting and naming. IDEs apply it
live; `dotnet format --verify-no-changes` turns it into a sensor. Tuned to the existing
style (4-space indent, CRLF, UTF-8 BOM, Allman braces) so adoption does not force a
repo-wide reformat. Also resolves the inconsistency where `README.md` advertised an
`.editorconfig` that did not exist.

**`Directory.Build.props`** — *computational guide.* Centralises shared MSBuild
properties so individual `.csproj` files cannot drift:
- `Nullable` / `ImplicitUsings` defaults for new projects (existing projects all
  declare their own, so this is currently a no-op for them).
- Analyzer strengthening is **not** set here — see the finding above and Thread 1b.

**`Directory.Packages.props`** — *computational guide.* Central Package Management
(`ManagePackageVersionsCentrally`). One declared version per package across all ~25
projects. This converts version conventions — e.g. the xunit `2.9.3` /
xunit.runner.visualstudio `3.1.5` pairing — from tribal knowledge into an enforced fact.
This is the most mechanical change (every `<PackageReference Version="...">` moves to a
`<PackageVersion>` entry) and is sequenced after the first two files.

**Acceptance criteria:**
- `dotnet build VAA.EAPI.Automation.sln -c Release` succeeds (0 errors) — *met*.
- All projects inherit the shared props; no per-project version declarations remain
  (once Central Package Management lands).
- `dotnet format --verify-no-changes` is *not* an acceptance gate yet — see Thread 2.

### Progress

- **Done:** `Directory.Build.props` — `Nullable` / `ImplicitUsings` defaults only;
  `AnalysisLevel` deliberately omitted (was briefly `latest-recommended`, reverted
  after it pushed the build to ~43k warnings).
- **Done:** `.editorconfig` — tuned to the existing style; no repo-wide reformat.
- **Done:** `Directory.Packages.props` — Central Package Management enabled; 69
  package versions pinned centrally and `Version=` stripped from all 249
  `<PackageReference>` tags across 25 `.csproj` files. Two divergent packages were
  consolidated — `coverlet.collector` to 6.0.4, and `Microsoft.Extensions.Configuration`
  to 10.0.0, which (per PR review) aligned the whole 13-package `Microsoft.Extensions.*`
  / `System.*` runtime band — later bumped to 10.0.6 to patch security advisories.
  Verified: `dotnet build -c Release` succeeds
  with 0 errors and no CPM restore errors.
- **Note:** existing code will not be fully clean against the new `.editorconfig`; a
  one-time `dotnet format` pass to normalise it is a large diff and should be its own
  commit/PR.

## Thread 1b — Analyzer ratchet

The maintainability *sensor* only works once the warning channel carries signal. Rather
than asking contributors to wade through 23,890 warnings, the ratchet inverts the
problem: **everything is an error except a known, frozen backlog.**

How it is wired:

1. **Baseline** — the legacy backlog (43 codes, 23,882 warnings) is captured and counted
   in [`warnings-baseline.md`](warnings-baseline.md).
2. **Quiet incoherent rules** — rules that are wrong for this codebase are set to `none`
   in `.editorconfig` (CA1707: underscore-separated test names are deliberate).
3. **Ratchet** — `Directory.Build.props` sets `TreatWarningsAsErrors=true`, then
   `WarningsNotAsErrors` re-exempts the 41-code compiler/analyzer/NuGet backlog. (The
   baseline catalogues 43 codes; the other two — `NETSDK1206` and `MSB3245` — are
   MSBuild/SDK warnings `TreatWarningsAsErrors` cannot turn into errors.)
   Result: the legacy backlog still builds, but any **new** warning whose code is not
   on the exempt list fails the build. The SDK-generated `WorkerExtensions` project is
   excepted (generated code must not be able to break the build).
4. **Burn down** — *ongoing.* Reduce a backlog rule in scoped passes; when its count
   hits zero, delete it from both `warnings-baseline.md` and the `WarningsNotAsErrors`
   list. From then on that rule is permanently gated.

### Progress

- **Done:** ratchet wired in `Directory.Build.props`; backlog recorded in
  `warnings-baseline.md`; CA1707 quieted in `.editorconfig`.
- **Verified:** clean `dotnet build -c Release` → 0 errors, 23,890 warnings (backlog
  builds). Negative test — a project with a deliberate `CS0164` (non-exempt) →
  `error CS0164`, build failed. The sensor fires in both directions.
- **Ongoing:** step 4 (burn-down). Highest-value targets: the ~18,900 nullable warnings
  (entangled with the `Nullable` migration) and `MSB3245` (a genuine unresolved-
  reference bug, not just noise).

## Thread 2 — Feedback loop

The ratchet (Thread 1b) is a build-time sensor. Thread 2 makes it runnable as one
command and discoverable by agents.

- **`scripts/verify.ps1`** — the canonical local sensor. Runs the ratchet build
  (Release), the offline test suites, and `dotnet format --verify-no-changes` scoped
  to `.cs` files newly added since the base branch. Exit code is 0/1, so the same
  script serves an agent, a developer before pushing, and CI.
- **`.githooks/pre-commit`** — an instant check (no build): blocks a commit that
  reintroduces an inline package `Version=` into a `.csproj` (CPM drift). Enabled per
  clone with `scripts/verify.ps1 -InstallHooks`, which sets `core.hooksPath`.
- **`AGENTS.md`** now links this document and lists `verify.ps1` in the preferred
  workflow and the definition of done.

The format check is scoped to *new* `.cs` files — a file born under the harness must
be clean. Touching a legacy file does not force a whole-file reformat (Thread 4
records why this was narrowed from *changed* to *new*). A one-time `dotnet format`
normalisation of the existing tree remains a separate, optional pass.

### Progress

- **Done:** `scripts/verify.ps1`, `.githooks/pre-commit`, and the `AGENTS.md` wiring.
- **Verified:** `verify.ps1` passes on a clean tree; a deliberately misformatted file
  is correctly reported as `VERIFY FAILED: format` (exit 1).
- **Done:** `.github/workflows/automation-harness.yml` provides the dedicated CI gate;
  Thread 5 records its always-reporting required-check design and branch protection.

## Thread 3 — Architecture fitness tests

Threads 1–2 govern *how code is written*. Thread 3 governs *how the solution is
structured* — it makes the layering in `FRAMEWORK_ARCHITECTURE.md` computational.

`VAA.EAPI.Automation.ArchitectureTests` is an xUnit project that parses the solution's
`.csproj` files into a project-reference graph and asserts invariants. It needs no
environment, so it runs offline in `dotnet test`, in `verify.ps1`, and in CI.

Rules encoded — each verified true when written (2026-05-18), so each is a ratchet:

1. **Core projects depend only on Core projects** — the foundation layer is self-contained.
2. **The dependency graph is acyclic** — no project-reference cycles.
3. **UI suites are referenced only by UI or Utility projects** — the Core and API
   layers never depend on the UI layer.
4. **Utility projects are referenced by no project** — runners and tooling sit at the top.

A change that breaks a layer — e.g. making `Core` reference a domain suite — fails the
test run, and therefore `verify.ps1` and CI.

Approach note: project-graph parsing is used rather than NetArchTest. The rules are
about *project* layering, which the `.csproj` graph expresses directly. NetArchTest
(assembly/namespace dependency analysis) remains the right tool if finer
intra-project rules are added later.

### Progress

- **Done:** `VAA.EAPI.Automation.ArchitectureTests` with 4 fitness tests, added to the
  solution; `verify.ps1` runs it as a third check after the ratchet build.
- **Verified:** all 4 tests pass; the project builds clean under the ratchet.

## Thread 4 — Behavior harness for Core

The build (Threads 1–2) and the architecture tests (Thread 3) verify *form and
structure*; they cannot tell whether a helper computes the right answer. Thread 4 adds
the first offline sensor for framework *correctness*.

`VAA.EAPI.Automation.Core.Tests` is an xUnit project that unit-tests the pure helpers
in `Core` — string, collection, and gzip helpers with branching logic and no
environment, secrets, or network. It runs offline in `dotnet test` and in `verify.ps1`.

It earned its place immediately: on its first run it caught a real latent bug in
`StringExtensions.TryParse<T>`. `CacheDelegates` wired the `Parse(string)` and
`Parse(string, IFormatProvider)` methods through delegate builders written for the
`TryParse` shape (which emit an extra `out` argument), so `Expression.Call` threw
`ArgumentException` for any type exposing a `Parse` method. The effect: `TryParse<T>` /
`Parse<T>` *threw* instead of failing gracefully on invalid numeric / Guid / DateTime
input. Fixed in this thread with dedicated `CreateParseDelegate` builders — exactly the
class of defect `dotnet build` and the architecture tests cannot see.

### Progress

- **Done:** `VAA.EAPI.Automation.Core.Tests` — 47 tests across `StringExtensions`,
  `IEnumerableExtensions`, and `ByteExtensions`; added to the solution.
- **Done:** fixed the `StringExtensions.TryParse<T>` bug the harness surfaced.
- **Done:** `verify.ps1` runs the offline test suites (architecture + Core behavior)
  after the ratchet build.
- **Refinement:** the `verify.ps1` format check was narrowed from *changed* files to
  *new* files. Holding a touched legacy file to full format compliance would force an
  unrelated whole-file reformat; new files are born clean, and the legacy tree is
  normalised by a separate one-time pass.

## Thread 5 — PR gate

Threads 1–4 give us a local sensor (`scripts/verify.ps1`) that catches everything
the ratchet defines as a regression. It only fires when a contributor runs it.
Thread 5 closes that gap: the same script runs on every PR, and the GitHub
branch protection rule on `main` makes the check required before merge.

The mechanism is `.github/workflows/automation-harness.yml` at the repo root —
one job, calling `./scripts/verify.ps1 -Base origin/main` inside a
`windows-latest` runner. The CRLF-honouring `.editorconfig` rule (Thread 1a)
means Linux runners would auto-normalise on checkout and pass spuriously; the
Windows runner sees the file as the committer saw it.

**Trigger pattern — always fires, no-ops internally.** The workflow has no
`paths:` filter on its trigger; it runs on every `pull_request` against `main`
(and every `push` to `main`). A first step diffs the changed files against the
base ref and sets a `skip` output: when nothing under `VAA.EAPI.Automation/`
(or the workflow file itself) has changed, all subsequent steps are gated
`if: steps.detect.outputs.skip == 'false'` and the job completes with success
without doing any work.

This shape is mandatory for a required status check. A `paths:` filter on the
trigger would skip the workflow entirely for unrelated PRs, and GitHub treats
a *skipped* required check as **pending forever** — blocking unrelated PRs
from merging indefinitely. Always-trigger + internal-skip keeps the check
required without becoming a merge blocker for the rest of the monorepo.

**One manual step the workflow can't do for you:** once the workflow has run
once and registered the check name with GitHub, go to *Settings → Branches →
main → Branch protection rule* and tick *"Require status checks to pass before
merging"*, then add `verify` to the required list. Without that, the workflow
runs but a PR can still merge red.

### Progress

- **Done:** `.github/workflows/automation-harness.yml` — runs `verify.ps1`
  on every PR / push to `main`, with an internal change-detection step that
  no-ops the expensive work for PRs that don't touch
  `VAA.EAPI.Automation/`. Uses `concurrency` to cancel superseded runs on the
  same ref, and a NuGet cache keyed on the central package versions to
  amortise restore cost across runs.
- **Done (manual):** add `verify` as a required status check on the `main`
  branch protection rule.

### Follow-ups (out of this thread's scope)

- A `pre-push` hook in `VAA.EAPI.Automation/.githooks/` that runs the same
  verify locally before the push hits the remote. Faster feedback than CI
  for the same checks; saves a round-trip when something's wrong.
- Roslyn analyzers for the conventions that currently live only in
  `AGENTS.md` (`.Is()` over `string.Equals`, `RequireToken` over chained
  indexers, no `#region` for short blocks). Each analyzer is a separable
  unit of work and a meaningful escalation in enforcement.

## Maintenance

- Update the roadmap table status as threads progress.
- When a new guide or sensor is added, record which category and control type it is.
- This doc is linked from `AGENTS.md`; keep that link and the `verify.ps1` references
  current as the harness evolves.

## Memory And Session State

To keep automation changes resumable and auditable across agent or human sessions, this
solution follows a local memory-state contract aligned with the repository harness.

### State Contract (what must persist)

For each active automation change, keep these items visible in repository artifacts:

- active objective and current phase/thread status,
- latest required verification command and outcome,
- last failing gate (if any) and the immediate remediation step,
- accepted gaps with owner and reopen condition,
- current owner/team for next action.

State must be explicit in tracked docs, plans, PR notes, or debt entries. Hidden
session-only state is not considered valid project memory.

### Session Start (required resume checklist)

Before continuing automation work in a new session:

1. Read this file and relevant active execution-plan/debt entries.
2. Confirm branch and PR context.
3. Run fast scoped verification for the owning automation scope:
   `scripts/verify.ps1 -SolutionPath VAA.EAPI.Automation/VAA.EAPI.Automation.sln -SkipBuild`
4. If the PR is labeled `high-risk`, confirm harness-coverage fields are present and
   non-empty in the PR description.

### State Update Rules

Update state artifacts whenever any of these occur:

- verification status changes (pass -> fail or fail -> pass),
- harness gates/scripts/workflows are added, removed, or tightened,
- accepted gaps are introduced, changed, or closed,
- roadmap phase/thread status changes.

Each update should include at least: date, reason, owner, and next action.

### Validation Sensor

Use repository memory-state validation as a required computational sensor:

- local/CI command: `scripts/check-agent-memory-state.ps1`
- expected behavior: non-zero exit when required memory/state markers are stale or
  missing for an active scoped change.

When this sensor fails, fix state artifacts first, then rerun verification.

### Recovery And Drift

If session state is stale or inconsistent:

1. fail closed in CI/local harness checks,
2. restore consistency in docs/plans/debt entries,
3. rerun `scripts/check-agent-memory-state.ps1` and scoped `verify.ps1`.

Repeated state failures should be recorded in debt tracking and promoted into stronger
guides/sensors if they recur.

### Ownership And Cadence

- Primary owner: Automation Engineering maintainers.
- Review cadence: weekly via scheduled drift/parity checks, or sooner when a harness
  failure occurs.
