# Retail Order Data

## SCV booking lookup

`POST /scv/orders/by-flight-date` accepts JSON containing required `pnr`,
`flightDate`, `firstName`, and `lastName` fields. It returns a list of matching
booking responses using the established response helper and OAuth/APIM boundary.
This supports the public Trips `FindTripByFlightDate` GraphQL query; GraphQL
does not connect directly to Cosmos DB.

The handler uses `IScvBookingRepository`, implemented by the same SCV repository
as `/svc/orders/search`. The validation endpoint remains unchanged. PNRs are
trimmed and uppercased, and both supplied names must match the same passenger
case-insensitively after trimming. `flightDate` replaces the old booking-date input
and uses `ddMMMyy` (for example `01OCT26`); past, present and future dates are accepted.
It matches any segment's scheduled local departure date in the persisted SCV
`ScheduledDepartureTime.Local_Date_Time` field. Missing local departures cannot
match; UTC departures, arrival dates and cancellation status do not filter results.

The handler first verifies that the trimmed PNR has exactly six characters.
Other nonblank PNR lengths return an empty list without any repository reads.
The repository projects only `OrderID` values for `STARTSWITH(c.OrderID, @Pnr)`,
then queries each exact candidate ID with the departure date and a same-passenger
name predicate. Only one matching document permits full retrieval and mapping;
zero or multiple matches return HTTP 200 with `[]`. Duplicate matching documents
are ambiguous; multiple matching segments or passengers in one document are not.
The handler revalidates the retrieved document before returning it.
The prefix query may fan out across partitions. Exact-ID equality enables routing
when `/OrderID` is the deployed partition key; returning IDs alone does not avoid
fan-out. Confirm the deployed partition path and measure RU/latency during SIT rollout.

The existing validation endpoint continues to search by PNR alone. The response
booking date remains the creation date. GraphQL retains its group-booking setting,
and accepted bookings retain all passengers. Invalid input returns the existing
validation error; storage failures and cancellation propagate through platform
middleware. Dates are validated as calendar dates without a comparison to today.

The service reuses `OrderSCVDBEndpoint`, `OrderDB`, and `OrdersCollection`.
No new store, connection or configuration is introduced. Responses contain
sensitive booking data and must not be logged or cached outside approved controls.

The new trigger supplies OpenAPI metadata. Deploy through the existing Order Data
pipeline and refresh APIM using its generated OpenAPI before deploying the GraphQL
consumer. The direct-service Bruno request lives in
`../VAA.EAPI.Microservices.Retail.Order.Data.Bruno/Orders/Find Trip By Flight Date.bru`.

The endpoint explicitly maps stored documents into the existing `TripDetailsResponse`
models. It returns booking identity, point of sale, passengers/loyalty membership,
booking reference, flights and legs. Each stored segment becomes one slice, retaining
its position; this does not infer the live service's journey grouping. Missing optional
dates stay null. SCV check-in eligibility is false.

No storage entities are forwarded. Payment records, card credentials, form-of-payment
values, transaction/purchase references, raw ticket/ancillary/history payloads and
untyped fare blobs are omitted before serialization. Booking totals and SCV-only
metadata are not part of this response. Source documents remain unchanged. The
GraphQL consumer uses the shared TripsBooking model and leaves payment unset.

SCV timestamp deserialization continues to accept persisted `UTC_Date_Time` and
`Local_Date_Time` aliases. Missing arrival dates do not
exclude the booking. This change does not modify the existing live TripDetails endpoint.
