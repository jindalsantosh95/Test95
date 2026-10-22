# VAA.EAPI.Microservices.CRM.Email

## Overview
Azure Functions microservice that processes CRM and platform email events from Service Bus topics and sends downstream communications.

## Triggers
- HTTP: health and email endpoints.
- Service Bus subscribers including:
  - Enrolment and account related emails.
  - Order email events.
  - **Store appointment CRM email events** via `%StoreAppointmentCrmTopicName%` and `%StoreAppointmentCrmEmailSubscriptionName%`.

## Store appointment email flow
The `StoreAppointmentEmailSubscriber` consumes CRM cloud events whose `Data` payload contains:
- `TemplateId` / `EmailAddress` / `EmailDataXml`

The subscriber processes one recipient per event and forwards it to ACM in the same SOAP `rtEvent` pattern used by welcome emails, with the template mapped via `new XAttribute("type", request.TemplateId)`.
## Young Flyer enrolment confirmation

Loyalty Member Management publishes the managed-child confirmation through the existing email event path after the managed-child household change events are accepted for publication. It selects configured template `YoungFlyerEnrolmentConfirmationEmailTemplate`, routes the message only to the trusted parent's address, and includes `Customer_FirstName`, `Customer_LastName`, `Membership_Number`, `Member_First_Name`, `Child_First_Name`, and `Child_Membership_Number` merge data.

The generated child `@U18.virgin.com` address must never be a recipient or merge field. CRM owns rendering and provisioning; the EAPI-owned copy and data contract is documented in [`docs/email-templates/young-flyer-enrolment-confirmation.md`](../../docs/email-templates/young-flyer-enrolment-confirmation.md).

## Young Flyer dummy-email suppression

The shared email delivery boundary treats any address on the exact `U18.virgin.com` domain as a non-deliverable managed-child placeholder. CCP and Adobe/ACM requests for that domain are acknowledged as intentionally suppressed without invoking the downstream provider, preventing retries while leaving normal adult and verified child addresses unchanged.

## Local verification
```bash
dotnet test VAA.EAPI.Microservices.CRM.sln -c Release --filter StoreAppointmentEmailEventProcessCommandHandlerTests
```
