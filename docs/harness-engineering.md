# Harness Engineering

This document defines the local harness pattern for repository solutions and projects.

## Scope And Intent

Repository parity batches resolved scopes into at most 200 matrix jobs so repository-wide changes stay below GitHub Actions' matrix limit. Every scope runs sequentially within its batch, failures are collected, and any failed scope fails the gate. The workflow passes its fetched base ref explicitly to verification. `scripts/invoke-verify-matrix-exercise.ps1` checks boundary sizes and that batching retains every scope exactly once.

Harness engineering here means: keep changes safe with fast, local, computational checks and clear operational docs.

Policy terms used in this document:

- `required`: blocking control for merge readiness in owning scope.
- `advisory`: non-blocking control used for additional assurance and trend review.
- `guidance`: feedforward context that shapes behavior but does not block.

Current harness priorities:

- maintainability: build and format checks,
- behavior: targeted tests,
- operational continuity: pre-commit knowledge updates,
- agent legibility: knowledge health checks for required docs, links, and generated knowledge entries,
- memory continuity: a tracked state contract plus optional worktree-local resume state,
- deployment readiness: project and infrastructure checks where applicable,
- harness coverage: visible mapping from important risks to guides and sensors.

Sequence-goal delivery opens a draft coordinator PR as soon as a Ready goal becomes InProgress, then isolates every production and QA agent in its own named job, worktree, owning-repository branch, pull request, logs, and resource evidence. Production surfaces run in parallel; each records its PR and immediately dispatches only its matching QA continuation, so QA does not wait for another surface or for production merge. Every QA PR is bound to the exact production repository, PR number, and head SHA it verified, preventing a replaced production PR from inheriting stale QA. Coordinator branches retain only goal and lifecycle evidence. After all current production and matching QA PRs merge and integrated acceptance verification passes, the goal becomes Complete and the existing coordinator PR becomes ready for review.

See [`harness-contract.md`](harness-contract.md) for the explicit controls matrix.
See [`harness-capabilities-matrix.md`](harness-capabilities-matrix.md) for a quick-read matrix of agent skills, executable checks, CI gates, and advisory capabilities.
See [`harness-coverage.md`](harness-coverage.md) for the coverage model used to decide whether the harness is protecting the right risks.
See [`harness-definition-of-done.md`](harness-definition-of-done.md) for measurable completion criteria, ownership, SLA windows, and weekly RAG scoring.
See [`harness-pipeline-checklist.md`](harness-pipeline-checklist.md) for CI parity and pipeline execution gates.
See [`behavior-harness.md`](behavior-harness.md) for the pattern that pins diagrams and workflows to executable sensors.
See [`agent-observability.md`](agent-observability.md) for safe runtime evidence and runtime-to-harness feedback.
See [`agent-tool-and-executable-environments.md`](agent-tool-and-executable-environments.md) for machine-checked tools, worktree isolation, network/secrets policy, and cleanup.
See [`harness-continuous-maturity.md`](harness-continuous-maturity.md) for scheduled evidence retention and derived maturity promotion.
See [`doc-gardening.md`](doc-gardening.md) for computational and inferential documentation drift control.
See [`agent-execution-and-recovery.md`](agent-execution-and-recovery.md) for durable runs, retries, replay limits, budgets, and recovery.
See [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md) for per-run token/cost enforcement, pricing governance, retained evidence, and rolling 7-day ceilings.

## What Is Implemented Now

GitHub Actions that implement documentation or harness controls use the `ai-harness-` prefix, with each display name matching its lowercase kebab-case filename stem. Internal job/check names remain stable so branch-protection rules are unaffected.

### Guides

- [`AGENTS.md`](../AGENTS.md) points agents to the repository documentation map.
- [`readme.md`](readme.md), [`standards/coding-standards.md`](standards/coding-standards.md), and [`principles`](principles/readme.md) define project workflow and definition of done.
- [`exec-plans`](exec-plans/readme.md), [`decisions`](decisions/readme.md), and [`tech-debt-tracker.md`](tech-debt-tracker.md) capture large work plans, durable trade-offs, and accepted gaps.
- [`agent-memory.md`](agent-memory.md) defines required cross-session memory state and freshness rules.
- This file documents verification controls and git-hook operations.
- [`agent-observability.md`](agent-observability.md), [`agent-tool-and-executable-environments.md`](agent-tool-and-executable-environments.md), [`harness-continuous-maturity.md`](harness-continuous-maturity.md), [`doc-gardening.md`](doc-gardening.md), [`agent-execution-and-recovery.md`](agent-execution-and-recovery.md), and [`agent-resource-and-cost-controls.md`](agent-resource-and-cost-controls.md) define runtime evidence, executable environments, continuous maturity, knowledge maintenance, autonomous execution, and resource budgets.

