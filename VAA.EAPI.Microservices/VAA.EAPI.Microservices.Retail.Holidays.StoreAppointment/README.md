# VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment

Azure Functions isolated-worker microservice for Holidays store operations.

## Capabilities
- Save store appointment bookings and publish CRM email event payloads.
- Fetch nearby stores, store details, store slots, and calendar availability.
- Provide an internal Store Manager API surface backed by SQL stored procedures.
- Store Manager authorization is enforced at the GraphQL Holidays entry point. The microservice routes are internal data-operation endpoints and intentionally use anonymous Function triggers.
- Slot regeneration is internal. It runs when opening hours or slot rules are replaced, and can also be invoked through the microservice-only `POST /stores/{storeId}/slots/generate` endpoint.

## Local run
```bash
dotnet restore VAA.EAPI.Microservices.Retail.Holidays.sln
dotnet build VAA.EAPI.Microservices.Retail.Holidays.sln -c Release
dotnet test VAA.EAPI.Microservices.Retail.Holidays.sln -c Release --filter StoreAppointment
```

Start the function host from this project folder:
```bash
cd VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment
func start
```

## Local smoke tests
- [Public store and booking flow](docs/smoke-tests/public-store-booking-flow.md)
- [Manage my-store flow](docs/smoke-tests/manage-my-store-flow.md)

Frontend integration docs:
- [Store Manager GraphQL integration guide](docs/frontend/store-manager-graphql-integration.md)
- [Store Manager GraphQL operations](docs/frontend/store-manager-graphql-operations.md)
- [Store Manager frontend authorization guide](docs/frontend/store-manager-authorization-guide.md)

OpenAPI UI:
```text
http://localhost:7071/api/swagger/ui
```

## Endpoints
- `POST /Holidays/Storeappointments`
- `POST /Stores/GetAllStores`
- `POST /Stores/GetStoresById`
- `POST /Stores/GetSlotsByDate`
- `POST /Stores/GetCalendarAvailability`
- `GET|PUT /stores/{id}`
- `GET|PUT /stores/{id}/opening-hours`
- `GET|PUT /stores/{storeId}/slot-rules`
- `GET /stores/{storeId}/slots?from=...&to=...`
- `PUT /stores/{storeId}/slots/{slotId}/status`
- `POST /stores/{storeId}/slots/generate` (internal regeneration endpoint; no GraphQL/frontend operation)
- `GET /stores/{storeId}/availability?date=...`
- `GET /stores/{storeId}/availability/range?from=...&to=...`
- `GET|POST /stores/{storeId}/closures`
- `DELETE /stores/{storeId}/closures/{closureId}`
- `GET /stores/{storeId}/bookings?date=...&status=...` (`status` is optional; Store Manager history uses `BOOKED` or `CANCELLED`)
- `POST /stores/{storeId}/bookings/{bookingId}/cancel`
- `GET /manage/admin/stores`
- `GET|PUT /manage/admin/stores/{id}`

Store update requests can include a `services` list using the existing Store Manager service names (`Kids area`, `Welcome drinks and snacks`, `Aircraft seats`, `Secret cinema`). The Store Manager update-store stored procedure must accept `@StoreServicesJson` as a JSON array. When `services` is omitted, `@StoreServicesJson` is sent as `NULL` and the current store services are preserved. An empty array clears all services, while one or more values replace the stored service list. Store update requests also validate contact details before SQL execution: telephone numbers must be UK national numbers with no spaces, and email addresses must be valid email addresses.

## Store Manager authorization boundary
- Store Manager authorization is enforced in the GraphQL Holidays domain.
- External callers should reach Store Manager operations through GraphQL, not by calling this microservice directly.
- GraphQL resolves the authenticated caller's role and store id, then calls store-specific microservice routes such as `/stores/{storeId}/bookings`.
- The microservice does not repeat Store Manager role checks by design.

## Environment variables
- `StoreAppointmentSqlConnectionString`
- `StoreAppointmentServiceBusConnection`
- `GoogleDistanceMatrixBaseUrl` (optional)
- `GoogleDistanceMatrixApiKey`
- `GoogleMapsDirectionsBaseUrl` (optional)
- `StoreDefaultImageSrc` (optional)
- `StoreDefaultImageAlt` (optional)

