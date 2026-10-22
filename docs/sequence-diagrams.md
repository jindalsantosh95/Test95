# Sequence Diagrams

This document is the repository index for detailed sequence diagrams.

"Goal Delivery Document" (GDD) is an alternative name for a sequence diagram. Both names refer to the same artifact and follow exactly the same guidance, status model, and delivery process in this document.

Detailed diagrams should live in `docs/sequence-diagrams/` or in a solution-owned docs folder when the diagram is too specific for the repository index. Diagrams should reflect implemented code paths rather than placeholder workflows. Unless the requester specifically asks for implementation detail, sequence diagrams should show the flow between services, systems, and external dependencies rather than internal handlers, resolvers, providers, or repositories. Implemented diagrams should have a matching behavior sensor in [`behavior-harness.md`](behavior-harness.md) or the owning project's test documentation; proposed diagrams should state the expected harness before implementation lands.

## Agent Loop Goal Status Model

Use each diagram as an agent-loop goal artifact with one of these statuses:

| Status | Meaning | Agent loop expectation |
|---|---|---|
| `Draft` | Initial or evolving goal. Flow or constraints are still being clarified. | Refine scope, dependencies, and failure paths until implementation-ready details are complete. |
| `Ready` | Goal is clear, reviewed, and implementation-ready. | Start implementation work against this diagram without further design discovery. |
| `InProgress` | Implementation or validation work is underway. | Keep the diagram aligned with active changes and record open points blocking completion. |
| `Complete` | Implemented behavior and harness coverage are aligned with the diagram. | Maintain as authoritative reference and update when behavior changes. |

### Transition Gates

Use these criteria before moving status:

| Transition | Required evidence |
|---|---|
| `Draft -> Ready` | Scope, participants, dependencies, failure paths, and open points are explicit; owner confirms implementation readiness. |
| `Ready -> InProgress` | Implementation task/PR started and the diagram is the active goal artifact for the change. |
| `InProgress -> Complete` | Implemented behavior matches diagram and the diagram references executable harness coverage. |
| `Complete -> Draft` | Trigger only when behavior drift or redesign invalidates the current diagram. |

When creating or updating a sequence diagram, include an `Agent Loop Goal` section in the diagram file and keep its status in sync with this index. The section must include `Goal`, `Status`, `Owner`, `Review cadence`, `Last reviewed`, `Harness Sensor`, `Promotion verification`, `Acceptance evidence`, and `Done when`.

Keep Draft and Ready goal tables focused on those reviewable fields. Do not copy placeholder agent-resource rows into each goal; the completion and PR-closure workflows add the measured resource rows when evidence exists. Omit the `UX contract` section entirely when no UX artefact was supplied.

Use `Promotion verification` to select the promotion gate:

- `Fast`: scoped harness verification with `-SkipBuild`.
- `Full`: solution/project verification without `-SkipBuild` for implementation goals that need build/test confidence before `Complete`.

Use `Acceptance evidence` to state the concrete evidence that the delivered implementation satisfies the diagram. The agent loop checks this field, the `Done when` condition, `Expected harness`, and open points before promoting `InProgress` goals to `Complete`.

Every diagram must include `Agent delivery constraints`, `Expected harness`, and `Open points to finalize` sections. Use `Agent delivery constraints` to name target project paths, exact contract/query/event names, required configuration, and harness commands so the coding agent can implement the goal without broad repository discovery.

When any UX artefact is supplied—including screenshots, Figma frames, prototypes, or video—also include a `UX contract`. The artefact is a direct implementation instruction for visible hierarchy, content, controls, layout, and represented states. Map every supplied screen/state to the user action, message, branch, or state transition in `Sequence (detailed)`; the sequence diagram remains authoritative for flow and behaviour. Before production edits, require a design inspection matrix that records each applicable state, target implementation surface, measurable visual and interaction properties, component/token reuse, responsive behavior, and intentional adaptations, followed by an early comparison of one representative implementation state. Apply only properties represented by the supplied artefact and target platform. If the artefact cannot be inspected or conflicts with the sequence, keep the goal out of delivery and record the conflict as an open point. A missing inspectable Figma node must not be used to downgrade a supplied screenshot to non-authoritative context.

