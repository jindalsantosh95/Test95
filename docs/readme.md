# VAA.EAPI Docs

This folder is the maintained source of truth for EAPI platform guidance, reusable engineering references, design patterns, execution plans, decision records, local harness behavior, and knowledge-repo flow for any solution in the repository. Root `AGENTS.md` points agents here; keep durable rules in this folder rather than expanding the root guide.

## Project Overview

VAA EAPI is a multi-solution .NET monorepo. It contains Azure Functions microservices, GraphQL gateway domains, facades, orchestration services, NDC transformation projects, shared infrastructure libraries, deployment projects, Azure DevOps pipelines, automation, and web-facing APIs.

Most active projects target .NET 10. Remaining .NET 5 test projects are legacy-only and must not be used as templates for new work.

## Repository Map

| Path | Purpose |
| --- | --- |
| `AGENTS.md` | Root AI-agent entry point and reading order. |
| `docs/` | Platform principles, standards, design patterns, execution plans, decision records, harness docs, sequence diagrams, and knowledge flow. |
| `scripts/` | Verification, deployment coverage, knowledge health, and knowledge-repo scripts. |
| `.codex/` | Repo-local Codex hooks, environments, and skills. |
| `.githooks/` | Git hooks installed by `scripts/verify.ps1 -InstallHooks`. |
| `.github/copilot-instructions.md` | GitHub Copilot guidance. |
| `VAA.EAPI.Microservices/` | Domain microservices and shared backend libraries. |
| `VAA.EAPI.GraphQL/` | GraphQL host, stitching, and domain resolver projects. |
| `VAA.EAPI.Web/` | Web API layer and related function apps. |
| `VAA.EAPI.Facade/` | Facade and adapter projects. |
| `VAA.EAPI.Orchastration/` | Orchestration function apps. |
| `VAA.EAPI.Deployments/` | Service deployment projects and ARM templates. |
| `VAA.EAPI.Pipelines/`, `VAA.EAPI.Pipelines.DigitalServices/` | Azure DevOps pipeline definitions. |
| `VAA.EAPI.Automation/` | Automation and functional test support. |
| `VAA.EAPI.NDC.Transformation/` | NDC transformation projects. |

## Common Commands

Locate solution files from the repo root and choose the smallest solution that owns the change:

```bash
find . -name '*.sln'
```

Use solution-scoped commands:

```bash
dotnet restore <solution>.sln
dotnet build <solution>.sln -c Release
dotnet test <solution>.sln -c Release --results-directory TestResults/<run-name> --logger "trx;LogFilePrefix=<run-name>" --collect:"XPlat Code Coverage"
scripts/analyse-test-run-results.ps1 -ResultsDirectory TestResults/<run-name> -SuiteName "<suite>" -EnvironmentName "local"
```

Use the local harness when feasible:

```pwsh
scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild
scripts/verify.ps1 -ProjectPath <project>.csproj -SkipBuild
scripts/verify-staged.ps1 -SkipBuild
scripts/check-knowledge-health.ps1
scripts/check-harness-rules.ps1
scripts/check-agent-memory-state.ps1
scripts/check-agent-tool-environment.ps1 -Profile docs-local -Probe
scripts/invoke-agent-observability-exercise.ps1 -EvidenceDirectory TestResults/agent-observability/manual
scripts/invoke-harness-maturity-evidence-exercise.ps1 -EvidenceDirectory TestResults/harness-maturity/manual
scripts/check-agent-resource-budget.ps1 -ValidateConfiguration
scripts/invoke-agent-resource-budget-exercise.ps1 -EvidenceDirectory TestResults/agent-resource-budget/manual
scripts/check-context-routing.ps1 -MaxAgeDays 30
scripts/check-harness-remediation-log.ps1 -MaxReviewAgeDays 14
scripts/check-sequence-diagram-status.ps1
scripts/check-mermaid-diagrams.ps1 -BaseRef origin/main
scripts/invoke-mermaid-validation-exercise.ps1
scripts/promote-sequence-goal.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md -ToStatus InProgress
scripts/run-sequence-goal-agent.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md
scripts/verify.ps1 -InstallHooks
```