## SQL-backed store configuration
- Store master/contact/work-hours data is expected to come from SQL schema-aligned tables and stored procedures.
- Store locator reads (`/Stores/GetAllStores` and `/Stores/GetStoresById`) return `markedAsClosed` from `StoreMaster.MarkedAsClosed` so appointment-booker clients can identify stores closed at store level.
- Slot definitions are stored in SQL and regenerated from weekly opening hours and slot rules.
- Public appointment-booker slot reads (`/Stores/GetSlotsByDate` and `/Stores/GetCalendarAvailability`) honour the store closure flag and current weekly opening hours. `StoreMaster.MarkedAsClosed` suppresses all generated slots; closed days in `StoreWorkHoursJson` suppress that day; and open days only expose generated slots inside the configured opening window. If the requested day is missing from work hours, has invalid times, or has an overnight/equal opening range, availability fails closed and returns no public slots. Calendar availability still omits past dates.
- Store Manager update-store/admin-store requests that mark a store closed deactivate active `StoreSlot` rows for that store. Reopening a previously closed store regenerates slots from the current slot rules.
- Booking capacity is enforced by stored procedures and surfaced by the API as a business validation response.
- Booking cancellation is an audited soft transition from `BOOKED` to `CANCELLED`. It releases the reusable allocation by clearing the booking's allocation link, retains booking history, records the UTC cancellation time and trusted actor, and is idempotent for an already-cancelled booking. Live allocation reads therefore expose only `AVAILABLE`, `BOOKED`, or `BLOCKED`; cancelled history remains in the bookings query and does not contribute to booked or remaining-capacity counts.
- Closure creation is rejected with `ACTIVE_BOOKINGS_EXIST` when the selected UK-local date still contains active `BOOKED` appointments. The booking check and closure insert run in one SQL transaction.
- Store closure, opening-hours replacement, and slot-rule replacement are rejected with `ACTIVE_BOOKINGS_EXIST` when the proposed configuration would affect `BOOKED` appointments from the current UK-local date (inclusive) to the same date 12 calendar months later (exclusive). Conflict details include the range, total active booking count, and ascending per-date counts; `CANCELLED` appointments remain in history and do not block changes.
- Slot-rule replacement treats any existing rule with changed start/end time, changed duration, decreased capacity, or removal as affecting its weekday. Every active booking on an affected weekday is reported. Pure additions and capacity increases are non-blocking.
- Schedule-impact validation, configuration replacement, and slot regeneration share one database transaction. Booking writers lock and revalidate the selected slot so a concurrent booking cannot bypass the guard.
- Initial slot-rule setup can be run with `dbo.usp_StoreManager_BootstrapDefaultSlotRules`. Passing a store id bootstraps one store; omitting `@StoreId` bootstraps all active stores without active slot rules. The procedure creates default rules inside opening hours using 60 minute slots and capacity 1, then calls slot generation for each bootstrapped store.
- `POST /stores/{storeId}/slots/generate` regenerates slots for the single store id in the route. It does not regenerate slots for every store.

## Store Manager conflict contract

Unsafe Store Manager writes return stable error codes:

| Operation | Condition | Code / reason |
| --- | --- | --- |
| Create one-day closure | Active bookings exist on the selected UK-local date | `ACTIVE_BOOKINGS_EXIST` |
| Mark store closed | A `BOOKED` appointment exists in the rolling UK-local 12-calendar-month range | `ACTIVE_BOOKINGS_EXIST` / `STORE_MARKED_CLOSED` |
| Replace opening hours | A booked appointment is on a removed/closed weekday or outside the proposed hours | `ACTIVE_BOOKINGS_EXIST` / `OUTSIDE_PROPOSED_OPENING_HOURS` |
| Replace slot rules | An existing rule is removed or its start, end, or duration changes; every active booking on that weekday conflicts | `ACTIVE_BOOKINGS_EXIST` / `NO_MATCHING_SLOT_RULE` |
| Replace slot rules | An existing rule's capacity decreases; every active booking on that weekday conflicts | `ACTIVE_BOOKINGS_EXIST` / `CAPACITY_REDUCTION` |
| Cancel booking | Booking does not belong to the supplied store | `BOOKING_NOT_FOUND` |
| Cancel booking | Booking cannot transition to cancelled | `BOOKING_NOT_CANCELLABLE` |
| Cancel booking | Customer contact was not confirmed | `CUSTOMER_CONTACT_NOT_CONFIRMED` |

## Test coverage

- Controller tests cover structured closure/schedule conflicts, cancellation request forwarding, malformed cancellation JSON, and standard success/failure response mapping.
- Repository validation tests cover required customer-contact confirmation, the supported `STORE_CLOSURE` reason, and the trusted cancellation actor.
- Store Manager repository tests cover opening-hours and slot-rule request validation and slot-allocation response mapping.
- SQL-backed atomicity, locking, date-window boundaries, cancellation exclusion, rule comparison, and rollback behavior require integration verification against the deployed database procedures; the repository's in-memory EF tests cannot execute those procedures.
- The test project covers cancellation ownership, idempotency, allocation release, request validation, and response mapping.
