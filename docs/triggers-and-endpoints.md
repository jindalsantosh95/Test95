# Triggers And Endpoints Reference

This document defines how trigger, endpoint, resolver, and subscriber inventories should be captured for any solution or project in the repository.

Use it for Azure Functions triggers, HTTP APIs, GraphQL queries/mutations, Service Bus subscribers, timer jobs, warmup handlers, background jobs, and operational endpoints.

## Required Sections Per Project

For each changed solution or project, document:

- project name and owning solution,
- public HTTP routes and authorization model,
- GraphQL operations where applicable,
- Service Bus, queue, event, or topic subscriptions,
- timer schedules and retry policies,
- warmup, heartbeat, health, and readiness endpoints,
- source files for each trigger or endpoint.

## HTTP Endpoints

| Project | Function/Controller | Method | Route | Auth | Notes |
|---|---|---|---|---|---|
| `<ProjectName>` | `<endpoint name>` | `<GET/POST/etc.>` | `<route>` | `<auth model>` | `<OpenAPI, response, or behavior notes>` |

## GraphQL Operations

| Project | Operation | Type | Resolver/Handler | Notes |
|---|---|---|---|---|
| `<ProjectName>` | `<query or mutation>` | `<Query/Mutation>` | `<source>` | `<contract notes>` |

## Message Subscribers

| Project | Function/Handler | Topic/Queue | Subscription | Connection Key | Notes |
|---|---|---|---|---|---|
| `<ProjectName>` | `<subscriber>` | `<topic or queue>` | `<subscription>` | `<configuration key>` | `<payload or retry notes>` |

## Timers And Jobs

| Project | Function/Job | Schedule Key | Retry | Notes |
|---|---|---|---|---|
| `<ProjectName>` | `<timer or job>` | `<configuration key>` | `<retry policy>` | `<behavior notes>` |

## Operational Endpoints

| Project | Endpoint/Trigger | Purpose | Notes |
|---|---|---|---|
| `<ProjectName>` | `<heartbeat, health, readiness, warmup>` | `<purpose>` | `<source or response notes>` |

## Maintenance Notes

- Keep this inventory aligned with implementation and OpenAPI/GraphQL schema changes.
- Keep triggers thin; orchestration and business decisions should live in application handlers or services.
- Update configuration and behavior harness documentation when routes, schedules, topics, subscriptions, authorization, or payload contracts change.
