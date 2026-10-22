# AI Agent Guide - VAA EAPI

Read this file first before writing code or documentation. Treat it as the repository entry point; the durable rules live in `docs/`.

## Project overview

See `docs/readme.md` for the maintained project overview, repository map, common commands, reading order, and change expectations.

## Repository structure

```text
/
|-- AGENTS.md                         <- you are here
|-- docs/                             <- platform guidance, principles, standards, plans, decisions, harness docs
|-- scripts/                          <- verification and knowledge-repo scripts
|-- .codex/                           <- repo-local Codex hooks and skills
|-- .githooks/                        <- Git hook scripts installed by scripts/verify.ps1
|-- VAA.EAPI.Microservices/           <- domain microservices and shared backend libraries
|-- VAA.EAPI.GraphQL/                 <- GraphQL host, stitching, and domain resolver projects
|-- VAA.EAPI.Web/                     <- web API layer and related functions
|-- VAA.EAPI.Facade/                  <- facade/adaptor projects
|-- VAA.EAPI.Orchastration/           <- orchestration function apps
|-- VAA.EAPI.Deployments/             <- ARM deployment projects
|-- VAA.EAPI.Pipelines*/              <- Azure DevOps pipeline definitions
|-- VAA.EAPI.Automation/              <- automation and functional test support
`-- VAA.EAPI.NDC.Transformation/      <- NDC transformation projects
```

## Documentation map

Read `docs/readme.md` for the maintained documentation map. Use these entry points most often:

| File | When to read |
| --- | --- |
| `docs/readme.md` | Always, for repo orientation, docs map, reading order, and common commands. |
| `docs/principles/architecture-principles.md` | Before changing service boundaries, solution ownership, Function Apps, GraphQL, facades, or orchestration. |
| `docs/principles/integration-principles.md` | Before changing outbound HTTP, Service Bus, NDC, GraphQL integration, or external system calls. |
| `docs/principles/infrastructure-principles.md` | Before changing startup, deployment templates, app settings, APIM, pipelines, health checks, or observability. |
| `docs/principles/security-principles.md` | Before touching auth, secrets, PII, payment, passenger documents, identity, loyalty, or sensitive telemetry. |
| `docs/standards/coding-standards.md` | Before writing or reviewing code, tests, new microservices, handlers, triggers, startup, or README updates. |
| `docs/design-patterns/readme.md` | When applying reusable EAPI patterns such as thin triggers, OpenAPI contracts, host baseline, connector gateways, caching, messaging, GraphQL composition, and test harnesses. |
| `docs/exec-plans/readme.md` | When a change is large, risky, multi-step, cross-solution, or needs progress and decision history. |
| `docs/decisions/readme.md` | When a durable architecture, integration, security, delivery, or harness trade-off should be visible to future agents. |
| `docs/tech-debt-tracker.md` | When accepting a known gap or seeing a repeated review comment/failure mode. |
| `docs/harness-engineering.md` | Before running local verification, installing hooks, or changing harness behavior. |
| `docs/agent-memory.md` | Before changing session-state handling, resume behavior, or memory freshness rules. |
| `docs/knowledge-repo-guide.md` | When updating or troubleshooting the knowledge-repo flow. |

## Reading order

1. `docs/readme.md` for the current map and common commands.
2. The relevant `docs/principles/*.md` file for the change area.
3. `docs/standards/coding-standards.md` for implementation, testing, documentation, and PR expectations.
4. The relevant pattern in `docs/design-patterns/`.
5. Any relevant execution plan, decision record, or technical-debt entry.
6. Existing code in the smallest owning solution.


## Test execution requests

When a user asks to run unit tests, automation tests, smoke tests, regression tests, or a specific test filter, default to the harness-friendly path:

1. Run `dotnet test` with TRX output into a run-scoped location.
   Use `LogFileName` for a single project where the TRX path is deterministic.
   Use `LogFilePrefix` for solution or multi-project runs so parallel projects write separate TRX files instead of overwriting one another.
   For full unit-test validation outside `VAA.EAPI.Automation.sln` or CI-style runs, also add `--collect:"XPlat Code Coverage"` so Cobertura coverage output is produced.
2. Analyse only the current run: use `-TrxPath` for a single named TRX, or use a run-scoped `-ResultsDirectory` for solution/multi-project runs.
3. Report the test result, the analyser summary, and any coverage table emitted by the analyser. If the analyser cannot run, report that explicitly.

Single-project example:

```pwsh
dotnet test <project>/<project>.csproj -c Release --filter "<filter>" --results-directory TestResults\<run-name> --logger "trx;LogFileName=<run-name>.trx" --collect:"XPlat Code Coverage"
scripts/analyse-test-run-results.ps1 -TrxPath TestResults\<run-name>\<run-name>.trx -SuiteName "<suite>" -EnvironmentName "local"
```

Solution or multi-project example:

```pwsh
dotnet test <solution>.sln -c Release --results-directory TestResults\<run-name> --logger "trx;LogFilePrefix=<run-name>" --collect:"XPlat Code Coverage"
scripts/analyse-test-run-results.ps1 -ResultsDirectory TestResults\<run-name> -SuiteName "<suite>" -EnvironmentName "local"
```
If the user explicitly asks for a raw/quick test run, skip the analyser and state that no TRX analysis was run.

## Rule sources

- Scope, commands, reading order, and PR expectations: `docs/readme.md`.
- Architecture, integration, infrastructure, security, and data rules: `docs/principles/`.
- Coding, testing, scaffolding, trigger, startup, outbound HTTP, and documentation rules: `docs/standards/coding-standards.md`.
- Reusable implementation patterns: `docs/design-patterns/`.
- Execution history, durable trade-offs, and accepted gaps: `docs/exec-plans/`, `docs/decisions/`, and `docs/tech-debt-tracker.md`.
- Local verification and hook behavior: `docs/harness-engineering.md`.
- Session memory/state continuity: `docs/agent-memory.md`.
- Knowledge-repo flow: `docs/knowledge-repo-guide.md`.
