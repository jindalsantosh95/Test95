# Agent Tool Integration And Executable Environments

This contract makes the repository's agent tooling and execution assumptions explicit and testable. It covers the `/docs` harness itself; solution-specific applications must add their own startup, readiness, telemetry, shutdown, and dependency contracts before claiming an isolated executable application environment.

## Environment Profiles

[`agent-tool-environments.json`](agent-tool-environments.json) is the machine-readable source of truth. `docs-local` is the required profile for local and scheduled `/docs` verification. `codex-ci` extends it for enrolled GitHub agent workflows after those workflows install and authenticate Codex.

Every profile declares:

- required commands;
- whether execution must be inside a Git worktree;
- allowed writable roots for generated evidence;
- network and secret policies;
- evidence location and cleanup behavior.

Missing tools, unsupported minimum versions, stale configuration, unsafe paths, execution outside a worktree, or a failed isolated write/read/delete probe fail closed. Codex CI workflows install the registry-pinned CLI version rather than an unbounded latest package. Capability discovery never prints environment-variable values or credentials.

## Operating Commands

```pwsh
scripts/check-agent-tool-environment.ps1 -Profile docs-local -Probe
scripts/invoke-agent-tool-environment-exercise.ps1 -EvidenceDirectory TestResults/agent-tool-environment/manual
scripts/check-agent-tool-environment-strength.ps1 -EvidencePath TestResults/agent-tool-environment/manual/summary.json
```

The exercise proves the real local profile, rejects a missing executable, rejects an unsafe writable root, and proves that evidence is written only beneath an ignored run-scoped directory.

## Workflow Integration

- Root verification runs the `docs-local` probe and the failure-path exercise.
- Scheduled parity repeats both and retains the sanitized summary.
- Enrolled Codex workflows install their declared CLI, pin the model, capture JSONL usage, and run inside bounded GitHub jobs.
- Hooks, skills, MCP servers, browser controls, and connectors remain separate capabilities. A registry entry documents availability; it does not grant credentials, widen sandbox access, or authorize external mutation.

## Ownership And Completion

- **Owner:** Platform Engineering for shared profiles; workflow owners for additional tools.
- **Review cadence:** every 90 days and whenever a required command, runner image, network policy, or evidence root changes.
- **Operational complete when:** the registry is current, `docs-local` passes a real command/worktree/isolation probe, failure mutations are rejected, and local plus scheduled verification execute the checks.
- **Application enrollment:** an owning solution is complete only after its local runtime also declares startup, isolated dependencies, readiness, observability queries, shutdown, and cleanup.