### Sensors

- `dotnet build <solution-or-project> -c Release`
- test projects discovered under the selected scope, or supplied via `-TestProjectPath`
- `scripts/check-deployment-projects.ps1`
- `scripts/check-knowledge-health.ps1`
- `scripts/check-harness-rules.ps1`
- `scripts/check-agent-memory-state.ps1`
- `scripts/update-agent-memory-state.ps1`
- `scripts/invoke-agent-memory-lifecycle-exercise.ps1`
- `scripts/check-context-routing.ps1`
- `scripts/check-docs-harness-support.ps1`
- `scripts/check-operational-harness.ps1`
- `scripts/check-agent-workflow-controls.ps1`
- `scripts/check-agent-tool-environment.ps1`
- `scripts/invoke-agent-tool-environment-exercise.ps1`
- `scripts/query-agent-observability.ps1`
- `scripts/invoke-agent-observability-exercise.ps1`
- `scripts/check-harness-maturity-evidence.ps1`
- `scripts/invoke-harness-maturity-evidence-exercise.ps1`
- `scripts/check-agent-resource-budget.ps1`
- `scripts/check-agent-resource-trend.ps1`
- `scripts/invoke-agent-resource-budget-exercise.ps1`
- `scripts/check-agent-resource-budget-strength.ps1`
- `scripts/check-architecture-fitness.ps1`
- `scripts/invoke-architecture-fitness-mutation-exercise.ps1`
- `scripts/check-preferred-extensions.ps1`
- `scripts/check-ui-automation-standards.ps1`
- `scripts/check-harness-remediation-log.ps1`
- `.github/workflows/ai-harness-high-risk-autolabel.yml`
- `.github/workflows/review-team-labels.yml`
- `.github/review-team-labels.json`
- `.github/workflows/ai-harness-closed-loop-autonomy.yml`
- `.github/workflows/ai-delivery-sequence-goal-pr-review-loop.yml`
- `.github/workflows/ai-delivery-sequence-goal-human-comment-loop.yml`
- `.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml`
- `.github/workflows/ai-delivery-sequence-goal-loop.yml`
- `scripts/save-sequence-goal-delivery.ps1` (publishes agent-owned goal entries with optimistic Git retries)
- `.github/workflows/ai-delivery-sequence-goal-qa.yml` (bounded per-surface QA entry point)
- `scripts/save-sequence-goal-resources.ps1` (records cost history in the goal document with concurrent-update retries)
- `scripts/check-sequence-diagram-status.ps1`
- `scripts/promote-sequence-goal.ps1`
- `scripts/open-sequence-goal-coordinator-pr.ps1`
- `scripts/run-sequence-goal-agent.ps1` (requires `OPENAI_API_KEY` and `SEQUENCE_GOAL_AGENT_COMMAND_JSON` in CI; supports `SEQUENCE_GOAL_AGENT_MODEL`)
- `scripts/run-sequence-goal-pr-review-agent.ps1` (requires `OPENAI_API_KEY` and `SEQUENCE_GOAL_REVIEW_AGENT_COMMAND_JSON` in CI; supports `SEQUENCE_GOAL_REVIEW_AGENT_MODEL`)
- `scripts/run-sequence-goal-human-comment-agent.ps1` (requires `OPENAI_API_KEY` and `SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON` in CI; supports `SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL`)
- `scripts/verify-staged.ps1 -SkipBuild`
- `dotnet format --verify-no-changes` on newly added `.cs` files under the selected scope.
- Sequence-goal automation runs `scripts/format-generated-csharp.ps1 -Base origin/main` before initial verification and again during remediation. It resolves each newly added C# file to its nearest owning project and applies `dotnet format --include` only to those generated files.

