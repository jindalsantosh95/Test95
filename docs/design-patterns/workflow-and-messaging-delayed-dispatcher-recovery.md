# Delayed Dispatcher Recovery

**Pattern group:** Workflow And Messaging

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Schedule delayed items in a recoverable dispatcher that can reload pending work, convert each item into a CloudEvent, write it to the event store, publish it, and reschedule on failure.

This pattern supports delayed event workflows without losing scheduled work during process restarts or transient publish failures.

## Use When

- A workflow must publish an event after a calculated delay.
- Scheduled work should survive process restarts or cache recovery.
- Failed dispatches should be retried by rescheduling.

## Do Not Use When

- A durable queue, timer trigger, or Service Bus scheduled message is the approved simpler option.
- The delay is purely UI-related or client-side.
- The scheduled payload contains data that should not be persisted.

## Pattern Shape

1. Add a delayed item to the dispatcher.
2. Persist or cache pending scheduled items.
3. On invocation, wrap the item in a CloudEvent and map EAPI context.
4. Insert the event into the event store.
5. Publish to the message bus.
6. Remove successful items or reschedule failed ones.

## Implementation Guidance

- Keep delayed dispatch services behind interfaces.
- Use clear event subjects and types.
- Make scheduled payloads minimal and recoverable.
- Log failures and reschedule without losing the item.

## Code References

- [DelayedDispatcherService.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Services/Dispatcher/DelayedDispatcherService.cs:245): delayed item invocation, event-store insert, publish, and reschedule.
- [CheckInOpeningService.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Infrastructure/Services/CheckInOpeningService.cs:16): service using delayed dispatch behavior.
- [CheckInOpeningServiceTests.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications.Tests/CheckInOpeningServiceTests.cs:1): tests around delayed dispatch workflow.

## Related Principles And Standards

- [Integration Principles](../principles/integration-principles.md)
- [Infrastructure Principles](../principles/infrastructure-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Delayed work has a clear owner and event type.
- Payload is safe to persist or cache.
- Failures reschedule rather than silently dropping work.
- Event-store and Service Bus behavior is tested.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
