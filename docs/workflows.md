# Workflows

This document defines how workflow references should be written for any solution or project in the repository.

Use it for cross-service journeys, API workflows, event-driven flows, scheduled jobs, batch processing, GraphQL composition, deployment flows, and operational recovery paths.

## Required Sections Per Workflow

For each workflow, document:

- owning solution and project,
- trigger or entry point,
- application handler or service that owns orchestration,
- downstream systems, queues, topics, caches, and data stores,
- important configuration keys,
- success path,
- failure, retry, timeout, and no-op paths,
- behavior tests, fixtures, scripts, or monitoring that prove the contract.

## Template

### `<Workflow Name>`

Owner:

- Solution: `<solution>.sln`
- Project: `<ProjectName>`
- Entry point: `<trigger, endpoint, resolver, subscriber, or job>`
- Handler/service: `<application handler or service>`

#### Overview

Describe the business capability and the boundary this workflow owns.

#### Steps

1. `<trigger or request arrives>`
2. `<validation, lookup, mapping, or enrichment>`
3. `<downstream call, publish, cache, storage, or domain decision>`
4. `<response, event, state change, or no-op outcome>`

#### Key Components

- `<component>`: `<purpose>`
- `<component>`: `<purpose>`

#### Error Handling

- `<retry, timeout, dead-letter, compensation, fallback, or rollback behavior>`
- `<observability and alerting behavior>`

#### Verification

- `<test, fixture, script, monitor, or behavior harness entry>`

## Deployment Workflow Template

Use this for solution-specific deployment flow documentation.

1. Restore dependencies.
2. Build the smallest impacted solution or project.
3. Run impacted tests and harness checks.
4. Package deployment artifacts where applicable.
5. Deploy through the owning pipeline.
6. Validate health, telemetry, and rollback readiness.

## References

- [Behavior Harness](behavior-harness.md)
- [Harness Pipeline Checklist](harness-pipeline-checklist.md)
- [Agent Loop Workflow](agent-loop-workflow.md)
- [Startup and Dependency Injection](startup-and-di.md)
- [Triggers and Endpoints](triggers-and-endpoints.md)