## Use The Harness

Run from the repository root with the smallest relevant solution:

```pwsh
scripts/verify.ps1 -SolutionPath path/to/Solution.sln
```

Fast mode:

```pwsh
scripts/verify.ps1 -SolutionPath path/to/Solution.sln -SkipBuild
```

Project-scoped mode:

```pwsh
scripts/verify.ps1 -ProjectPath path/to/Project.csproj -SkipBuild
```

Staged-file mode:

```pwsh
scripts/verify-staged.ps1 -SkipBuild
```

Knowledge-health mode:

```pwsh
scripts/check-knowledge-health.ps1
```

Harness-rules mode:

```pwsh
scripts/check-harness-rules.ps1
```

Agent-memory contract, lifecycle, and deterministic cross-process resume:

```pwsh
scripts/check-agent-memory-state.ps1
scripts/check-agent-memory-state.ps1 -Initialize
scripts/update-agent-memory-state.ps1 -Action Checkpoint -ScopeKind ScopePath -ScopePath docs -NextActions 'Run scoped verification'
scripts/invoke-agent-memory-lifecycle-exercise.ps1
```

Context-routing mode:

```pwsh
scripts/check-context-routing.ps1 -MaxAgeDays 30
```

Docs harness support mode:

```pwsh
scripts/check-docs-harness-support.ps1
```

Operational capability enrollment and evidence mode:

```pwsh
scripts/check-operational-harness.ps1
scripts/check-operational-harness.ps1 -RequireMature -MaturityLedgerPath <retained-ledger.json> -MaturitySummaryPath <retained-summary.json>
```

Agent resource-budget configuration and exercise mode:

```pwsh
scripts/check-agent-resource-budget.ps1 -ValidateConfiguration
scripts/invoke-agent-resource-budget-exercise.ps1 -EvidenceDirectory TestResults/agent-resource-budget/manual
scripts/check-agent-resource-budget-strength.ps1 -EvidencePath TestResults/agent-resource-budget/manual/summary.json
```

Tool, observability, and continuous-maturity exercise mode:

```pwsh
scripts/check-agent-tool-environment.ps1 -Profile docs-local -Probe
scripts/invoke-agent-tool-environment-exercise.ps1 -EvidenceDirectory TestResults/agent-tool-environment/manual
scripts/invoke-agent-observability-exercise.ps1 -EvidenceDirectory TestResults/agent-observability/manual
scripts/invoke-harness-maturity-evidence-exercise.ps1 -EvidenceDirectory TestResults/harness-maturity/manual
```

Preferred-extension mode:

```pwsh
scripts/check-preferred-extensions.ps1 -BaseRef main -ScopePath path/to/project
```

Unit-test standards mode:

```pwsh
scripts/check-test-standards.ps1 -BaseRef main -ScopePath path/to/test-project
```

New UI test files are also checked for the mandatory journey-flow boundary. Tests cannot construct
`PlaywrightBrowserSession`, access the browser page, or call browser primitives directly:

```pwsh
scripts/check-ui-automation-standards.ps1 -BaseRef main -ScopePath VAA.EAPI.Automation
```

High-risk autolabelling fails closed when its token cannot add the label. It requests both pull
request and issue write access because a pull request label is applied through the Issues API; a
green label-routing check therefore means semantic review was actually enrolled, not merely
detected and skipped after a permissions warning.

The high-risk inferential review builds its review bundle in trusted workflow code before Codex
starts. The prompt contains the changed-file list, repository and Automation agent guides, coding
standards, root and Automation principle documents, and complete PR diff. Guidance and policy files
come from the trusted base-branch snapshot; only the proposed diff comes from the PR. Codex is
instructed not to invoke shell commands or other tools.
This preserves the read-only sandbox on hosted runners where nested `bwrap` network setup is not
permitted. The workflow rejects `BLOCKED`, `ERROR`, or `FAILED` output so an agent startup or
inspection failure cannot be published as a successful semantic review.

