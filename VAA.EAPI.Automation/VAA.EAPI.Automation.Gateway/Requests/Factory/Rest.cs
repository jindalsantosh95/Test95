using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Gateway.Helpers;

namespace VAA.EAPI.Automation.Gateway.Requests.Factory
{
    public static class Rest
    {
        public static string Booking(string usingString, string confirmationNum, string givenNames, string surname)
        {
            JObject variables = PayloadHelper.GetVariables(@"Requests\Body\GraphQL\Trips\Booking.json");
            if (variables["tripDetailsRequest"] is not JObject tripDetailsRequest)
            {
                throw new InvalidOperationException(
                    "Booking payload is missing a 'tripDetailsRequest' object under 'variables', or it is not a JSON object.");
            }

            tripDetailsRequest["using"] = usingString;
            tripDetailsRequest["confirmationNum"] = confirmationNum;
            tripDetailsRequest["givenNames"] = givenNames;
            tripDetailsRequest["surname"] = surname;

            return JsonConvert.SerializeObject(variables);
        }

        public static string DeepLinks(string tripReservationId, string deeplinkOption)
        {
            // Source-of-truth GraphQL payload lives in the Trips project. PayloadHelper will first
            // look in the test output (AppContext.BaseDirectory) and then fall back to the Trips
            // project folder so Gateway does not need its own copy.
            const string payloadPath = @"Requests\Body\GraphQL\Trips\DeepLinks.json";

            JObject variables = PayloadHelper.GetVariables(payloadPath);
            if (variables["request"] is not JObject request)
            {
                throw new InvalidOperationException(
                    "Deeplink payload is missing a 'request' object under 'variables', or it is not a JSON object.");
            }

            request["tripReservationId"] = tripReservationId;
            request["deeplinkOption"] = deeplinkOption;
            request["additionalData"] = JValue.CreateNull();

            return JsonConvert.SerializeObject(variables);
        }

        public static string AccountMemberDetails(int vouchersCount, int flightsCount, int activitiesCount)
        {
            JObject payload = new ()
            {
                ["vouchersCount"] = vouchersCount,
                ["flightsCount"] = flightsCount,
                ["activitiesCount"] = activitiesCount
            };

            return JsonConvert.SerializeObject(payload);
        }

        public static string RetrieveHouseHoldAccount(string memberType, string houseHoldFcNumber)
        {
            JObject payload = new ()
            {
                ["retrieveHHAccountRequest"] = new JObject
                {
                    ["memberType"] = memberType,
                    ["houseHoldFcNumber"] = houseHoldFcNumber
                }
            };

            return JsonConvert.SerializeObject(payload);
        }
        public static string RetrieveHolidayBooking(string bookingReference, string dateOfBirth, string surname)

        {
            JObject variables = PayloadHelper.GetVariables(@"Requests\Body\GraphQL\Trips\Holidays\RetrieveHolidayBooking.json");
            if (variables["request"] is not JObject request)
            {
                throw new InvalidOperationException(
                    "RetrieveHolidayBooking payload is missing a 'request' object under 'variables', or it is not a JSON object.");
            }

            request["using"] = "HOLIDAYS";
            request["bookingReference"] = bookingReference;
            request["dateOfBirth"] = dateOfBirth;
            request["surname"] = surname;

            return JsonConvert.SerializeObject(variables);
        }

        public static string TripDetails(string bookingReference, string dateOfBirth, string bookingSurname, string pnrLocator, string givenNames, string pnrSurname)
        {
            const string payloadPath = @"Requests\Body\GraphQL\Trips\TripDetails.json";

            JObject variables = PayloadHelper.GetVariables(payloadPath);

            if (variables["request"] is not JObject request)
            {
                throw new InvalidOperationException(
                    "TripDetails payload is missing a 'request' object under 'variables', or it is not a JSON object.");
            }

            if (request["tripDetails"] is not JArray tripDetails || tripDetails.Count < 2 || tripDetails[0] is not JObject firstTrip || tripDetails[1] is not JObject secondTrip)
            {
                throw new InvalidOperationException(
                    "TripDetails payload is missing a 'tripDetails' array with at least 2 items under 'variables.request'.");
            }

            firstTrip["bookingReference"] = bookingReference;
            firstTrip["dateOfBirth"] = dateOfBirth;
            firstTrip["createdDate"] = JValue.CreateNull();
            firstTrip["givenNames"] = JValue.CreateNull();
            firstTrip["pnrLocator"] = JValue.CreateNull();
            firstTrip["surname"] = bookingSurname;

            secondTrip["bookingReference"] = JValue.CreateNull();
            secondTrip["dateOfBirth"] = JValue.CreateNull();
            secondTrip["createdDate"] = JValue.CreateNull();
            secondTrip["givenNames"] = givenNames;
            secondTrip["pnrLocator"] = pnrLocator;
            secondTrip["surname"] = pnrSurname;

            return JsonConvert.SerializeObject(variables);
        }

        public static string ServicingValidatePnrOrder(string pnr, string givenName, string lastName)
        {
            JObject payload = new()
            {
                ["request"] = new JObject
                {
                    ["pnr"] = pnr,
                    ["givenName"] = givenName,
                    ["lastName"] = lastName
                }
            };

            return JsonConvert.SerializeObject(payload);
        }

        public static string GetDeepLinkForPost(string id)
        {
            JObject payload = new()
            {
                ["id"] = id
            };

            return JsonConvert.SerializeObject(payload);
        }

        public static string LoyaltyIssueFlyingClubApplicationToken(string flyingClubNumber)
        {
            JObject payload = new()
            {
                ["request"] = new JObject
                {
                    ["flyingClubAccountNumber"] = flyingClubNumber
                }
            };

            return JsonConvert.SerializeObject(payload);
        }
    }
}

