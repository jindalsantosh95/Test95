# Subscriber Context Rehydration

**Pattern group:** Workflow And Messaging

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Restore EAPI request context from an incoming CloudEvent before processing a Service Bus message so logs, telemetry, downstream calls, and follow-on events carry the original correlation, operation, session, and channel values.

This pattern makes asynchronous processing traceable as part of the originating journey rather than as disconnected background work.

## Use When

- Handling CloudEvent-backed Service Bus messages.
- Publishing follow-on events from a subscriber.
- Calling downstream services from an event handler.

## Do Not Use When

- The incoming message is not an EAPI CloudEvent.
- The message comes from an external producer with an incompatible envelope.
- A new correlation ID is intentionally required for a new independent workflow.

## Pattern Shape

1. Service Bus trigger receives event payload.
2. Trigger deserializes payload into a CloudEvent.
3. Trigger maps CloudEvent context into `AsyncContext`.
4. Trigger delegates to an application event handler.
5. Handler logs, calls downstream systems, or publishes follow-on events using restored context.

## Implementation Guidance

- Keep subscribers thin and focused on envelope handling.
- Validate the event before processing business data.
- Preserve cancellation tokens.
- Do not overwrite correlation values unless starting a new workflow intentionally.

## Code References

- [CheckinOpenSubscriber.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Triggers/ServiceBus/CheckinOpenSubscriber.cs:15): restores context from CloudEvent metadata.
- [AsyncContext.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Common/AsyncContext.cs:125): maps context from CloudEvents.
- [FlightDataRequestEventCommandHandler.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Application/Events/FlightDataRequestEventCommandHandler.cs:29): event handler invoked after context restoration.

## Related Principles And Standards

- [Integration Principles](../principles/integration-principles.md)
- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Subscriber restores context before invoking handler logic.
- Handler receives a `CancellationToken`.
- Logs and follow-on events preserve the original correlation.
- Non-EAPI envelopes are handled explicitly.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
