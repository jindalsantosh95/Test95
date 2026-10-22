# Trips GraphQL

## FindTripByFlightDate

`FindTripByFlightDate(pnr: String!, flightDate: String!, givenName: String!, surname: String!)` returns
SCV bookings through the public schema, alongside `TripDetails`, using a scheduled local departure date.
It requires no staff role or signed-in customer token. The existing public gateway
controls still apply, and Order Data verifies both names against the same passenger
on each returned booking. Blank inputs are rejected before any downstream call.

```graphql
query FindTripByFlightDate($pnr: String!, $flightDate: String!, $givenName: String!, $surname: String!) {
  FindTripByFlightDate(pnr: $pnr, flightDate: $flightDate, givenName: $givenName, surname: $surname) {
    id
    passengers { firstName lastName }
    product {
      reference { pnrLocator bookingDate }
      checkInEligible
      slices {
        segments {
          marketingCarrier { carrierCode flightNumber }
          origin { code }
          destination { code }
          scheduledArrivalUtcDateTime
        }
      }
    }
  }
}
```

Synthetic variables: `{"pnr":"ABC123","flightDate":"31AUG25","givenName":"Jane","surname":"Doe"}`.

The query returns the existing `TripsBooking` type used by `TripDetails`. Order Data
projects SCV records into its existing `TripDetailsResponse` contract: booking identity,
point of sale, passenger and loyalty details, booking reference, flights and legs.
SCV segments are represented in stored order as one slice per segment; live journey
and reference-data enrichment are not inferred. SCV check-in and accessible-travel
eligibility are false. Missing optional dates remain null.

The shared `payment` field always returns null for this query. Payment records,
card credentials, payment types, purchase references, raw ticket/ancillary/history
payloads, storage metadata and untyped fare blobs are not returned. Booking totals
and SCV-only delivery metadata are also omitted. The SCV `id` is the composite
SCV OrderID, not a live trip resource token. Existing live queries are unchanged.

The query is now `FindTripByFlightDate` (replacing `HistoricTripDetails`).
The required input is now `flightDate` (replacing `bookingDate`), in `ddMMMyy` format,
for example `01OCT26`. Past, present and future dates are accepted. It matches the
scheduled local departure date of any segment, using SCV's persisted
`SegmentDetails[].ScheduledDepartureTime.Local_Date_Time`. UTC departure dates,
arrival dates and cancellation status do not determine the match. Missing local
departure dates cannot match. This does not establish that travel has completed.

Order Data first projects candidate `OrderID` values using the normalized PNR
prefix, then filters each exact candidate ID by flight date and both names on the
same passenger. Only an unambiguous match is retrieved and mapped: zero or more
than one matching booking returns `[]`. Multiple matching segments or passengers
within one booking do not make it ambiguous. Duplicate matching documents do.
Downstream failures remain errors; the REST router retains its empty-list behavior.
The response's `product.reference.bookingDate` remains the booking creation date.

The prefix lookup can fan out across Cosmos partitions even though it returns
only IDs. Exact candidate queries use equality on `OrderID`; partition routing
depends on the deployed container partition-key path. Validate RU and latency in
SIT before rollout; no claim of avoiding a cross-partition prefix query is made.

Group bookings still follow `TripSettings:ExcludeGroupBookings` (default true).
Accepted bookings retain all passengers at booking and product level. SCV
check-in and accessible-travel eligibility remain false.

## Configuration and deployment

The provider reuses `OrderDataMSUrl`, `OrderDataOcpKey` and `OrderDataAppId` from
the shared GraphQL environment constants, with the existing OAuth provider and
HTTP connector. It calls `POST /scv/orders/by-flight-date`. Deploy Order Data and
import its generated OpenAPI before deploying this query. No new secret or setting
is needed. Do not log request names, PNRs or booking payloads.

The Bruno example is in `../VAA.EAPI.GraphQL.Bruno/Queries/FindTripByFlightDate.bru`.

Request cancellation propagates to the downstream HTTP call. Cancellation also stops
waiting for the shared OAuth token acquisition without cancelling other callers.

The downstream SCV route is classified as sensitive by the shared connector;
failed-call request and response payloads are redacted from connector diagnostics.
The request also sends Cache-Control: no-cache, no-store to prevent compliant downstream caches from retaining booking payloads.

SCV retrieval does not establish live operational eligibility. Check-in and
accessible-travel eligibility remain false for past, current and future bookings.