## Diagram Index

Account linking: [agreed EAPI contract and sequence](sequence-diagrams/vaa-eapi-alexa-mcp-proxy-account-linking.md).

| Diagram | Scope | Agent Loop Goal Status |
|---|---|---|
| [Alexa Account Linking](sequence-diagrams/vaa-eapi-alexa-mcp-proxy-account-linking.md) | EAPI code delivery merged; formal Full verification closure pending. Plain-identifier proofs, single-use cache, broker-token reuse and least-data summary | `InProgress` |
| [Address Lookup v3 Suggestions and Selected-Address Retrieval](sequence-diagrams/vaa-eapi-address-lookup-v3-cost-control.md) | Additive Find-only suggestions with bounded recursive container expansion and selected-address retrieval; v2 application OAuth and existing APIM subscription reused; no signed-context/Function-key layer or APIM Key Vault reads. V2 contracts unchanged. Existing APIM publication retained; shared cleanup no longer stops apps. Existing service vault retained; coordinate service/policy rollout and fetch fresh references. No new cache or audit timers. Shared HTTP/GraphQL host security are separate PRs; production savings remain unverified. | `Draft` |
| [Check-in Notification Flow](sequence-diagrams/vaa-eapi-orchestration-checkinnotification.md) | Orchestration notifications example | `Complete` |
| [Aircraft Swap Email Eligibility Using SCV Seat History](sequence-diagrams/vaa-eapi-aircraft-swap-pnr-history-seat-eligibility.md) | Proposed ADO 856713 seat eligibility using SCV-stored history and current seats, with no live host-history lookup | `Draft` |
| [Seat Selection Reminder Notification Flow](sequence-diagrams/vaa-eapi-orchestration-seatselectionnotification.md) | Orchestration notifications example (7-day pre-departure seat reminder) | `Complete` |
| [Notification Starlink Flow](sequence-diagrams/vaa-eapi-orchestration-notificationstarlink.md) | Orchestration notifications example | `Complete` |
| [Flight Operations Flight Status Delivery Flow](sequence-diagrams/vaa-eapi-flight-operations-flight-status.md) | Add Flight Status endpoints to the existing Flights microservice and public Operations GraphQL queries over the production Delta Hosted flight-status API | `Draft` |
| [GraphQL WebForms Dynamic Form Flow](sequence-diagrams/vaa-eapi-graphql-webforms-dynamicforms.md) | WebForms schema query and submission mutation backed by CRM/Dynamics | `Draft` |
| [MyAccount Young Flyer Enrolment and Household Linking](sequence-diagrams/vaa-eapi-myaccount-under18-enrolment.md) | Cross-service authenticated-parent Young Flyer enrolment with dummy-email validity and managed household persistence | `InProgress` |
| [MyAccount Entra Native MFA Registration During Enrolment](sequence-diagrams/vaa-eapi-myaccount-entra-native-mfa-registration.md) | Complete Entra native registration only when token issuance has not already completed the enrolment authentication journey | `Complete` |
| [MyAccount Mobile Enrolment Marketing Preference Guard](sequence-diagrams/vaa-eapi-myaccount-mobile-enrolment-marketing-preferences.md) | GraphQL Account compatibility guard that resets three marketing permissions for legacy iOS and Android enrolment callers | `Complete` |
| [MyAccount Email-Update Protection](sequence-diagrams/vaa-eapi-myaccount-email-update-protection.md) | OTP-proven, rate-limited MyAccount email change that prevents B2C email-address harvesting | `Draft` |
| [Mobile Feature Configuration](sequence-diagrams/vaa-eapi-mobile-features.md) | Public mobile bootstrap query backed by Retail Reference blob configuration | `Complete` |
| [GraphQL Servicing PNR Order Validation Flow](sequence-diagrams/vaa-eapi-graphql-servicing-pnr-order-validation.md) | Secure Servicing query that validates PNR and passenger names against Order Data | `Complete` |
| [GraphQL Find Trip By Flight Date](sequence-diagrams/vaa-eapi-graphql-historic-trip-details.md) | Public Trips query for SCV bookings by PNR plus a valid local flight departure date and matching passenger names, returning only an unambiguous booking; travel completion is not inferred | `Complete` |
| [Retail Holidays TravelBox DMAZE Flow](sequence-diagrams/vaa-eapi-retail-holidays-travelbox-dmaze.md) | Implemented Retail Holidays facade boundary and delivery guidance for TravelBox capabilities supplied through DMAZE | `Complete` |
| [GraphQL Holidays Tokenized Document Download](sequence-diagrams/vaa-eapi-graphql-holidays-document-token-download.md) | Proposed GraphQL and Retail Holidays flow that exchanges short-lived JWT document tokens for DMAZE document streams without exposing downstream URLs | `InProgress` |
| [GraphQL Identity Token Validation Customer Lookup](sequence-diagrams/vaa-eapi-graphql-identity-token-validation-customer-lookup.md) | GraphQL account token validation enriched from Customer microservice lookup by email | `Complete` |
| [Native Search and Book - All Scenarios](sequence-diagrams/vaa-eapi-native-search-and-book-all-scenarios.md) | Implemented Search and Book GraphQL/microservice flows, proposed native iOS journey, and the full 88-scenario automation catalogue | `Complete` |
| [Skyscanner Flight Summary Deep-Link Discounted Offer](sequence-diagrams/vaa-eapi-skyscanner-deeplink-discounted-offer.md) | Proposed propagation of an optional Skyscanner flight-summary-page discount code through Search GraphQL and Retail Offer to Delta pricing | `Draft` |
| [Reward Seat Finder API Delivery Flow](sequence-diagrams/vaa-eapi-reward-seat-finder.md) | Backend-only feature-gated Reward Seat Finder API capability spanning the public GraphQL edge, Search aggregation, cache and Retail Offer ITA fare-family search; explicit SDL, UK-default market selection, atomic full-party pricing including taxes/fees, nonstop eligibility, bounded reference freshness and configured booking horizon; anonymous calendar access through the subscription-required Offer APIM route, with staged generic-proxy authorization; website migration is a separate delivery | `InProgress` |
| [Flight Operations - All GraphQL Scenarios](sequence-diagrams/vaa-eapi-flight-operations-all-graphql-scenarios.md) | Operations GraphQL booking retrieval plus Flight Operations flights, schedules, and crew automation coverage | `Complete` |
| [Explore - All GraphQL Scenarios](sequence-diagrams/vaa-eapi-explore-all-graphql-scenarios.md) | Appointment booking and store administration through the Store Appointment microservice | `Complete` |
| [CRM - All GraphQL Scenarios](sequence-diagrams/vaa-eapi-crm-all-graphql-scenarios.md) | EU261 web-form payment-detail submission through CRM to Dynamics 365 | `Complete` |
| [Website My Account - All GraphQL Scenarios](sequence-diagrams/vaa-eapi-website-my-account-all-graphql-scenarios.md) | Website account, identity, profile, household, rewards, and inflight account operations | `Complete` |
| [NDC - All Backend Scenarios](sequence-diagrams/vaa-eapi-ndc-all-backend-scenarios.md) | IATA NDC XML and supporting Offer, Seat, and Order Payment REST coverage | `Complete` |
| [Marketing Loyalty - All GraphQL Scenarios](sequence-diagrams/vaa-eapi-marketing-loyalty-all-graphql-scenarios.md) | Internal loyalty GraphQL operations spanning campaigns, members, transactions, partners, and audit | `Complete` |
| [Website My Trips - All Scenarios](sequence-diagrams/vaa-eapi-website-my-trips-all-scenarios.md) | Website trip retrieval and servicing, seats, SSRs, deep links, and Virgin Holidays | `Complete` |
| [Website OCI - All Backend Automation Scenarios](sequence-diagrams/vaa-eapi-website-oci-all-scenarios.md) | Website online check-in, travel requirements, baggage payment, boarding passes, and backend parity | `Complete` |
| [GraphQL Flying Club Application Token Flow](sequence-diagrams/vaa-eapi-graphql-flying-club-application-token.md) | Secure Account mutation and Identity-owned issuance of an EAPI broker token containing a Flying Club account claim | `Complete` |
| [GraphQL Flying Club Token Customer Profile Query](sequence-diagrams/vaa-eapi-graphql-flying-club-token-customer-profile.md) | Secure Account query that validates the Identity broker token before retrieving Customer, Customer Profile, and Flying Club account data | `Complete` |
| [Mobile Hotel Content GraphQL Flow](sequence-diagrams/vaa-eapi-mobile-hotel-content-graphql.md) | Implemented Mobile App AEM and VHOLS hotel/car content retrieval through Stitch, Trips GraphQL, and CRM Content | `Complete` |
| [Dual Application Insights and Datadog Logging Flow](sequence-diagrams/vaa-eapi-dual-application-insights-datadog-logging.md) | Shared EAPI structured logging with W3C correlation, Application Insights telemetry, and opt-in direct Datadog delivery | `Complete` |
| [GraphQL Custom-Event Delivery to Datadog](sequence-diagrams/vaa-eapi-graphql-datadog-custom-events.md) | Implemented Application Insights and direct Datadog destination parity for existing GraphQL business custom events | `Complete` |
| [PaxOps Travel Requirements Secure Flight Passenger Data](sequence-diagrams/vaa-eapi-trips-trusted-traveller-programs.md) | Mobile App Travel Requirements read and save flow for per-passenger KTN, redress, and Canadian traveller numbers through PaxOps GraphQL, Retail Trip, and Delta | `Draft` |
| [Darwinium Flying Club Fraud Account Lock Flow](sequence-diagrams/vaa-eapi-darwinium-flying-club-fraud-account-lock.md) | Split boundary-integration view and concise VAA-responsibility sequence for the new `VAA.EAPI.Facade.Darwinium` service, dedicated fraud-lock message, non-retrying Loyalty subscriber, configuration-driven status update, and correlation-keyed application telemetry | `Draft` |
| [Concierge GraphQL Subscription Delivery](sequence-diagrams/vaa-eapi-graphql-concierge-subscriptions.md) | Concierge domain POC with existing Functions SDK hosting; native WebSocket transport implemented; Azure/APIM acceptance outstanding | `Draft` |

