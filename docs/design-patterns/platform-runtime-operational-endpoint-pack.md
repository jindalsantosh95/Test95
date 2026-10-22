# Operational Endpoint Pack

**Pattern group:** Platform Runtime

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Give every operationally important EAPI service a predictable set of readiness, warmup, health, and scheduled execution entry points.

This pattern makes services easier to monitor, warm, retry, and support without engineers having to rediscover each service's operational behavior.

## Use When

- A service exposes HTTP endpoints and needs a heartbeat or health endpoint.
- A service has warmup requirements.
- A service performs scheduled work through timer triggers.
- A new dependency needs to be reflected in health checks.

## Do Not Use When

- The project has no runtime endpoint or scheduled workload.
- A timer is being used to replace event-driven or request-driven behavior.
- The endpoint would expose sensitive operational detail.

## Pattern Shape

1. Expose a `heartbeat` endpoint backed by `HealthCheckService`.
2. Add a warmup trigger where cold-start readiness matters.
3. Use timer triggers for scheduled publish or refresh jobs.
4. Apply retries where the timer workload should be retried by the Functions runtime.

## Implementation Guidance

- Keep operational triggers in `Triggers/Http` or `Triggers/Timer`.
- Keep timer trigger methods thin and delegate to an application or infrastructure service.
- Add OpenAPI metadata for HTTP health endpoints.
- Do not return secrets, connection strings, or sensitive dependency details from health responses.

## Code References

- [HealthController.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.Refunds/Triggers/Http/HealthController.cs:20): heartbeat and warmup trigger shape.
- [FlightCrewRosterTimerFunction.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.RetailTherapy/Triggers/Timer/FlightCrewRosterTimerFunction.cs:12): timer trigger with fixed-delay retry.
- [FlightCrewRosterService.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.RetailTherapy/Infrastructure/Services/FlightCrewRosterService.cs:30): scheduled workload delegated to a service.

## Related Principles And Standards

- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Health and warmup endpoints expose only safe operational information.
- Timer triggers delegate work and do not contain business orchestration.
- New dependencies have health-check coverage where appropriate.
- Retry behavior is intentional and documented.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
