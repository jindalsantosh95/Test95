# Execution Plans

Execution plans are versioned working notes for changes that are too large, risky, or cross-cutting to live only in a prompt or PR description.

Use an execution plan when a change:

- spans multiple solutions, projects, deployment assets, or docs areas,
- changes service boundaries, startup, messaging, security, data ownership, or infrastructure,
- needs several agent runs or human review checkpoints,
- carries decisions that future agents should be able to recover from the repository.

## Structure

- `active/`: plans currently being executed or intentionally paused.
- `completed/`: plans that have landed, been superseded, or been explicitly abandoned.
- `template.md`: copy this shape for new plans.

## Expectations

- Keep plans scoped to the smallest useful unit of work.
- Record decisions and validation as the work evolves.
- Move completed plans to `completed/` and leave a short outcome.
- Promote durable rules discovered during execution into principles, standards, design patterns, harness checks, or decision records.
