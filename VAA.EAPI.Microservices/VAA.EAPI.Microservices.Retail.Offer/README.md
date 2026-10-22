# Internal Reward Seat Calendar Availability

Responses use the shared EAPI response helper, including its JSON content type and security headers. Successful responses retain the calendar response object. HTTP 400 returns an `ErrorDetails` array with `REWARD_SEAT_INVALID_DEADLINE` or `REWARD_SEAT_INVALID_REQUEST`; HTTP 502 returns `REWARD_SEAT_UNAVAILABLE`. Errors include an ID and safe configured messages, never raw exception details.

Send exactly one `x-reward-seat-deadline-ms` header containing an integer from `1` to `30000` inclusive, for example `x-reward-seat-deadline-ms: 25000`. The value is the remaining downstream execution budget in milliseconds. Missing, repeated, malformed or out-of-range values return HTTP 400. This required header is declared in the generated OpenAPI contract.

`POST /reward-seat-calendar-availability` is a dedicated, non-persisting Retail Offer operation. Its request contains only route and date/window criteria. It reuses the existing `IItaService.GetOffers` transport and generated ITA XML models; it does not create booking Offer documents. At the ITA boundary it adds one adult (`ADT`), VS carrier and configured GB market settings. It requests ITA `calendar` with `availSummary` and `sliceBySliceSummary`, all three reward fare families and direct flights. The response contains only non-personal, normalized availability candidates.

The calendar function allows anonymous requests: no login, Entra bearer token, application role, or client ID allowlist is required. It is included in OpenAPI and can be reached through the existing Offer APIM route, whose platform subscription and network controls remain in place. Its request validation and required `x-reward-seat-deadline-ms` header (1–30000) bound downstream work. Search keeps ITA credentials server-side.

Before enabling Search, override the Offer deployment `RewardSeatAvailabilitySettings` object with the VS booking codes (the template leaves these empty). Coordinate its `ItaVersion=161`, `MarketPolicyVersion`, `FareFamilyMappingVersion`, and `Markets:GB` (GBR/GBR/GBP/LON) with Search. Additional permitted carriers require their own `BookingCodes:{carrier}:Economy`, `Premium`, `Upper` settings. Add dedicated row keys `REWARD-SEAT-FINDER-ECONOMY`, `REWARD-SEAT-FINDER-PREMIUM`, `REWARD-SEAT-FINDER-UPPER` to the existing `ItaFareFamilyBlobName` JSON blob with exact family names `AWARD-ECONOMY`, `AWARD-COMFORT-PLUS-PREMIUM-ECONOMY`, `AWARD-BUSINESS-FIRST`. Keep existing booking rows unchanged.


The existing `ITAFlightOffer` operation and `/ITAOffers` APIM behaviour are unchanged. Reward Seat Finder reuses `IItaService.GetOffers` internally and adds no authentication middleware or APIM blocking policy to existing operations.

## PSS endpoint configuration

The Retail Offer deployment supplies `BookingProcessV3EndPoint`, `Air4PassengerV4EndPoint`, `Air4SeatV8EndPoint`, `CreditCardV2EndPoint`, and `RemarksV3EndPoint` for its PSS clients. The existing `Air4PnrCommonEndPoint` and `TicketingProcessV3EndPoint` settings are still used. Keep these settings aligned across the main, slot, digital, performance, and sandbox templates and their environment parameter files.

## Local Bruno requests

Open the sibling `VAA.EAPI.Microservices.Retail.Offer.Bruno` collection and select its `Local` environment. Disable the collection's APIM subscription header for local Functions calls. For the Dev environment, set the `apim_subscription_key` secret locally; the enabled collection header sends it with every request. The `Common`, `Delta`, and `ITA` folders mirror `Triggers/Http` and include starter requests for heartbeat, flight availability, and reward seat calendar availability. Update the sample dates and identifiers before sending requests. For the reward request, configure the ignored `local.settings.json` with local ITA and reference-blob access and populate the three reward fare-family rows described above.
