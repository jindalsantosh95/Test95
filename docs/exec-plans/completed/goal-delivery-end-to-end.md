# End-to-end goal delivery

- Status: implemented
- Created: 2026-10-06
- Scope: goal coordinator, associated-PR monitor, QA publication and regression harness

## Objective

Carry a Ready goal through draft coordinator creation, independent production and
QA branches, child review/merge, and integrated verification before coordinator
readiness. The goal document and Git history remain the coordination record.

## Decisions

- Retain immediate per-surface QA dispatch after verified production publication;
  production merge is not a prerequisite.
- Use the existing monitor to recover missing publication and incomplete delivery.
  Record up to three automatic continuations per delivery/completion stage in the Coordinator document entry;
  explicit restart resets the allowance. Blocked task outcomes require intervention.
- Retain draft QA publication until verification and document completion succeed.
- Never treat a green workflow as goal completion. All required child PRs must be
  merged and current verification must pass before coordinator readiness.

## Validation

- Monitor exercises execute actual workflow JavaScript for missing children,
  incomplete production/QA, missing QA before production merge, review waiting,
  merged children, blocked/exhausted recovery, active runs, API failure, and moved
  coordinator heads.
- Readiness exercise: 52 cases, including verified-draft QA publication and stale
  merged-child routing; 56 workflow PowerShell blocks parsed.
- Document exercises cover automatic count retention, explicit restart reset,
  exhaustion, and a fresh allowance for integrated completion.
- Workflow YAML and 19 JavaScript blocks parse successfully.
- Full root harness passed: `scripts/verify.ps1 -ScopePath . -Base origin/main -SkipBuild -SkipDeploymentCheck`.

## Outcome

The document remains the coordination record. The existing monitor now continues
incomplete delivery and recovers missing QA without waiting for production merge;
QA becomes ready after verification, and the coordinator remains draft until all
current child PRs merge and integrated verification passes. No live goal was
modified or restarted. Rebase goal branches after rollout before resuming hosted
runs; hosted end-to-end validation remains required.
