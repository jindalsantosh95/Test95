# Use Loyalty Member Management for Darwinium Fraud-Lock Orchestration

- Status: accepted
- Date: 2026-09-04
- Scope: `VAA.EAPI.Facade.Darwinium`, `VAA.EAPI.Microservices.Loyalty.Member.Management`, `VAA.EAPI.Microservices.Loyalty.Customer`, and Darwinium integration documentation

## Context

The original Darwinium fraud-lock proposal placed its dedicated Service Bus subscriber and account-status orchestration in Loyalty Customer. Loyalty Member Management already orchestrates the comparable Loops bulk member-status process: it resolves members and publishes the established Customer, VRED, B2C, Customer Transactions, Flying Club Member, and audit events. A new Darwinium path should reuse that orchestration ownership without allowing another service to write Loyalty Customer data directly.

## Decision

Loyalty Member Management owns the dedicated Darwinium fraud-lock subscriber, account matching, eligibility decision, audit attribution, and downstream event publication. It uses the existing Customer-search client and a focused Customer-document query through the Loyalty Customer API to obtain the authoritative current state.

Loyalty Customer remains the owner of the Customer document. Member Management requests its mutation by publishing the existing `VAA.Loyalty.Customer.UpdateCustomerDocument` contract, including server-controlled `LastUpdatedBy = Darwinium`; the existing Loyalty Customer subscriber performs persistence. The external ingress remains a dedicated APIM-fronted OAuth facade and all fraud alerts remain asynchronous.

## Consequences

- Reuses the established Member Management event-orchestration boundary and avoids a duplicate Darwinium workflow in Loyalty Customer.
- Preserves Loyalty Customer's data ownership and prevents direct data-store access from Member Management.
- The Member Management terminal result confirms durable acceptance of its events, not that downstream subscribers have completed. Customer and other consumer failures need their established monitoring and reconciliation paths.
- The existing blob/CSV `BulkMemberUpdate` endpoint and handler are unsuitable for direct reuse; the new capability needs a dedicated, single-member command and message contract.
- A Customer-document read and an agreed conditional-write/status-precedence policy are required if the business needs guarantees for concurrent status updates.

## Alternatives Considered

- Keep all Darwinium subscription and orchestration in Loyalty Customer: rejected because Member Management already owns the equivalent status-change fan-out workflow.
- Let Member Management write the Customer data store directly: rejected because it breaks Loyalty Customer data ownership.
- Send Darwinium notifications through the existing bulk-upload endpoint: rejected because its blob/CSV and PowerApps contract does not represent a protected, single-member external fraud alert.

## Follow-up Controls

- Guide: [Darwinium fraud-account-lock sequence](../sequence-diagrams/vaa-eapi-darwinium-flying-club-fraud-account-lock.md)
- Sensor: dedicated facade and Member Management handler/subscriber tests; existing Loyalty Customer Customer-document subscriber tests; downstream event monitoring and reconciliation.
