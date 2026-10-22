# Goal delivery flow comparison

The original process reconciled multiple recovery records. The current process uses
the goal document on the delivery branch, with ordinary commits providing history.

## Original flow

```mermaid
flowchart TD
    O1[Goal document] --> O2[Origin JSON and commit/blob hashes]
    O2 --> O3[Lock files and attempt counters]
    O3 --> O4[Run summary JSON and PR summary pointer]
    O4 --> O5[Placeholder commit and draft child PR]
    O5 --> O6[Production work and encoded recovery checkpoint]
    O6 --> O7[PR readiness marker and ledger workflow]
    O7 --> O8[QA work and continuation summary]
    O8 --> O9[Separate cost retention workflow]
    O9 --> O10[Merge and integrated verification]
    O10 --> O11[Cleanup locks and write origin tombstone]
    OR[Restart] --> O2
    OR --> O3
    OR --> O4
    OR --> O7
```

## Simplified flow

```mermaid
flowchart TD
    R[Push goal branch with Ready document] --> C[Create draft coordinator PR to main]
    C --> P[Register production branches for each surface]
    P --> V[Generate and verify production; publish child PR]
    V --> Q[Start matching QA immediately on its own branch]
    Q --> A[Generate automation; publish draft QA PR]
    A --> T[Verify pinned QA commit; record completion; mark QA ready]
    V --> M[Review and merge production PR]
    T --> N[Review and merge QA PR]
    M --> I[All required children merged; integrated verification]
    N --> I
    I --> F[Commit Complete goal; mark coordinator ready]
    G[Goal document and commit history] --- P
    G --- T
    G --- I
    W[Monitor resumes incomplete or interrupted work] --> P
    W -->|Stop for Blocked work or after three continuations| B[Resolve blocker and explicitly restart]
```

QA starts after verified production publication, without waiting for production
merge. Each surface has its own production and QA branches. The monitor waits for
active runs and verified PR review, and resumes missing work from the document.

| Concern | Original | Simplified |
| --- | --- | --- |
| Recovery | Reconcile origin, locks, summaries, PR markers, checkpoints | Read the goal document and validate its branches/PRs |
| Registration | Placeholder commit to create a draft child PR | Record the branch; open the PR when substantive work exists |
| Concurrency | Persistent locks plus workflow coordination | GitHub job concurrency and retryable document commits |
| Verification | PR readiness markers | Verified production revision recorded in the document |
| Costs | Separate dispatched workflow and JSON ledger | Idempotent run/attempt entries in the document |
| Completion | Origin tombstones and lock cleanup | Goal closure evidence; closed PRs excluded from monitoring |

Repository/PR ownership checks and QA's binding to the production revision remain.
Diagnostic artifacts and review evidence are not alternate delivery recovery records.
Incomplete or interrupted runs resume within the document continuation limit. Blocked or exhausted recovery requires intervention. Hosted end-to-end delivery still needs validation after merge.