This changed-code sensor rejects newly introduced Moq APIs in new or amended unit tests, including untracked test files. For HTTP-backed tests it also rejects ad hoc `IHttpConnector` implementations and instance registrations, requires the real `HttpConnector` plus shared xUnit `IHttpClientFactory` to be registered through `ServiceCollection`, and verifies that the owning test project has a checked-in or linked `.mock` resource. When an HTTP/resource harness is genuinely not applicable, a small hand-written fake or stub may be used for a non-HTTP boundary. Existing untouched legacy Moq tests are not broadened into the change scope.

`scripts/invoke-test-standards-exercise.ps1` protects the detector against common syntax bypasses, including alias and global imports, fully qualified Moq names, and generic, member, or call-shaped Moq APIs. It also checks representative non-Moq fixture/resource code to limit false positives.

This required sensor blocks newly added `string.Equals(..., StringComparison.OrdinalIgnoreCase)` and `string.IsNullOrWhiteSpace(...)` calls. Projects that reference `VAA.EAPI.Common.Extensions` must use shared helpers such as `.Is(...)` and `.IsNullOrWhiteSpace()` so string semantics remain consistent. The shared extension implementation itself is exempt.

`scripts/run-sequence-goal-agent.ps1` injects a broader, solution-aware extension requirement as repository-owned input on both initial generation and remediation runs. Before using native .NET APIs or adding helper logic, the agent must inspect the owning project's reference graph and applicable extension classes. The string patterns above are executable examples of that general policy, not an exhaustive extension allowlist. This keeps the rule effective even when the externally configured `SEQUENCE_GOAL_AGENT_COMMAND_JSON` prompt is stale.

The sequence-goal runner also injects a generated C# layout rule into every production and automation phase, including initial generation and remediation runs. Each top-level class, record, struct, enum, or interface must be written to its own `.cs` file named after the type instead of bundling several top-level types into one generated file. Nested implementation-detail types may remain with their owner.

Full sequence-goal verification requires at least one resolved project or solution scope. When a change also resolves a repository fallback scope (for example, because goal evidence or documentation changed), the workflow records that fallback as covered by the Full project or solution verification instead of running or rejecting it separately. The same rule applies after automated remediation.

When the delivery loop creates or refreshes a pull request, its description includes the maximum budget and actual token/cost labels retained in the goal document. This keeps the resource evidence visible on the delivery artifact alongside the goal document and diagnostic artifacts.

Remediation-log mode:

```pwsh
scripts/check-harness-remediation-log.ps1 -MaxReviewAgeDays 14
```

Sequence-diagram status mode:

```pwsh
scripts/check-sequence-diagram-status.ps1
```

Mermaid validation first checks sequence-diagram authoring rules without external dependencies, then uses the pinned Mermaid CLI through Node.js and `npx` to render every selected fence into a temporary SVG. Raw semicolons in sequence statements are rejected with the Markdown line and block number: use a comma, `<br/>`, or the `#59;` entity. Comments and Mermaid entity terminators are allowed; flowchart statement separators and ordinary Markdown prose are unaffected. This authoring rule deliberately requires one sequence statement per line.

`scripts/verify.ps1`, including `-SkipBuild`, runs the authoring lint on changed Markdown and its regression exercise. `-LintOnly` on the checker or exercise runs without Node and explicitly does not establish parser/render success. Full rendering remains required authoring evidence and is enforced by the Mermaid CI workflow. When `npx` is missing, report rendering as blocked and rerun the full check in a Node-enabled environment.

Run full validation with:

```pwsh
scripts/check-mermaid-diagrams.ps1 -BaseRef origin/main
scripts/check-mermaid-diagrams.ps1 -Path docs/sequence-diagrams/<diagram-file>.md
scripts/invoke-mermaid-validation-exercise.ps1
```

The sequence-goal delivery, AI-review, and human-comment workflows run this check against their generated Markdown. The `ai-harness-mermaid-validation` PR action also checks every changed Markdown file and runs the exercise. The exercise covers notes, messages, participant labels, branch text, later blocks, safe entities, comments, prose and flowchart separators; full mode also renders safe fixtures. Workflow display names use lowercase kebab-case; the existing comment monitor remains `ai-delivery-sequence-goal-comment-monitor`.

Paths supplied with `-Path` are repository-relative Markdown files. Explicit paths fail closed when they are rooted, traverse outside the repository, do not identify an existing file, or identify a non-Markdown file. Node.js is only required when fence discovery finds at least one Mermaid block to render.

