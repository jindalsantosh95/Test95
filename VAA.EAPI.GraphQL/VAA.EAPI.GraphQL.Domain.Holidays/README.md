# VAA.EAPI.GraphQL.Domain.Holidays

GraphQL Holidays domain project for store-locator and holidays features.

## Store Manager GraphQL surface
The domain exposes Store Manager operations backed by the Store Appointment microservice HTTP endpoints.

### Queries
- Stores: `storeManagerStoreById`
- Store setup: `storeManagerOpeningHours`, `storeManagerSlotRules`, `storeManagerSlots`, `storeManagerAvailability`, `storeManagerAvailabilityRange`, `storeManagerClosures`
- Bookings: `storeManagerMyBookings`, `storeManagerBookings`
- My-store context: `storeManagerMe`
- Shared store-selection reads: `storeManagerAdminStores`, `storeManagerAdminStoreById`

### Mutations
- Store mutations: `storeManagerUpdateStore`, `storeManagerUpdateAdminStore`
- Opening hours and slot rules: `storeManagerReplaceOpeningHours`, `storeManagerReplaceSlotRules`
- Slot status: `storeManagerUpdateSlotStatus`
- Closures: `storeManagerCreateClosure`, `storeManagerDeleteClosure`
- Bookings: `storeManagerCancelBooking`

## Notes
- Slot generation is not exposed as a GraphQL mutation. It is handled by the Store Appointment microservice after opening hours or slot rules are replaced.
- Closure bulk replace is not exposed because it is not used by the frontend.
- Booking by id/create and admin store create/delete are not part of the implemented Store Manager GraphQL surface.
- `storeManagerBookings(storeId, date, status)` supports selected-store `BOOKED` or `CANCELLED` history. Dates are passed to Store Appointment as UK-local calendar dates.
- `storeManagerCancelBooking` derives cancellation actor fields from authenticated claims; the browser supplies only the `STORE_CLOSURE` reason and customer-contact acknowledgement. The generated input contract is `BookingCancellationRequestInput`, and `reason` accepts the exact GraphQL enum value `STORE_CLOSURE`.
- `storeManagerBookings` uses `StoreManagerBookingStatus`; its accepted GraphQL enum values are exactly `BOOKED` and `CANCELLED`. Both Store Manager enums use explicit Hot Chocolate type mappings so frontend enum literals and variables parse consistently.
- Cancelled bookings are returned from `storeManagerBookings`, not from live slot allocations. The frontend joins history to the live slot by `storeSlotId` (or exact start/end time as fallback); cancelled records do not consume capacity and may coexist with a newly reused live allocation.
- Existing store, opening-hours, and slot-rule mutations retain their current GraphQL inputs and success types. Unsafe changes surface `ACTIVE_BOOKINGS_EXIST` with `storeId`, `rangeStart`, `rangeEndExclusive`, `activeBookingCount`, ascending per-date `affectedDates`, and `reason` extensions.
- Store Manager downstream errors use stable GraphQL codes. Closure conflicts also expose `storeId`, `closureDate`, and `activeBookingCount` in GraphQL extensions.
- Delete operations return `Boolean` in GraphQL and throw GraphQL errors for downstream non-success responses.
- Store Manager IDs remain opaque strings. Absolute timestamps use `DateTimeOffset`, and closure dates use `LocalDate`, matching the Store Appointment microservice contract.
- Opening-hours time values remain strings because the downstream contract permits non-time values such as `closed`. Slot-rule times are validated as `HH:mm`.
- Authorization:
  - All Store Manager queries and mutations require role `VAH.RetailStore.Store.Manager` or `VAH.RetailStore.Admin`; both roles can currently use every operation.
  - The legacy Admin-named operations `storeManagerAdminStores`, `storeManagerAdminStoreById`, and `storeManagerUpdateAdminStore` remain separate GraphQL contracts so role-specific access can be reintroduced without adding replacement fields if requirements change.
  - The Admin-named store queries provide the shared store-selection data used throughout the Store Manager frontend.
  - `VAH.RetailStore.Store.Manager` access is restricted by operation, not by store. Operations that accept a store id intentionally allow managers to view or manage stores other than the store in their token claims.
  - My-store operations resolve the authenticated caller store context from token claims where the operation does not accept a store id.
