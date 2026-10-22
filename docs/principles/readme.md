# EAPI Principles
**Virgin Atlantic EAPI Platform**

---

These documents define the platform principles AI agents and engineers should apply when changing the EAPI codebase. They are written from the current repository shape: a multi-solution .NET monorepo with Azure Functions microservices, GraphQL gateway domains, facades, orchestration services, shared infrastructure libraries, deployment templates, and Azure DevOps pipeline definitions.

Use these principles before designing or implementing a change. They are intentionally normative: existing legacy code may not always match them, but new work should move the platform toward them without broad unrelated refactors.

## Principle Documents

- [Security Principles](security-principles.md)
- [Architecture Principles](architecture-principles.md)
- [Data Principles](data-principles.md)
- [Integration Principles](integration-principles.md)
- [Infrastructure Principles](infrastructure-principles.md)

## How AI Agents Should Use This Folder

- Read the relevant principle document before changing a service, GraphQL domain, facade, deployment, or pipeline.
- Keep changes inside the smallest solution and project that owns the behaviour.
- Prefer established EAPI shared libraries and conventions over new local infrastructure.
- Preserve existing behaviour unless the requested change explicitly requires migration.
- When adding new code, follow the principle even if older nearby code is inconsistent.
- In PR notes, call out any deliberate exception and why it is safe.