Base-ref discovery uses a three-dot merge-base comparison so updated base-branch-only Markdown is not attributed to the delivery branch. Manual workflow runs use `HEAD~1` when available and safely fall back to `HEAD` for single-commit histories.

Sequence-diagram authoring template:

```text
docs/sequence-diagrams/_template.sequence-diagram.md
```

Sequence-goal promotion mode:

```pwsh
scripts/promote-sequence-goal.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md -ToStatus InProgress
scripts/promote-sequence-goal.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md -ToStatus Complete
```

Sequence-goal agent mode:

```pwsh
$env:OPENAI_API_KEY = '<set locally or configure GitHub secret>'
# scripts/run-sequence-goal-agent.ps1 maps OPENAI_API_KEY to CODEX_API_KEY for codex exec.
# The runner script writes ephemeral Codex config and normalizes launch args to --dangerously-bypass-approvals-and-sandbox before exec in the isolated GitHub Actions runner.
$env:SEQUENCE_GOAL_AGENT_MODEL = 'gpt-6-sol'
$env:SEQUENCE_GOAL_AGENT_COMMAND_JSON = '{"executable":"codex","args":["--dangerously-bypass-approvals-and-sandbox","exec","--model","{model}","Implement the sequence-goal diagram at {diagram} on branch {branch}. Repository root: {repoRoot}. First read AGENTS.md, docs/readme.md, docs/standards/coding-standards.md, docs/agent-loop-workflow.md, and the target diagram. Use the diagram''s Agent delivery constraints as the primary implementation brief. Limit discovery to the diagram-defined target paths, owning solution/project files, referenced source files, and nearest tests unless a compile/test failure proves more context is required. Make the smallest valid code, test, and documentation changes. You must create or update substantive implementation/test files under the appropriate VAA.EAPI.* project path; .codex/agent-loop evidence and diagram edits alone are not a delivered goal. Implement all settled requirements and preserve genuine Open points to finalize for human input on the delivery PR. Do not invent decisions merely to close them. If the settled scope cannot be implemented safely, exit non-zero with the blocker instead of producing evidence-only output. Update evidence fields and leave the goal ready for verification."]}'
scripts/run-sequence-goal-agent.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md
```

Sequence-goal PR review mode:

```pwsh
$env:OPENAI_API_KEY = '<set locally or configure GitHub secret>'
# scripts/run-sequence-goal-pr-review-agent.ps1 maps OPENAI_API_KEY to CODEX_API_KEY for codex exec.
$env:SEQUENCE_GOAL_REVIEW_AGENT_MODEL = 'gpt-6-sol'
$env:SEQUENCE_GOAL_REVIEW_AGENT_COMMAND_JSON = '{"executable":"codex","args":["--dangerously-bypass-approvals-and-sandbox","exec","--model","{model}","Review PR {prNumber} using {reviewEvidence} on branch {branch}. Repository root: {repoRoot}. Apply valid AI review fixes, leave invalid or not-applicable comments unchanged except for evidence, and record valid/invalid/not-applicable decisions plus resolutionComment in the evidence file."]}'
scripts/run-sequence-goal-pr-review-agent.ps1 -PrNumber <number> -ReviewEvidencePath .codex/agent-loop/pr-reviews/pr-<number>-<run-id>.json
```

Sequence-goal human comment mode:

```pwsh
$env:OPENAI_API_KEY = '<set locally or configure GitHub secret>'
# scripts/run-sequence-goal-human-comment-agent.ps1 maps OPENAI_API_KEY to CODEX_API_KEY for codex exec.
$env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_MODEL = 'gpt-6-sol'
$env:SEQUENCE_GOAL_HUMAN_COMMENT_AGENT_COMMAND_JSON = '{"executable":"codex","args":["--dangerously-bypass-approvals-and-sandbox","exec","--model","{model}","Review non-AI PR comments for PR {prNumber} using {commentEvidence} on branch {branch}. Repository root: {repoRoot}. Agree or disagree, action agreed comments, and record responseComment/actionSummary without resolving human review threads."]}'
scripts/run-sequence-goal-human-comment-agent.ps1 -PrNumber <number> -CommentEvidencePath .codex/agent-loop/human-comments/pr-<number>-<run-id>.json
```

