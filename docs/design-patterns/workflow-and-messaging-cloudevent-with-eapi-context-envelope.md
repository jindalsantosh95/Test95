# CloudEvent With EAPI Context Envelope

**Pattern group:** Workflow And Messaging

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Wrap asynchronous EAPI messages in CloudEvents enriched with diagnostics, session, operation, correlation, and channel context from `AsyncContext`.

This pattern preserves traceability across request-driven and event-driven flows, and provides a consistent message envelope for Service Bus publishers and subscribers.

## Use When

- Publishing an event to Service Bus.
- Translating an application workflow into an asynchronous notification or command.
- Propagating correlation and diagnostics through asynchronous processing.

## Do Not Use When

- The message contract is controlled by an external system and cannot carry EAPI metadata.
- The event has no EAPI ownership or processing responsibility.
- Sensitive data would be put into the envelope without minimisation.

## Pattern Shape

1. Create a CloudEvent with ID, subject, type, source, time, spec version, and data.
2. Use `AsyncContext.Current.MapToCloudEvent(...)` to add EAPI context.
3. Publish through the EAPI Service Bus abstraction.
4. Set Service Bus message ID, subject, and correlation ID from the CloudEvent.

## Implementation Guidance

- Use consistent event type and subject constants.
- Minimise event payload data and avoid secrets or unnecessary PII.
- Use versioned event types when consumers depend on the contract.
- Keep event mapping explicit and close to the workflow.

## Code References

- [AsyncContext.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/AsyncContext.cs:115): maps EAPI context onto CloudEvents.
- [NotifCloudEventMapper.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Mapper/NotifCloudEventMapper.cs:16): creates notification CloudEvents.
- [AzureServiceBus.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/MessageBroker/ServiceBusMessagingV2/AzureServiceBus.cs:17): publishes CloudEvents to Service Bus.

## Related Principles And Standards

- [Integration Principles](../principles/integration-principles.md)
- [Security Principles](../principles/security-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Event includes correlation and operation context.
- Payload is minimised and safe.
- Event type and subject are stable constants.
- Publisher uses EAPI messaging abstractions.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
