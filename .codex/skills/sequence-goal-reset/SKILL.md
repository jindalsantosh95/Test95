---
name: sequence-goal-reset
description: Reset one or more EAPI sequence-goal branches to clean Draft state by abandoning their related delivery PRs and branches, removing generated implementation and evidence, retaining reusable requirements, and rebuilding each branch from its latest base. Use only when the user explicitly asks to abandon or reset sequence-goal delivery work; do not use for ordinary status changes, retries, or PR cleanup.
---

# Sequence Goal Reset

Reset every supplied goal independently and leave it ready for a new delivery run. This workflow rewrites remote branches and closes pull requests, so inventory is read-only but mutation requires explicit user authorization.

## Safety boundary

- Treat the named goal branches as the complete scope. Do not infer authority to reset similarly named goals.
- Before mutation, require explicit authorization to abandon delivery work, close related PRs, delete related child branches, and rewrite each named goal branch.
- Identify relationships from the goal document's Delivery progress entries and associated-PR table, coordinator PR metadata, PR base/head relationships, and goal-branch commit history. A name resemblance alone is not sufficient.
- Report uncertain PRs or branches without changing them.
- Preserve the named goal branch; delete only confirmed child production, QA, remediation, or temporary delivery branches.
- Use exact remote SHAs and `--force-with-lease` when replacing a goal branch. Never use an unguarded force push.
- Work in isolated temporary worktrees created from the latest base. Do not overwrite unrelated local changes.

## Inventory

Run the bundled read-only helper before changing anything:

```powershell
pwsh -NoProfile -File .codex/skills/sequence-goal-reset/scripts/Get-SequenceGoalResetInventory.ps1 -GoalBranch "docs/goal-one","docs/goal-two"
```

Review its per-branch inventory errors, goal documents, deleted goal-document candidates, branch divergence, base-resolution status, coordinator PRs, resolved associated PRs, metadata-correlated PRs, changed delivery files, and evidence paths. The helper searches PR bodies across the coordinator's GitHub owner for the exact `repository@goal-branch` marker written by the delivery workflow, so a PR created before its ledger row was committed is reported as unledgered. One inaccessible branch does not prevent the remaining supplied branches from being inventoried. Stop before mutation when any branch has an inventory error, an associated PR is missing from the ledger, base resolution is ambiguous, or an associated PR cannot be resolved. Supplement the inventory with PR comments and reviews when they may contain reusable requirements. Show the user the confirmed destructive scope if their authorization did not already name it precisely.

## Reset each goal

1. Fetch the exact goal branch and its owning base. Confirm the goal base from consistent coordinator-PR metadata or explicit user/repository configuration. Associated delivery rows describe child repositories and bases; do not use them to infer the coordinator goal branch's base. Treat a default-base fallback as unconfirmed until checked.
2. Read review comments before closing PRs. Move durable product, UX, contract, testing, accessibility, failure-handling, and operational requirements into the goal document. Do not retain run results, review transcripts, costs, commit IDs, PR state, or implementation claims as requirements.
3. Close every confirmed open coordinator and child delivery PR as abandoned. Delete confirmed child delivery branches after resolving their exact repository and ref. Leave unrelated and uncertain refs untouched.
4. Rebuild the named goal branch from the latest base rather than reverting delivery commits. Restore only:
   - the goal document;
   - its sequence-diagram index row;
   - authoritative supplied UX/source assets;
   - other requirement documents that existed before delivery or are necessary for the Draft definition.
5. Make the goal internally consistent as a fresh Draft:
   - set `Status` and its index entry to `Draft`;
   - use the current sequence-diagram template and correct durable owners/delivery surfaces;
   - make harness and acceptance evidence explicitly planned;
   - reset maximum, production, automation, actual, and lifecycle resource rows to their template placeholders;
   - remove completion-only status, task, transition, verification, and completed-by fields;
   - remove the Delivery progress entries (Coordinator, Production, QA, and Resources) and empty the associated delivery PR table;
   - restore the generic no-closure-evidence statement;
   - remove goal-specific run, review, comment, remediation, resource, lock, and tracking files;
   - remove generated production and automation changes from the abandoned delivery.
6. Remove behavior-harness enrollment when it was introduced only by the abandoned implementation. Retain independently existing sensors and describe them as read-only contract evidence when appropriate.
7. Validate before publishing:

```powershell
git diff --check
./scripts/check-sequence-diagram-status.ps1
./scripts/check-behavior-harness.ps1
```

Run the Mermaid checker when Node/npx is available. State explicitly when rendering cannot run.

8. Re-fetch the remote goal ref, then replace it with `--force-with-lease=refs/heads/goal-branch:observed-full-sha`. Substitute the exact branch and SHA; if the lease fails, stop, re-inventory, and do not overwrite the newer state.
9. Re-fetch the base after the push. If it advanced during the reset, rebase the Draft-only commit onto the new base, rerun validation, and repeat the guarded push.

## Completion audit

For every supplied goal, prove and report:

- the coordinator and confirmed child PR states;
- deleted child branches and the preserved goal branch;
- zero commits behind the latest base;
- the exact remaining diff from the base;
- zero goal-specific `.codex/agent-loop` or generated delivery-evidence files;
- no abandoned production or automation changes;
- `Draft` status, empty PR tracking, generic closure text, and placeholder resource counters;
- validation results and any unavailable checks.

Do not call the reset complete when any confirmed delivery PR remains open, a confirmed child branch remains, the goal is behind its base, or abandoned delivery/evidence is still reachable from the reset branch over its base.