Strict repository-link mode:

```pwsh
scripts/check-knowledge-health.ps1 -CheckRepositoryLinks
```

Install hooks once per clone:

```pwsh
scripts/verify.ps1 -InstallHooks
```

## Unit-Test Execution Contract

Unit-test requests use evidence from the current run only:

1. Choose the smallest owning `.csproj` or `.sln`; never issue an unscoped repository-root test command.
2. Create a unique `TestResults/<run-name>` directory. For a single project use `trx;LogFileName=<run-name>.trx`; for a solution or multiple projects use `trx;LogFilePrefix=<run-name>` so parallel writers cannot overwrite one another.
3. For full validation outside `VAA.EAPI.Automation.sln` and for CI-style runs, add `--collect:"XPlat Code Coverage"`.
4. Run `scripts/analyse-test-run-results.ps1` with the exact `-TrxPath` for one named TRX or the exact run-scoped `-ResultsDirectory` for multi-project output.
5. Report the `dotnet test` result, analyser summary, and analyser coverage table. If an expected TRX or the analyser is unavailable, fail the harness step and report why.

`scripts/verify.ps1` implements this contract for every discovered test project. A raw/quick test run may skip evidence analysis only when the requester explicitly asks for it; state that no TRX analysis was run.

## Verification Expectations

- Keep verification scoped to the smallest solution or project that owns the change.
- Prefer `scripts/verify.ps1 -SolutionPath <solution>.sln -SkipBuild` for fast checks (`required`).
- Omit `-SkipBuild` for the full build/test harness when feasible (`required` before merge for owning scope).
- Use `scripts/verify.ps1 -ProjectPath <project>.csproj -SkipBuild` for project-scoped checks (`required`).
- If tests or verification cannot be run, record why in the final response or PR notes.
- Do not run root-level `dotnet build` or `dotnet test` across the whole repository unless there is a specific reason.
- Every orchestration function app must have a matching deployment project beside it, named `<FunctionAppProject>.Deployment`, unless a skip reason is explicitly documented (`required`).
- Deployment coverage validates the sibling `.deployproj` and required deployment assets on disk. Legacy deployment projects may be omitted from Visual Studio solutions when the installed IDE does not support their project type; solution membership is not required by this check.
- Deployment coverage also resolves the shared `VAA.EAPI.Deployments` tree and accepts a single legacy deployment-project filename that differs from its folder. [Deployment asset exceptions](deployment-asset-exceptions.json) records user-approved legacy slot/APIM and project-support gaps for PR #2398. Exceptions apply only to their named app and files; core ARM templates and environment parameters cannot be exempted. Accepted gaps do not establish that those deployment paths are usable.
- Update docs and harness files when execution paths, startup registration, verification flow, or deployment coverage rules change (`required`).

## Full Enforcement Baseline

To consider a scope fully enforced, all of the following must be true:

- it is listed in `docs/harness-pipeline-checklist.md` enforcement inventory with owner, pipeline path, and required status check;
- PR CI runs `scripts/verify.ps1 -SolutionPath <path> -SkipBuild` or `-ProjectPath <path> -SkipBuild` and blocks merge on failure;
- main/release flow runs full `scripts/verify.ps1 -SolutionPath <path>` or `-ProjectPath <path>`;
- required status checks are fail-closed (no permanently pending skipped checks);
- local pre-commit policy has equivalent non-bypassable CI parity.

## Checks Performed By verify.ps1

| Check | Intent |
|---|---|
| Solution/project build | Catch compile/analyzer regressions in the selected scope. |
| Test projects | Guard behavior for discovered or explicitly supplied test projects. |
| Format on new `.cs` files | Keep new files conformant without forcing whole-file normalization on legacy files. |
| Deployment project coverage | Ensure Azure Functions apps included in the selected solution have matching deployment projects and required deployment assets. Neighbouring apps outside that solution are excluded; a four-case regression exercise verifies scope and missing-asset detection. |
| Knowledge health | Ensure required docs artifacts exist, docs-local links resolve, and recent generated knowledge entries keep the expected shape. |
| Harness rules | Enforce core harness wiring and prevent placeholder-only behavior sensor catalogs. |
| Agent memory state | Validate the tracked `.codex/state/session-state.example.json` contract and validate local `.codex/state/session-state.json` freshness when present. |
| Context routing | Ensure high-risk context fields and context-routing guidance stay complete and fresh. |
| Docs harness support | Ensure behavior sensors, Complete sequence-goal sensors, templates, DoD status, and runtime/drift evidence stay operational for `/docs`. |
| Operational harness registry | Validate capability enrollment, owners, evidence paths, explicit gaps, review freshness, and maturity-cycle claims. |
| Preferred shared extensions | Block newly added static null-or-whitespace checks and case-insensitive static string comparisons when the shared extensions are available. |
| Unit-test coding standards | Block newly introduced Moq and ad hoc HTTP connector test doubles; require shared DI/HTTP fixtures and checked-in `.mock` resources where applicable. |