## Template

Use this file when authoring new diagrams:

- [Sequence Diagram Template](sequence-diagrams/_template.sequence-diagram.md)

Creation rule:

- Copy the template into a new file under `docs/sequence-diagrams/`.
- Replace all placeholders.
- Add the new diagram to this index with matching status.
- Add or update the sensor mapping in `docs/behavior-harness.md`.
- Render changed Mermaid fences with `scripts/check-mermaid-diagrams.ps1 -Path docs/sequence-diagrams/<diagram-file>.md` before committing.
- In sequence diagrams, put each statement on its own line and avoid raw semicolons, including in notes, messages, participant labels and branch descriptions. Mermaid can treat a semicolon as a statement separator even inside prose. Use a comma or `<br/>`; when a literal semicolon is necessary, use Mermaid's `#59;` entity. For example, write `Note over A,B: Seats confirmed, send email` rather than separating those clauses with a semicolon.
- Run `scripts/check-mermaid-diagrams.ps1 -Path docs/sequence-diagrams/<diagram-file>.md -LintOnly` for an immediate authoring check without Node. This catches the punctuation rule but does not prove the diagram parses. A missing renderer must be reported as unverified rendering, never a successful validation.

## Maintenance Notes

- Add new detailed sequence diagrams as separate Markdown files.
- Link diagrams from this index only when they are useful as shared repository reference material; otherwise link them from the owning project README.
- Use actual repository project names for service participants, for example `VAA.EAPI.GraphQL.Domain.WebForms` or `VAA.EAPI.Microservices.CRM`, instead of generic labels when a project exists.
- Label every repository-owned resource in a sequence diagram or flowchart with its actual project name where one exists.
- Use Mermaid's default unstyled presentation: do not add background or fill colours, borders, boundary boxes or containers such as `box` and `subgraph`, custom themes, `style`, or `classDef` directives.
- Sequence diagrams in this repository must show connections between processes/services/systems only.
- Keep participants at service, system, broker, datastore, or external dependency level.
- Keep an explicit `Agent Loop Goal` section in each diagram file with one status from the model in this document.
- Treat Mermaid parser validation as required authoring evidence; visual plausibility is not a substitute for a successful render.
- Do not include internal handlers, subscribers, command handlers, repositories, or class-level participants unless a requester explicitly asks for implementation-internal detail.
- Keep diagram participants, route names, event names, topics, subscriptions, headers, and timing rules aligned with implementation and configuration docs.
- Keep implemented diagrams aligned with behavior tests, fixtures, or script sensors listed in the behavior harness.
- Avoid generic placeholder diagrams in this index; use this file for navigation.
- Keep template files in this folder prefixed with `_template` so harness index checks can ignore them.

