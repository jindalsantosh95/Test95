# Automation Principles
**VAA.EAPI.Automation**

---

These documents define the principles AI agents and engineers should apply when changing
the `VAA.EAPI.Automation.sln` test-automation codebase. They are written from the current
shape of the suite: a multi-project .NET / xUnit solution covering API, GraphQL, and SOAP
automation, Selenium and Appium UI/mobile automation, shared `Core` libraries, test-data
tooling, the Heimdall test-runner service, and a harness-engineering verification layer.

Use these principles before designing or implementing a change. They are intentionally
normative: existing legacy tests may not always match them, but new work should move the
suite toward them without broad unrelated refactors.

## Relationship To The Platform Principles

These **extend** the EAPI platform principles (repo-root [`doc/principles/`](../../../doc/principles/README.md))
and the [EAPI Coding Standards](../../../doc/standards/coding-standards.md) for the
automation suite — they do not replace or restate them. Where a platform principle applies
(secrets resolved from Key Vault, PII classification, encryption at rest, least-privilege
identities, "use the shared abstraction not a raw client"), it holds here too and is
**not** repeated; the automation docs state only the test-suite-specific application and
deltas. The platform set is the single source of truth for cross-cutting rules.

The coding standards apply to the suite's C# as they do to any EAPI code; the automation
[Test Design](test-design-principles.md) and [Execution & Infrastructure](execution-infrastructure-principles.md)
docs and [`AGENTS.md`](../../AGENTS.md) carry only the test-suite-specific conventions on top
(explicit types, no `#region` for short blocks, the harness format gate).

## Principles Vs Reference Docs

These are the **principles** layer (what to decide, and why). The existing docs under
[`../`](..) are the **reference** layer (how things work today). Each principle links to the
reference doc that backs it rather than restating it:

- [`AGENTS.md`](../../AGENTS.md) — preferred workflow, command baseline, design conventions, definition of done
- [`FRAMEWORK_ARCHITECTURE.md`](../FRAMEWORK_ARCHITECTURE.md) — layering and change-placement rules
- [`TEST_DATA_STRATEGY.md`](../TEST_DATA_STRATEGY.md) — storage-table data, base URLs, response assertions
- [`TEST_EXECUTION.md`](../TEST_EXECUTION.md) / [`LOCAL_SETUP.md`](../LOCAL_SETUP.md) — running tests
- [`HARNESS_ENGINEERING.md`](../HARNESS_ENGINEERING.md) — the guides/sensors and `verify.ps1` gate
- [`CI_PIPELINE_MAP.md`](../CI_PIPELINE_MAP.md) — pipeline and CI behaviour

## Principle Documents

- [Test Design Principles](test-design-principles.md)
- [Architecture Principles](architecture-principles.md)
- [Test Data Principles](test-data-principles.md)
- [Integration Principles](integration-principles.md)
- [Execution & Infrastructure Principles](execution-infrastructure-principles.md)
- [Security Principles](security-principles.md)

## How AI Agents Should Use This Folder

- Read the relevant principle document before adding or changing a test, request helper,
  page object, fixture, data flow, or pipeline/runner behaviour.
- Keep changes inside the smallest project that owns the behaviour; promote to `Core` only
  when a pattern is genuinely reused across suites.
- Prefer established helpers, fixtures, page objects, and request wrappers over new local
  primitives.
- Preserve existing behaviour unless the requested change explicitly requires migration.
- When adding new code, follow the principle even if older nearby code is inconsistent —
  new files are held to the harness standard (see [Execution & Infrastructure](execution-infrastructure-principles.md)).
- Run `scripts/verify.ps1` before pushing; it is the single local sensor and the required
  PR gate.
- In PR notes, list the impacted projects, the commands run, and call out any deliberate
  exception and why it is safe.