## CI And Scheduled Gates Beyond verify.ps1

The following controls are required in CI/scheduled operations, but are not executed by `verify.ps1` itself:

| Gate | Intent |
|---|---|
| Harness remediation log | Ensure scheduled drift outcomes and remediation closures are tracked with owner/due-date evidence. |
| Closed-loop autonomy gate | Ensure required governance controls are computationally enforced in PR/main/scheduled CI. |
| Sequence diagram status | Ensure index/file status consistency and required Agent Loop Goal metadata for diagram-driven goals. |
| Sequence-goal PR review loop | Ensure agent-created sequence-goal PRs respond to AI-only review threads, verify fixes, resolve only AI-only threads with `[from-codex]`, leave threads containing user feedback unresolved, and stay incomplete until approved for `main`. |
| Sequence-goal human comment loop | Ensure agent-created sequence-goal PRs respond to non-AI comments, action agreed feedback, explain disagreement, and leave human review threads unresolved. |
| Sequence-goal PR finalizer | Ensure merged coordinator PRs finalize their ledger row on `main`, while closed-unmerged PRs write `Abandoned` evidence on their source branch when available or a sanitized standalone archive on `main` when already deleted; remove transient review/comment evidence only when the source branch is available for the durable lifecycle update. |
| Review team labels | Add concise PR labels for requested review teams and leave each label in place once added. |

The sequence-goal PR lifecycle is event-driven. Configure `SEQUENCE_GOAL_GITHUB_APP_ID` and `SEQUENCE_GOAL_GITHUB_APP_PRIVATE_KEY` as repository secrets and `SEQUENCE_GOAL_GITHUB_APP_SLUG` as a repository variable. On the EAPI coordinator repository, grant the App Actions read/write, Checks read, Commit statuses read, Contents read/write, Issues read/write, and Pull requests read/write permissions. On each associated delivery repository, grant Actions read, Checks read, Commit statuses read, Contents read/write, Issues read/write, and Pull requests read/write permissions; Actions write is not required there. The App creates and updates delivery PRs, inspects failing checks, dispatches review workers from EAPI, and performs controlled worker pushes/replies so automation-created PR events do not wait for workflow approval. The write-capable App token is confined to controlled workflow steps and is not exposed to the remediation agent process. Workflows that explicitly implement a conditional App-token step retain a `GITHUB_TOKEN` compatibility fallback during rollout; the cross-repository associated-PR monitor and feedback worker require the configured App token and fail closed when it is unavailable. The trusted Copilot completion relay dispatches only for successful runs; unsuccessful completions dispatch nothing. Fifteen-minute scheduled recovery scans all trusted open sequence-goal PRs and dispatches both the AI-review and human-comment workers, covering feedback events GitHub blocks as `action_required` while matching the associated-PR recovery cadence. This schedule is a best-effort recovery watchdog rather than a response-time guarantee. Review-agent command JSON must resolve to a command ending in a non-empty, non-whitespace prompt argument.

Production and QA delivery discovery authenticates workflow-created PRs by trusted author, exact coordinator/surface/phase metadata, expected head repository, and expected base branch. Deterministic branches include a short hash of the exact coordinator ref. Recovery reads registered branches and PRs from the goal document and validates their current metadata; retired branch and metadata formats require explicit migration. A closed unmerged PR remains historical evidence and does not block a fresh attempt. When no reusable open PR exists but its deterministic remote ref remains, the publisher retains that historical ref and creates or reuses a collision-safe `-retry-<run>-<attempt>` branch.

