# Thin Trigger To Handler

**Pattern group:** API Boundary

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Keep Azure Function triggers focused on transport concerns while application handlers own validation sequencing, business orchestration, downstream calls, and response payload creation.

This pattern makes endpoint behavior testable without invoking the Functions runtime and keeps triggers small enough to review safely.

## Use When

- Adding an HTTP, Service Bus, or timer trigger.
- Moving business logic out of a function method.
- Creating a new microservice or facade endpoint.

## Do Not Use When

- The trigger only returns a static operational response such as warmup.
- The handler would simply wrap one trivial line with no reusable behavior.
- The implementation would split a single cohesive rule across too many classes.

## Pattern Shape

1. Trigger reads input and sets context.
2. Trigger performs basic boundary validation such as malformed body handling.
3. Trigger calls an application handler interface.
4. Handler performs orchestration and returns a domain or response model.
5. Trigger translates that result to the transport response.

## Implementation Guidance

- Put handlers in `Application/`.
- Define handler interfaces near the application layer or in the established interfaces folder.
- Pass `CancellationToken` into handlers.
- Keep downstream integration behind services or gateways.
- Unit test handlers directly.

## Code References

- [RefundsController.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.Refunds/Triggers/Http/RefundsController.cs:53): trigger delegates refund status handling.
- [SubmitRefundCommandHandler.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Microservices.CRM.Refunds/Application/SubmitRefundCommandHandler.cs:21): handler owns submission orchestration.
- [CheckinOpenSubscriber.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Triggers/ServiceBus/CheckinOpenSubscriber.cs:26): Service Bus trigger delegates event handling.

## Related Principles And Standards

- [Architecture Principles](../principles/architecture-principles.md)
- [Integration Principles](../principles/integration-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Trigger code is transport-focused.
- Handler owns orchestration and business decisions.
- Dependencies are injected through interfaces.
- Handler tests cover success and failure paths.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
