# Continue production within its Codex session

- Date: 2026-10-06
- Scope: sequence-goal production runner and verification harness

## Problem and decision

Darwinium production returned Incomplete after a normal nine-minute turn despite
an instruction to finish all actionable work and a passing resource check. The
runner accepted that checkpoint and relied on the scheduled monitor to resume.

Resume the exact session immediately when production remains Incomplete and
another turn fits the existing resource allowance. Keep the worktree and append
usage to one cumulative record. Require an explicit stopping reason; external
blockers must exhaust independent work first. Keep existing document publication,
verified completion gates, and monitor recovery rather than adding coordination
state or another workflow.

## Verification and limits

The session exercise executes the actual runner loop with synthetic CLI events
and the real cumulative budget validator. It covers same-session continuation,
completion, blocked work, budget headroom, elapsed time, and missing session or
stopping reason. No paid agent or live goal is used for these tests.

Time and next-turn resource checks occur between turns. They cannot guarantee the
next turn's duration or usage; existing job timeouts and final budget validation
remain in force. Hosted execution must be confirmed after rollout.
