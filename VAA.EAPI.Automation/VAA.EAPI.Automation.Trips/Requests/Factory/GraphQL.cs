using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Trips.Requests.Factory
{
    public static class GraphQL
    {
        public static string Booking(string usingString, string pnr, string givenNames, string surname)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Booking.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken tripDetailsRequest = variables["tripDetailsRequest"];
            tripDetailsRequest["using"] = usingString;
            tripDetailsRequest["confirmationNum"] = pnr;
            tripDetailsRequest["givenNames"] = givenNames;
            tripDetailsRequest["surname"] = surname;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetFlyReadyStatus(string bookingId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\GetFlyReadyStatus.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetSpecialAssistance(string bookingId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\SpecialAssistance\GetSpecialAssistance.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetSpecialMeals(string bookingId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\SpecialAssistance\GetSpecialMeals.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateSpecialMeals(string bookingId, string passengerId, string segmentId, string ssrCode, bool applyToAll)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\SpecialAssistance\UpdateSpecialMeals.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;
            variables["segmentId"] = segmentId;
            variables["passengerId"] = passengerId;
            variables["ssrCode"] = ssrCode;
            variables["applyToAll"] = applyToAll;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SelectSpecialAssistance(string bookingId, string passengerId, List<string> segmentIds, string ssrCode, string ssrCategoryCodeString)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\SpecialAssistance\UpdateSpecialAssistance.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;
            variables["segmentIds"] = JArray.FromObject(segmentIds);
            variables["passengerId"] = passengerId;
            variables["ssrCode"] = ssrCode;
            variables["ssrCategoryCode"] = ssrCategoryCodeString;

            return JsonConvert.SerializeObject(payloadObject);
        }
        public static string LoyaltyProgramsDetails()
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\ManageLoyalty\LoyaltyProgramDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateLoyaltyMemberDetails(string bookingId, string loyaltyMemberId, string loyaltyProgramCode, string leadPassengerId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\ManageLoyalty\ManageLoyaltyMember.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Navigate through the JSON structure
            JToken variables = payloadObject["variables"];
            JToken loyaltyMemberDetailsRequest = variables["loyaltyMemberDetailsRequest"];

            // Update all required fields
            loyaltyMemberDetailsRequest["loyaltyMemberId"] = loyaltyMemberId;
            loyaltyMemberDetailsRequest["loyaltyProgramCode"] = loyaltyProgramCode;
            loyaltyMemberDetailsRequest["passengerId"] = leadPassengerId;
            loyaltyMemberDetailsRequest["tripReservationId"] = bookingId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetSeatMap(string bookingId, string segmentId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Seats\SeatMap.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;
            variables["segmentId"] = segmentId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SelectSeatMutation(string bookingId, string segmentId, string passengerId, string seatNumber, string seatOfferId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Seats\SelectSeatMutation.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Navigate through the JSON structure
            JToken variables = payloadObject["variables"];
            JToken selectSeatRequest = variables["selectSeatRequest"];

            // Update all required fields
            selectSeatRequest["tripReservationId"] = bookingId;
            selectSeatRequest["segmentId"] = segmentId;
            selectSeatRequest["passengerId"] = passengerId;
            selectSeatRequest["seatNumber"] = seatNumber;
            selectSeatRequest["seatOfferId"] = seatOfferId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SeatPaymentMutation(string bookingId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Seats\SeatPaymentMutation.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Navigate through the JSON structure
            JToken variables = payloadObject["variables"];
            JToken seatPaymentMutationRequest = variables["request"];

            // Update all required fields
            seatPaymentMutationRequest["tripReservationId"] = bookingId;
            seatPaymentMutationRequest["paymentSuccessLink"] = "https://stg.virginatlantic.com/payment/captured";
            seatPaymentMutationRequest["paymentFailureLink"] = "https://testSuccessLink.com/payment/failure";
            seatPaymentMutationRequest["emailAddress"] = VAA.EAPI.Automation.Core.Constants.Application.EmailAddressForTripsSeatsTests;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SeatFulfilmentMutation(string bookingId, string paymentStatus)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Seats\SeatFulfilmentMutation.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken seatFulfilmentMutationRequest = variables["request"];

            seatFulfilmentMutationRequest["tripReservationId"] = bookingId;
            seatFulfilmentMutationRequest["paymentStatus"] = paymentStatus;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SeatConfirmationMutation(string bookingId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Seats\SeatConfirmationMutation.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string Content(string contentFragmentId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Content\Content.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["contentFragmentId"] = contentFragmentId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetContentFragment(string hotelId)
        {
            JObject payloadObject = new()
            {
                ["query"] = "query GetContentFragment($contentFragmentId: String, $contentRequest: ContentRequestInput) { GetContentFragment(contentFragmentId: $contentFragmentId, contentRequest: $contentRequest) }",
                ["variables"] = new JObject
                {
                    ["contentFragmentId"] = JValue.CreateNull(),
                    ["contentRequest"] = new JObject
                    {
                        ["sourceType"] = "VHOLS",
                        ["detail"] = new JObject
                        {
                            ["contentFragmentName"] = JValue.CreateNull(),
                            ["contentId"] = hotelId,
                            ["brandCode"] = "WW",
                            ["contentType"] = "HOTEL"
                        }
                    }
                }
            };

            return JsonConvert.SerializeObject(payloadObject);
        }
        public static string GetUpsellOffers(string bookingId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\UpsellOffers\GetUpsellOffers.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["tripReservationId"] = bookingId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        // Removed legacy single-PNR overload `TripDetails(string usingString, string pnr,
        // string givenNames, string surname)`. It read `variables["tripDetailsRequest"]`
        // from TripDetails.json, but that JSON now uses `variables.request.tripDetails[]`
        // (the multi-entry shape) — so the overload would null-deref. No callers
        // remained in the solution: TC02/TC03 use the list overload below, and the
        // single-PNR Booking flow has its own `Factory.GraphQL.Booking(...)` method
        // hitting Booking.json (which still has `tripDetailsRequest` in its variables).

        public static string TripDetails(List<string> pnrs, List<string> givenNames, List<string> surnames, List<string>? createdDates)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\TripDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken request = variables["request"];
            JArray tripDetailsArray = [];

            for (int i = 0; i < pnrs.Count; i++)
            {
                JObject tripDetail = new()
                {
                    ["createdDate"] = createdDates?[i],
                    ["givenNames"] = givenNames[i],
                    ["pnrLocator"] = pnrs[i],
                    ["surname"] = surnames[i],
                    ["bookingReference"] = null,
                    ["dateOfBirth"] = null
                };
                tripDetailsArray.Add(tripDetail);
            }

            request["tripDetails"] = tripDetailsArray;

            return JsonConvert.SerializeObject(payloadObject);
        }

        /// <summary>
        /// Mixed-shape <c>TripDetails</c> request: each entry can carry either a
        /// PNR-style identification (<c>pnrLocator</c> + <c>givenNames</c> + <c>surname</c>,
        /// optionally <c>createdDate</c>) or a Holidays-style identification
        /// (<c>bookingReference</c> + <c>dateOfBirth</c> + <c>surname</c>). Unused
        /// fields on a given entry stay <c>null</c> and serialise as JSON <c>null</c>,
        /// matching the wire format the GraphQL endpoint expects.
        /// </summary>
        public static string TripDetails(List<TripDetailEntry> entries)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\TripDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken request = variables["request"];
            JArray tripDetailsArray = new();

            foreach (TripDetailEntry entry in entries)
            {
                JObject tripDetail = new()
                {
                    ["bookingReference"] = entry.BookingReference,
                    ["dateOfBirth"] = entry.DateOfBirth,
                    ["createdDate"] = entry.CreatedDate,
                    ["givenNames"] = entry.GivenNames,
                    ["pnrLocator"] = entry.PnrLocator,
                    ["surname"] = entry.Surname
                };
                tripDetailsArray.Add(tripDetail);
            }

            request["tripDetails"] = tripDetailsArray;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string FindTripByFlightDate(string pnr, string flightDate, string givenName, string surname)
        {
            string payload = File.ReadAllText(Path.Combine(
                "Requests",
                "Body",
                "GraphQL",
                "Trips",
                "FindTripByFlightDate.json"));
            JObject payloadObject = JObject.Parse(payload);
            JObject variables = (JObject)payloadObject.RequireToken("variables");

            variables["pnr"] = pnr;
            variables["flightDate"] = flightDate;
            variables["givenName"] = givenName;
            variables["surname"] = surname;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetDeepLinks(string bookingId, string deeplinkOption, string segmentId = null)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\DeepLinks.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Navigate through the JSON structure
            JToken variables = payloadObject["variables"];
            JToken deepLinkQueryRequest = variables["request"];

            // Update all required fields
            deepLinkQueryRequest["deeplinkOption"] = deeplinkOption;
            deepLinkQueryRequest["tripReservationId"] = bookingId;

            if (deeplinkOption == "POPSMeal" && !string.IsNullOrEmpty(segmentId))
            {
                deepLinkQueryRequest["additionalData"] = new JArray { segmentId };
            }

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetDeepLinksSecure(string cacheId)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\DeepLinksSecure.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Update the getDeepLinkForPostId variable
            payloadObject["variables"]["getDeepLinkForPostId"] = cacheId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string AccountMemberSignIn(string flyingClubNumber, string password)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\MYA\AccountMemberSignIn.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Navigate through the JSON structure
            JToken variables = payloadObject["variables"];
            JToken accountMemberSignInRequest = variables["request"];

            // Update all required fields
            accountMemberSignInRequest["flyingClubNumber"] = flyingClubNumber;
            accountMemberSignInRequest["password"] = password;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string AccountMemberSignInVAC(string emailVAC, string password)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\MYA\AccountMemberSignInVAC.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            // Navigate through the JSON structure
            JToken variables = payloadObject["variables"];
            JToken accountMemberSignInRequest = variables["request"];

            // Update all required fields
            accountMemberSignInRequest["email"] = emailVAC;
            accountMemberSignInRequest["password"] = password;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string TripDetailsLoggedInUserToken()
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\TripDetailsLoggedInUserToken.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken request = variables["request"];
            JArray tripDetailsArray = new();

            request["tripDetails"] = tripDetailsArray;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveHolidayBooking(string usingString, string bookingReference, string dateOfBirth, string surname)
        {
            string payload = File.ReadAllText(@"requests\Body\GraphQL\Trips\Holidays\RetrieveHolidayBooking.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken request = variables["request"];
            request["using"] = usingString;
            request["bookingReference"] = bookingReference;
            request["dateOfBirth"] = dateOfBirth;
            request["surname"] = surname;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string HolidayPaymentMutation(string tripResourceId, decimal amountToPay)
        {
            string payloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Body", "GraphQL", "Trips", "Holidays", "HolidayPaymentMutation.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken request = variables["request"];
            request["tripResourceId"] = tripResourceId;
            request["amountToPay"] = amountToPay;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveHolidayBookingDocumentDetails(string tripResourceId)
        {
            string payloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Body", "GraphQL", "Trips", "Holidays", "RetrieveHolidayBookingDocumentDetails.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken request = variables["request"];
            request["tripResourceId"] = tripResourceId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveHolidayBookingDocument(string documentId, string documentToken)
        {
            string payloadPath = Path.Combine(AppContext.BaseDirectory, "Requests", "Body", "GraphQL", "Trips", "Holidays", "RetrieveHolidayBookingDocument.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);
            JToken request = payloadObject["variables"]["request"];
            request["documentId"] = documentId;
            request["documentToken"] = documentToken;

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
