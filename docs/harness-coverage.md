# Harness Coverage

Harness coverage is the review practice that asks whether important failure modes have enough guides and sensors around them. It is not the same as test coverage. Test coverage measures executed code paths; harness coverage measures whether the coding-agent workflow has controls that prevent, detect, or expose the risks that matter for a change.

## Coverage Model

Assess coverage across these dimensions:

| Dimension | Question |
|---|---|
| Regulation category | Is the risk about maintainability, architecture fitness, behavior, safety, operability, or delivery? |
| Failure mode | What could the agent or contributor get wrong? |
| Feedforward guide | Where does the agent learn the desired shape before changing code? |
| Feedback sensor | What detects the problem after code or docs change? |
| Mode | Is the control computational, inferential, or manual? |
| Lifecycle placement | Does the control run during authoring, pre-commit, PR, main, scheduled drift, or runtime monitoring? |
| Policy | Is the control blocking, advisory, or an explicit accepted gap? |
| Evidence and freshness | Where is the result recorded, who owns it, and when does it expire or require review? |

## Coverage Matrix Template

Use this matrix in project docs, PR notes, or harness updates when a change introduces or exposes a meaningful risk.

| Scope | Failure mode | Guide | Sensor | Mode | Lifecycle | Policy | Gap or next control |
|---|---|---|---|---|---|---|---|
| `<solution/project/area>` | `<what could go wrong>` | `<doc, principle, skill, template, or script>` | `<test, linter, script, review, monitor, or none>` | `<computational/inferential/manual>` | `<authoring/pre-commit/PR/main/scheduled/runtime>` | `<blocking/advisory/accepted gap>` | `<planned guide or sensor>` |

### Sequence-goal QA production context

| Scope | Failure mode | Guide | Sensor | Mode | Lifecycle | Policy | Gap or next control |
|---|---|---|---|---|---|---|---|
| Sequence-goal QA generation | An agent receives production implementation evidence from an untrusted repository, branch, PR identity, or revision | `scripts/run-sequence-goal-agent.ps1` authenticated-snapshot guardrails | `.github/workflows/ai-delivery-sequence-goal-loop.yml` validates the configured repository/base, trusted metadata, PR identity, and stable head SHA; `scripts/check-harness-rules.ps1` protects those controls | Computational | PR | Blocking | Keep snapshot size bounded and revision-bound |
| Sequence-goal QA generation | GitHub credentials remain available while agent-authored code or build targets execute | QA credential-isolation and trusted-publication contract | The workflow removes and verifies agent credentials, creates a hash-verified data-only handoff, and publishes from a fresh post-agent checkout; restore/build remain credential-free and build-tainted checkouts cannot publish failure commits; `scripts/check-harness-rules.ps1` verifies the boundary | Computational | PR | Blocking | Keep all credentialed QA publication confined to the fresh checkout |
| Sequence-goal QA generation | Temporary PR evidence escapes its directory or survives normal failure | Production-context path and cleanup contract | Runner rejects linked ancestors and consumes the snapshot before agent execution; workflow safely removes it on pre-invocation failure | Computational | PR | Blocking | Accepted gap: abrupt runner cancellation before consumption relies on ephemeral hosted-runner teardown; replace with an isolated worker boundary before claiming cancellation-proof cleanup |
| UI automation authoring | Added or modified test/feature/flow code creates a browser session directly, bypasses a journey flow, or introduces driver APIs | `VAA.EAPI.Automation/AGENTS.md` and `docs/FRAMEWORK_ARCHITECTURE.md` | `scripts/check-ui-automation-standards.ps1` reuses the architecture-fitness driver-token policy and scans committed, staged, and untracked additions; UI Tests/Features/Flows/Pages receive the `high-risk` label and inferential review | Computational + inferential | Pre-commit + PR | Blocking | Added files are scanned completely; modified legacy files are checked on added lines so existing debt does not create unrelated failures |

## Minimum Expectations

- Every high-risk behavior change should have at least one feedforward guide and one feedback sensor, or an explicit accepted gap.
- Every implemented workflow listed in [`behavior-harness.md`](behavior-harness.md) should name the test, fixture, script, or monitor that protects its contract.
- Every repeated failure mode or review comment repeated more than twice should lead to a coverage entry and either a new guide, a new sensor, a technical-debt entry, or an explicit decision not to automate it yet.
- Every new solution or project should document the smallest relevant verify command and the expected test project scope.
- Every architecture rule that reviewers repeatedly enforce should be moved into a principle, coding standard, design pattern, analyzer, script, or test.
- Every declared agent-readable runtime source should satisfy the evidence and security contract in [`agent-observability.md`](agent-observability.md).
- Every autonomous mutating workflow should satisfy the run, recovery, and resource contract in [`agent-execution-and-recovery.md`](agent-execution-and-recovery.md).
- Documentation drift coverage should distinguish deterministic structure/link checks from semantic code-versus-document review as defined in [`doc-gardening.md`](doc-gardening.md).

## Review Flow

1. Identify the changed solution, project, workflow, or shared area.
2. List the failure modes that would be costly, likely, or hard to spot in review.
3. Map the current guides and sensors for each failure mode.
4. Classify each sensor by mode and lifecycle placement.
5. Move cheap, deterministic checks as far left as practical.
6. Record remaining gaps with an owner, trigger, and next control in the PR notes, an execution plan, or `tech-debt-tracker.md`.

## Coverage Snapshot Template

Use this summary when a solution or project needs a compact harness coverage statement.

| Scope | Maintainability | Architecture fitness | Behavior | Operability/delivery | Open gaps |
|---|---|---|---|---|---|
| `<solution/project>` | `<guides/sensors>` | `<guides/sensors>` | `<guides/sensors>` | `<guides/sensors>` | `<accepted or planned gaps>` |

## Gap Policy

An accepted gap is allowed when the cost of automation is higher than the current risk, but it should be visible. Revisit accepted gaps when:

- the same issue appears more than twice,
- the affected code becomes shared infrastructure,
- the workflow becomes customer-facing or revenue-impacting,
- the change touches security, privacy, availability, or compliance,
- a cheap deterministic sensor becomes available.

## Maintenance Rule

When adding or changing a harness guide, hook, script, test category, review step, or pipeline gate, update the coverage matrix or snapshot if the change alters what risks the harness can regulate.
