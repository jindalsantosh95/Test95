# Startup And Dependency Injection

This document defines how startup and dependency injection references should be captured for any solution or project in the repository.

Use it when changing `Program.cs`, host configuration, middleware, telemetry, health checks, outbound HTTP registrations, cache registrations, message clients, storage clients, or application service lifetimes.

## Required Sections Per Project

For each changed solution or project, document:

- startup pipeline order,
- configuration providers,
- middleware registration,
- platform/shared service registrations,
- application handler/service registrations,
- external integration registrations,
- lifetime choices and thread-safety implications,
- supporting startup utilities or extension methods.

## Template

### `<ProjectName>` Startup Pipeline

Implementation source:

- `<ProjectName>/Program.cs`

Startup order:

1. `<configuration provider or host setup>`
2. `<worker/web host middleware setup>`
3. `<OpenAPI, GraphQL, controller, or route setup where applicable>`
4. `<service registration>`
5. `<Build and Run>`

### Platform And Shared Registrations

- `<registration, for example AddDefaultServices()>`
- `<registration, for example AddHealthChecks()>`
- `<registration, for example AddSingleton<IHttpConnector, HttpConnector>()>`

### Application Registrations

| Service | Implementation | Lifetime | Notes |
|---|---|---|---|
| `<interface>` | `<implementation>` | `<Singleton/Scoped/Transient>` | `<reason or caveat>` |

### Integration Registrations

| Dependency | Registration | Configuration |
|---|---|---|
| `<downstream, cache, queue, storage, or SDK>` | `<DI registration>` | `<key or options source>` |

## Lifetime Guidance

- Use singleton only for stateless, thread-safe services.
- Use scoped or transient where lifecycle should be bounded per request, operation, function invocation, or handler execution.
- Do not store per-request mutable state in singleton fields.
- Favor method-local variables and immutable DTOs.
- Keep outbound HTTP behind the repository's connector or approved infrastructure client pattern.

## Maintenance Rule

When startup registration, middleware, configuration loading, telemetry, health checks, cache, messaging, storage, or outbound HTTP behavior changes, update this reference or the owning project README in the same change.
