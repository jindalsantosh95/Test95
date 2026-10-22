# Execution & Infrastructure Principles
**VAA.EAPI.Automation**

---

How tests build, run, and gate is itself engineered. The suite has a harness — guides that
prevent bad output and sensors that catch it — plus the Heimdall test-runner service that
executes suites as scheduled monitoring. AI agents must keep changes self-verifiable
offline, must not weaken the gate, and must respect how the runner discovers and executes
tests. The full rationale is in [`HARNESS_ENGINEERING.md`](../HARNESS_ENGINEERING.md).

## One Gate, Run It

`scripts/verify.ps1` is the single sensor — for an agent, a developer before pushing, and
the required PR check.

- Run `scripts/verify.ps1` from the `VAA.EAPI.Automation` folder before pushing; exit 0
  means good to push.
- It runs three checks: the **analyzer-ratchet build** (`dotnet build -c Release`), the
  **offline test suites** (`ArchitectureTests` + `Core.Tests`), and a **format check** on
  newly added `.cs` files.
- The same script runs in CI (`.github/workflows/automation-harness.yml`) against
  `origin/main`. Do not let "what I ran locally" diverge from the gate — run the full
  script, not just `-SkipBuild`.
- Enable the pre-commit hook once per clone: `scripts/verify.ps1 -InstallHooks`.

## Keep Quality Left

Every check should run as early in the lifecycle as it can.

- New `.cs` files must be clean against `.editorconfig` (explicit types, no stray `var`,
  no `#region` for short blocks) — the format check fails them otherwise.
- The build treats warnings as errors except a frozen legacy backlog in
  [`docs/warnings-baseline.md`](../warnings-baseline.md). **Fix a new warning; never silence
  it by adding its code to the baseline.** Burning a backlog rule to zero removes it from
  both the baseline and `Directory.Build.props` — permanently gating it.
- Add new packages via Central Package Management: a `<PackageVersion>` in
  `Directory.Packages.props` plus a version-less `<PackageReference>`. The pre-commit hook
  blocks an inline `Version=`.

## Extend The Harness, Don't Route Around It

The sensors are meant to grow with the suite.

- New project-layering rule → add a `[Fact]` to `VAA.EAPI.Automation.ArchitectureTests`
  (see [Architecture Principles](architecture-principles.md)).
- New behaviour to protect in a shared `Core` helper → add a test to
  `VAA.EAPI.Automation.Core.Tests`. Keep these tests **offline** — no environment, secrets,
  or network — so they run in `verify.ps1` and CI.
- The live API/UI suites need real environments and secrets, so they are *not* the local
  feedback loop; do not make `verify.ps1`'s offline checks depend on them.

## Heimdall Runner

The `VAA.EAPI.Automation.Utility.TestRunner.Service` ("Heimdall agent") runs suites as a
scheduled Windows service for synthetic monitoring.

- It discovers `VAA.EAPI.Automation.*.dll`, keeps assemblies that contain xUnit tests, and
  filters by an **assembly regex + trait/filter expression** from config. A test with the
  wrong (or no) trait silently never runs — tag correctly (see
  [Test Design Principles](test-design-principles.md)).
- It can run suites in parallel; design tests to be parallel-safe and self-cleaning.
- Build artifacts must be environment-agnostic: an agent re-targets *which* tests run via
  config, with no rebuild. Do not bake an environment into a test or project.
- Runner-level concerns (browser/process cleanup, scheduling, telemetry to Application
  Insights / Event Log) live in the service — do not reimplement them in tests.

## Configuration And Pipelines

- Environment differences belong in `local.settings.json` (gitignored), pipeline variable
  groups, or config — never in committed code or docs. Confirm required keys per suite via
  [`LOCAL_SETUP.md`](../LOCAL_SETUP.md).
- Prefer solution-scoped build and targeted, trait-filtered test runs (see
  [`TEST_EXECUTION.md`](../TEST_EXECUTION.md)); full-solution runs are long and
  environment-sensitive.
- Pipelines are part of the product: when execution, gating, or CI behaviour changes,
  update the pipeline source and [`CI_PIPELINE_MAP.md`](../CI_PIPELINE_MAP.md). The
  `verify` check is a required status check on `main` — keep it green and required.

> Reference: [`HARNESS_ENGINEERING.md`](../HARNESS_ENGINEERING.md),
> [`TEST_EXECUTION.md`](../TEST_EXECUTION.md), [`LOCAL_SETUP.md`](../LOCAL_SETUP.md),
> [`CI_PIPELINE_MAP.md`](../CI_PIPELINE_MAP.md).
