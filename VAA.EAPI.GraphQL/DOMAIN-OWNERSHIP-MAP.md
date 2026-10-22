# VAA.EAPI.GraphQL Domain Ownership Map

This onboarding map links `VAA.EAPI.GraphQL.Domain.*` projects to business capabilities and the team that should be engaged first.

## How To Use This Map

- Use this as the default routing table for onboarding, triage, and reviewer selection.
- Team ownership can evolve; treat this as the current onboarding baseline.
- If ownership is unclear, route first to the **EAPI GraphQL Technical Leads** and the capability team listed below.

## Domain To Capability To Team

| Project | Primary Business Capability | Responsible Team (Onboarding Route) | Supporting Teams |
| --- | --- | --- | --- |
| `VAA.EAPI.GraphQL.Domain.Account` | My Account, member profile, account security flows, enrolment | Loyalty & Account Experience Team | Identity/Security, CRM, Retail |
| `VAA.EAPI.GraphQL.Domain.Basket` | Basket orchestration and basket-facing GraphQL composition | Retail Booking & Basket Team | Retailing, Book, Search |
| `VAA.EAPI.GraphQL.Domain.Book` | Booking flow, payment, seat/contact updates, insurance/co-brand | Retail Booking & Payments Team | Retailing, Security, Loyalty |
| `VAA.EAPI.GraphQL.Domain.Holidays` | Holidays store locator, appointment booking, store manager operations | Holidays Digital Team | Holidays Services, Retail Platform |
| `VAA.EAPI.GraphQL.Domain.Holidays.Services` | Holidays domain shared service components | Holidays Digital Team | GraphQL Platform Team |
| `VAA.EAPI.GraphQL.Domain.Loyalty` | Loyalty account, campaigns, transactions, household, profile updates | Loyalty Platform Team | Account, Security, Data/RefData |
| `VAA.EAPI.GraphQL.Domain.NDCHealthCheck` | NDC downstream health and readiness checks | Retail NDC / Partner Connectivity Team | Platform Runtime, SRE |
| `VAA.EAPI.GraphQL.Domain.Operations` | Operational flight and manifest queries | Flight Operations Digital Team | PaxOps, Trips, RefData |
| `VAA.EAPI.GraphQL.Domain.PaxOps` | Passenger operations (check-in, baggage, travel requirements) | Ground Operations Digital Team | Operations, Trips, Security |
| `VAA.EAPI.GraphQL.Domain.RefData` | Shared reference data queries (airports, countries, programmes, products) | Data & Reference Services Team | All domain teams |
| `VAA.EAPI.GraphQL.Domain.Retailing` | Retail order and ancillaries orchestration | Retail Order & Ancillaries Team | Book, Trips, Search |
| `VAA.EAPI.GraphQL.Domain.Search` | Offer/search BFF composition, pricing/reprice, content and sustainability overlays | Retail Search & Offers Team | Retailing, RefData, Loyalty |
| `VAA.EAPI.GraphQL.Domain.Security` | APIM subscription and GraphQL-facing security operations | Identity & Security Platform Team | Account, Platform Runtime |
| `VAA.EAPI.GraphQL.Domain.Servicing` | Booking retrieval, order search, validation for servicing flows | Retail Servicing Team | Trips, Retailing, Customer Care |
| `VAA.EAPI.GraphQL.Domain.Trips` | Post-booking trip details, seats/meals/SSR, upsell, loyalty actions | Trips & Post-Booking Team | PaxOps, Retailing, Loyalty |
| `VAA.EAPI.GraphQL.Domain.VHols` | Virgin Holidays auth and booking retrieval flows | Holidays Digital Team | Account, Servicing |
| `VAA.EAPI.GraphQL.Domain.WebForms` | Web form workflows (including refund request forms) | CRM & Customer Care Team | Servicing, Security |

## Team Routing Notes

- **Retail Booking & Payments Team**: booking lifecycle, payment state, basket/order mutation behavior.
- **Retail Search & Offers Team**: shopping/search orchestration and offer shaping.
- **Loyalty Platform Team**: loyalty profile, campaigns, membership, and transactions.
- **Ground/Flight Operations Digital Teams**: day-of-travel and operational flight/passenger capabilities.
- **Identity & Security Platform Team**: authn/authz, APIM subscription and protected operations.
- **Data & Reference Services Team**: shared reference data and lookup correctness/performance.
- **Holidays Digital Team**: Virgin Holidays-specific flows and store/appointment capability.
- **CRM & Customer Care Team**: customer support and web-form workflow integrations.

## Escalation Path

When onboarding ownership is uncertain:

1. Route to **EAPI GraphQL Technical Leads**.
2. Include the mapped responsible team from this table.
3. Include any listed supporting team when change impact crosses domain boundaries.