- The microservice does not repeat Store Manager role checks by design.

### Active-booking conflict response

Store closure, opening-hours replacement, and slot-rule replacement use this GraphQL error shape:

```json
{
  "errors": [
    {
      "message": "The requested change cannot be saved because it would affect active bookings.",
      "extensions": {
        "code": "ACTIVE_BOOKINGS_EXIST",
        "storeId": "1025",
        "rangeStart": "2026-07-24",
        "rangeEndExclusive": "2027-07-24",
        "activeBookingCount": 3,
        "affectedDates": [
          {
            "date": "2026-07-27",
            "activeBookingCount": 2
          },
          {
            "date": "2026-08-03",
            "activeBookingCount": 1
          }
        ],
        "reason": "NO_MATCHING_SLOT_RULE"
      }
    }
  ]
}
```

`affectedDates` is ordered by ascending local date, contains each date once, and its counts total `activeBookingCount`. The existing one-day closure conflict continues to use `closureDate` and `activeBookingCount`.

## Nearby Stores Query
- Query: `RetrieveAllNearByStores`
- Response model: `NearbyStoreResult` containing `stores: [StoreResult]`.
- Store records include `markedAsClosed` from Store Appointment `StoreMaster.MarkedAsClosed`.
- Provider calls the Store Appointment microservice store-locator endpoint.

## Store Details Query
- Query: `RetrieveStoreDetailsByID(storeId: String!, origin: String!)`
- Response model: `StoreResult`.
- Store details include `markedAsClosed` from Store Appointment `StoreMaster.MarkedAsClosed`.

## Store Slots By Date Query
- Query: `RetrieveStoreSlotsByDate(storeId: String!, date: DateTime!)`
- Response model: `StoreSlotsResponse` with `storeId` and `slots`.
- Availability is delegated to the Store Appointment microservice. Public appointment-booker results honour store closure and current opening hours: stores marked closed return no slots, closed days return no slots, and open days only expose generated slots inside the opening window. Missing work-hours entries, invalid time values, and overnight/equal opening ranges fail closed.

## Store Calendar Availability Query
- Query: `RetrieveStoreCalendarAvailability(storeId: String!, year: Int!, month: Int!)`
- Response model: `StoreCalendarAvailabilityResponse` with day-level `hasAvailableSlot` values.
- Past dates are omitted by the Store Appointment microservice. Future/current days only report availability when the store is not marked closed, the requested day has a valid non-overnight opening window, and at least one generated slot is available inside that configured window.

## Save Appointment Booking Mutation
- Mutation: `SaveAppointmentBooking(bookingDetails: AppointmentBookingRequest!)`
- Provider calls the Store Appointment microservice endpoint `/Holidays/Storeappointments`.

## Tests

- `GraphQlOperationContractTests` verifies operation-name constants and parses the exact frontend enum literals `STORE_CLOSURE`, `BOOKED`, and `CANCELLED` through initialized Hot Chocolate enum types.
- Store Manager mutation/query pass-through tests cover cancellation, selected-store cancelled history, UK-offset timestamps, and the downstream HTTP routes.
- Cancellation validation tests cover missing customer-contact confirmation; error-contract tests cover `BOOKING_NOT_FOUND`, `BOOKING_NOT_CANCELLABLE`, the unchanged one-day closure extensions, and the exact schedule `ACTIVE_BOOKINGS_EXIST` response shown above.
- Authorization contract tests verify every Store Manager resolver requires either `VAH.RetailStore.Store.Manager` or `VAH.RetailStore.Admin`. MVP authorization is operation-based: either role may manage any selected store, while the microservice still validates that the booking belongs to the supplied `storeId`.