To maintain review labels, edit `teamLabels` in `.github/review-team-labels.json`. Keys are GitHub team slugs and values are the complete PR label names. Keep label names under the configured `managedLabelPrefix`; the workflow loads this mapping at runtime, creates missing labels, and requires no workflow edit when the mapping changes. Once a configured review label has been added to a pull request, the workflow does not remove it.

For `MobileIos` associated-PR remediation, the feedback worker downloads the official SwiftLint `0.65.0` portable release, verifies its SHA-256 checksum and reported version, and adds it to `PATH` before invoking the agent. This keeps strict SwiftLint verification locally actionable without relying on Homebrew version extraction.

Associated-PR check evidence includes up to 50 check annotations, with truncation identified, and a bounded failed GitHub Actions job-log tail when available. The collector derives the Actions job ID from the check run's `details_url`, extracts the downloaded ZIP archive without buffering its full output, and reads bounded file tails. For `MobileIos` PR-description validation failures, the remediation agent must record an array containing exactly the six unique, case-sensitive mandated inferential-review passes and a constrained outcome in evidence; a controlled App-token step renders that evidence into the PR body, keeping GitHub credentials outside the agent process.

## Git Hooks And Knowledge Updates

Hooks live under `.githooks/` and are installed via `core.hooksPath`.

- `pre-commit`: blocks staged `.csproj` files that pin inline `PackageReference Version=` values, runs `scripts/verify-staged.ps1 -SkipBuild`, then updates and stages `docs/knowledge-repo.md`.
- `post-commit`: intentionally no-op to avoid creating new local changes after a commit.

Pre-commit records the central package-management guard and the resolved staged verification command(s) in the knowledge entry metadata via `scripts/update-knowledge-repo.ps1 -FromStaged -ValidationCommand ...`.

See [`knowledge-repo-guide.md`](knowledge-repo-guide.md) for exact command flow and entry fields.

## Codex Hooks

Repository-local Codex hooks are defined in `.codex/hooks.json`.

- `SessionStart`: injects repository working context and surfaces the untracked worktree-local `.codex/state/session-state.json` summary when available.
- `PreToolUse` (`Bash`): blocks destructive shell commands and nudges solution-scoped `dotnet` commands when invoked from repo root.
- `PreToolUse` (`Bash`): for mutating `gh api ... pulls/comments/<id>/replies` calls (`POST` or explicit reply `body` payload), blocks the command unless the reply body includes the marker `[from-codex]`. Read-only/list calls are allowed.
- `PreToolUse` (`apply_patch` / `Edit` / `Write`): blocks adding inline `PackageReference Version=` in `.csproj` files.
- `PermissionRequest` (`Bash`): denies destructive approval requests and denies risky network/package approvals that do not include a rationale.
- `PostToolUse` (`Bash`): on failing `dotnet build`/`dotnet test`, injects self-correction guidance to rerun verification.
- `PostToolUse` (`apply_patch` / `Edit` / `Write`): after edits to `Program.cs`, `host.json`, or `.csproj`, injects reminders to run targeted verification.
- `Stop`: if there are pending repository changes, requests one continuation reminder to run scoped `verify.ps1` and optionally update the untracked local state for same-worktree resumption.

## Optional Sensors

- Inferential review (`advisory`): run on high-risk API, DI, business logic, security, or integration changes and record output.
- Drift/coverage sensor (`advisory` unless promoted by owning scope): run dead code scan, coverage trend, dependency drift checks, stale plan/debt review, docs health checks, optional strict repository-link checks, and behavior-harness hygiene checks in CI or on a schedule.

## Harness Coverage

Use the coverage model when a change introduces a new failure mode, when review comments repeat, or when a solution adopts new startup, messaging, API, deployment, or security conventions.

The expected output is a short mapping from risk to control:

- the guide that tells the agent or contributor what good looks like,
- the sensor that detects a violation,
- the lifecycle stage where the sensor runs,
- any accepted gap and the condition that should reopen it.

## Alignment Notes

When a solution adopts or changes shared startup, middleware, dependency injection, deployment, or verification patterns, update this document only if the reusable harness pattern changes. Solution-specific registrations and inventory belong in the project README or solution-scoped docs.