Avoid building or testing from the repository root unless there is a specific reason. Unit-test requests must use run-scoped TRX evidence and the analyser; see [Harness Engineering](harness-engineering.md#unit-test-execution-contract).

## Documentation Map

| File | When to read |
| --- | --- |
| [Principles](principles/readme.md) | Before designing or implementing platform changes. |
| [Architecture Principles](principles/architecture-principles.md) | Service boundaries, GraphQL composition, facades, orchestration, Function Apps, and shared libraries. |
| [Integration Principles](principles/integration-principles.md) | Outbound HTTP, `IHttpConnector`, Service Bus, external systems, NDC, GraphQL integration, and contract testing. |
| [Darwinium Fraud Notification API Integration Guide](integration-guides/darwinium-fraud-notification-api.md) | Partner-facing OAuth, `POST /FraudAlert` request validation, `api-correlation-id` response header, asynchronous acceptance, retry, support, and onboarding contract for the Darwinium fraud-notification integration. |
| [CRM Email Template Contracts](email-templates/readme.md) | EAPI trigger, recipient, merge-field, and copy contracts for templates provisioned in the CRM email platform. |
| [Retail Holidays TravelBox DMAZE Flow](sequence-diagrams/vaa-eapi-retail-holidays-travelbox-dmaze.md) | Before designing or implementing TravelBox capabilities in the Retail Holidays microservice. |
| [Native Search and Book — All Scenarios](sequence-diagrams/vaa-eapi-native-search-and-book-all-scenarios.md) | Before designing or implementing native Search and Book journeys across Search GraphQL, Book GraphQL, and their downstream microservices. |
| [Infrastructure Principles](principles/infrastructure-principles.md) | Startup, middleware, app settings, deployment templates, APIM, pipelines, observability, and local development. |
| [Security Principles](principles/security-principles.md) | Auth, secrets, PII, payment, passenger documents, identity, loyalty, secure failure, and auditability. |
| [Data Principles](principles/data-principles.md) | Data ownership, persistence, retention, identifiers, and data lifecycle. |
| [Coding Standards](standards/coding-standards.md) | Coding, testing, microservice scaffolding, triggers, startup, outbound HTTP, documentation, and PR expectations. |
| [Design Patterns Catalog](design-patterns/readme.md) | Reusable EAPI patterns for implementation, integration, runtime, GraphQL, messaging, and testing. |
| [Execution Plans](exec-plans/readme.md) | Versioned plans for large, risky, or multi-step changes. |
| [Decision Records](decisions/readme.md) | Durable architecture, integration, security, delivery, and harness decisions. |
| [Technical Debt Tracker](tech-debt-tracker.md) | Visible debt that affects agent legibility, harness coverage, or repeated review effort. |
| [Configuration Reference](configuration-reference.md) | Configuration overlays, app settings, and key lookup behavior. |
| [Startup and Dependency Injection](startup-and-di.md) | Startup and DI guidance for repository solution references. |
| [Triggers and Endpoints](triggers-and-endpoints.md) | Trigger and endpoint guidance for repository solution references. |
| [Workflows](workflows.md) | Workflow reference guidance for repository solution references. |
| [Workflow Failure Tracing](workflow-failure-tracing.md) | Failure-only TRX manifests, local Azure enrichment, operator prerequisites, and the pattern for enrolling additional workflows. |
| [Agent Loop Workflow](agent-loop-workflow.md) | Automated sequence-diagram delivery loop and status lifecycle. |
| [Sequence Diagrams Index](sequence-diagrams.md) | Orchestration sequence diagrams and flow references. |
| [Behavior Harness](behavior-harness.md) | Executable behavior sensors linked to sequence diagrams. |
| [Harness Capabilities Matrix](harness-capabilities-matrix.md) | Quick-read matrix of agent skills, executable checks, CI gates, and advisory harness capabilities. |
| [Harness Contract](harness-contract.md) | Harness controls matrix and policies. |
| [Harness Coverage](harness-coverage.md) | Coverage model for mapping important risks to guides and sensors. |
| [Harness Engineering](harness-engineering.md) | Local verification, hook behavior, Codex hooks, and harness commands. |
| [Harness Overview](harness-overview.md) | High-level briefing and diagrams for the platform harness, Automation harness, supported themes, and agent delivery flow. |
| [Agent Observability](agent-observability.md) | Contract for safe agent-readable logs, metrics, traces, correlation, and runtime-to-harness feedback. |
| [Agent Tool And Executable Environments](agent-tool-and-executable-environments.md) | Machine-checked command, isolation, network, secret, evidence, and cleanup profiles for agent execution. |
| [Continuous Harness Maturity](harness-continuous-maturity.md) | Retained scheduled-run ledger and derived consecutive-cycle maturity evidence. |
| [Documentation Gardening](doc-gardening.md) | Computational and inferential documentation-drift lifecycle, ownership, SLA, and evidence. |
| [Current SDLC Flow](current-sdlc.md) | Environment-focused delivery lifecycle from DEV through SIT and UAT to PROD, including gates and recovery. |
| [Agent Execution And Recovery](agent-execution-and-recovery.md) | Durable run records, checkpoints, idempotency, retry, replay, resource budgets, and escalation. |
| [Agent Resource And Cost Controls](agent-resource-and-cost-controls.md) | Required per-run token/cost ceilings, rate-card governance, retained evidence, and rolling 7-day enforcement. |
| [Operational Harness Registry](harness-operational-capabilities.json) | Machine-readable enrollment, evidence cycles, owners, gaps, and maturity state for operational capabilities. |
| [Context Routing Operating Guidance](context-routing-operating-guidance.md) | Required context fields, scope routing map, and freshness rules for high-risk operational guidance. |
| [Agent Memory](agent-memory.md) | Tracked session-state contract, optional local resume state, and handoff flow. |
| [Harness Definition Of Done](harness-definition-of-done.md) | Measurable harness themes, ownership, SLA windows, and RAG scoring. |
| [Harness Pipeline Checklist](harness-pipeline-checklist.md) | CI parity, pipeline gates, drift checks, and failure policy. |
| [Harness Remediation Log](harness-remediation-log.md) | Branch-protection confirmations, drift review evidence, and remediation closure tracking. |
| [Knowledge Repo Guide](knowledge-repo-guide.md) | Knowledge update flow, hook behavior, and entry fields. |
| [Knowledge Repo Log](knowledge-repo.md) | Local change log generated by the knowledge-repo scripts. |

## Solution References

Use these documents as solution-level reference templates. Keep entries scoped to the solution or project being changed, and avoid turning the root docs into ownership documentation for a single implementation.

- [Configuration Reference](configuration-reference.md)
- [Startup and Dependency Injection](startup-and-di.md)
- [Triggers and Endpoints](triggers-and-endpoints.md)
- [Workflows](workflows.md)
- [Workflow Failure Tracing](workflow-failure-tracing.md)
- [Agent Loop Workflow](agent-loop-workflow.md)
- [Sequence Diagrams Index](sequence-diagrams.md)

## Harness And Delivery

- [Behavior Harness](behavior-harness.md)
- [Harness Capabilities Matrix](harness-capabilities-matrix.md)
- [Harness Contract](harness-contract.md)
- [Harness Coverage](harness-coverage.md)
- [Harness Engineering](harness-engineering.md)
- [Agent Observability](agent-observability.md)
- [Agent Tool And Executable Environments](agent-tool-and-executable-environments.md)
- [Continuous Harness Maturity](harness-continuous-maturity.md)
- [Documentation Gardening](doc-gardening.md)
- [Current SDLC Flow](current-sdlc.md)
- [Agent Execution And Recovery](agent-execution-and-recovery.md)
- [Agent Resource And Cost Controls](agent-resource-and-cost-controls.md)
- [Context Routing Operating Guidance](context-routing-operating-guidance.md)
- [Agent Memory](agent-memory.md)
- [Harness Definition Of Done](harness-definition-of-done.md)
- [Harness Pipeline Checklist](harness-pipeline-checklist.md)
- [Harness Remediation Log](harness-remediation-log.md)
- [Technical Debt Tracker](tech-debt-tracker.md)

## Design Pattern Entry Points

Use [Design Patterns Catalog](design-patterns/readme.md) first, then the specific pattern:

- API boundary: thin triggers, OpenAPI contracts, response envelopes, and error mapping.
- Platform runtime: Function host baseline, GraphQL domain startup, GraphQL host topology, and operational endpoints.
- Integration: connector-backed gateways, token-brokered calls, facade adapters, and validation-to-mapping pipelines.
- Workflow and messaging: CloudEvents, delayed dispatcher recovery, event store rollback, and subscriber context rehydration.
- Testing and operations: EAPI Function test harness and HTTP mocking with `MockDelegatingHandler`.

## Agent Commands And Skills

- `/new-microservice [domain and name]`: use `.codex/skills/new-microservice/` to scaffold a bare-bones Azure Functions microservice, sibling test project, deployment project, OpenAPI/swagger attributes, and APIM-aware pipeline workflow.
- To abandon one or more sequence-goal deliveries and rebuild their branches as clean Drafts, use `.codex/skills/sequence-goal-reset/`. The skill inventories related PRs, branches, delivery changes, and evidence before any explicitly authorized destructive action.

## Reading Order For AI Agents

1. Read `AGENTS.md`.
2. Read this file.
3. Read the principle document for the change area.
4. Read `standards/coding-standards.md`.
5. Read any relevant `design-patterns/*.md`.
6. For large, risky, or cross-cutting work, check `exec-plans/`, `decisions/`, and `tech-debt-tracker.md`.
7. Inspect existing code in the smallest owning solution and project.
8. Run or report solution/project-scoped verification.

## Change Expectations

- Keep changes inside the owning solution and project whenever possible.
- Update deployment and pipeline files when runtime behavior, configuration, infrastructure, APIM, or release wiring changes.
- Update the owning service `README.md` when local setup, configuration, endpoints, triggers, or operational behavior changes.
- Update these docs when a change introduces or changes durable platform guidance.
- Use an execution plan for large, risky, or multi-step changes that need progress and decision history.
- Capture durable trade-offs in decision records, and record repeated review findings or accepted gaps in the technical debt tracker.
- Keep PR descriptions short but explicit: summary, impacted services, and tests or verification commands run.