## Pull Request Rule

When a PR changes behavior represented by a sequence diagram, update all of the following in the same PR:

- the affected diagram file under `docs/sequence-diagrams/`,
- the status row in this index,
- the matching sensor entry in `docs/behavior-harness.md` (or a planned-behavior row if not implemented yet).

Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/check-sequence-diagram-status.ps1` locally (or rely on `scripts/verify.ps1`, which runs it by default).

## Automation

Agent-loop delivery is automated by repository workflows plus `scripts/promote-sequence-goal.ps1` and `scripts/run-sequence-goal-agent.ps1`.

- Branch push automation (`ai-delivery-sequence-goal-loop`) detects changed sequence diagrams on non-`main` branches.
- The triggering branch remains an evidence-only coordinator; Backend, mobile/web production, and every QA agent use independent branches and PRs recorded in its goal ledger.
- The workflow opens a draft coordinator PR when the Ready goal becomes InProgress. Each parallel production surface records its PR and immediately starts only its matching QA continuation. The monitor discovers active coordinator branches directly; after all child PRs merge and integrated verification passes, the workflow marks the goal `Complete` and makes the existing coordinator PR ready.
- `Ready` goals are auto-promoted to `InProgress`.
- `InProgress` goals run the configured coding-agent command via secret `SEQUENCE_GOAL_AGENT_COMMAND_JSON`.
- On successful verify/harness checks, goals without open questions are auto-promoted to `Complete`; question-blocked goals remain `InProgress`.
- A PR to `main` is created after integrated verification, ready for completed goals or draft while verified questions remain, making PR creation the delivery artifact of the loop.
- After the PR exists, the sequence-goal PR review loop waits for AI review comments, runs the configured review agent for valid fixes, resolves only AI-only threads with `[from-codex]` comments, and only reports completion once the PR is approved for `main`. A thread containing user feedback remains unresolved for human closure.
- Non-AI PR comments are handled by a separate human-comment loop: the agent replies when it disagrees, actions and replies when it agrees, and never resolves human review threads.
- Completion evidence is included in the coordinator PR before merge. After merge, the PR finalizer records the coordinator row as `Merged` on `main`; if closed without merge, it records abandonment evidence on the source branch when available, or a sanitized standalone record under `.codex/agent-loop/abandonments/` on `main` when that branch was already deleted. AI/human comment evidence is archived with the durable source-branch update when available.
- Manual workflow smoke tests can run with `smoke_test=true` to exercise promotion, evidence, locks, and verification without invoking the external agent command or opening a PR; smoke artifacts are cleaned after completion.

Local/manual promotion (when needed):

```pwsh
scripts/promote-sequence-goal.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md -ToStatus InProgress
scripts/promote-sequence-goal.ps1 -DiagramPath docs/sequence-diagrams/<diagram-file>.md -ToStatus Complete
```
