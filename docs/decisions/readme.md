# Decision Records

Decision records capture architecture, integration, security, delivery, and harness choices that future engineers or agents need to understand.

Use a decision record when:

- a rule or trade-off is likely to be reused,
- a design choice rejects a plausible alternative,
- the decision came from chat, meetings, PR review, or incident follow-up and would otherwise be invisible in the repository,
- a repeated review comment should become durable guidance.

## Naming

Use:

```text
YYYY-MM-DD-short-title.md
```

Start from `template.md`.

## Expectations

- Link decisions from the docs or plans that depend on them.
- Keep records short and explicit.
- If a decision becomes a mechanical rule, add the corresponding harness check, analyzer, script, or test.
