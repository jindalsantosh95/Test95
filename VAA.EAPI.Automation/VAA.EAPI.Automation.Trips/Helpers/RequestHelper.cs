using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Trips.Helpers
{
    public static class RequestHelper
    {
        public static string FormatTripsBookingContext(string confirmationNum, string givenNames, string surname)
        {
            string safeConfirmationNum = string.IsNullOrWhiteSpace(confirmationNum) ? "<empty>" : confirmationNum;
            string safeGivenNames = string.IsNullOrWhiteSpace(givenNames) ? "<empty>" : givenNames;
            string safeSurname = string.IsNullOrWhiteSpace(surname) ? "<empty>" : surname;

            return $"[TripsBooking] confirmationNum='{safeConfirmationNum}', givenNames='{safeGivenNames}', surname='{safeSurname}'";
        }

        public static string TripsSearchRequest(string @using, string pnr, string givenNames, string surname)
        {
            JObject tripsSearchRequest = new()
            {
                ["using"] = @using,
                ["confirmationNum"] = pnr,
                ["givenNames"] = givenNames,
                ["surname"] = surname,
            };

            return JsonConvert.SerializeObject(tripsSearchRequest);
        }
        public static string DeeplinkRequest(string tripReservationId, string deeplinkOption, string posCountryCode)
        {
            JObject deeplinkRequest = new()
            {
                ["tripReservationId"] = tripReservationId,
                ["deeplinkOption"] = deeplinkOption,
                ["posCountryCode"] = posCountryCode,
            };

            return JsonConvert.SerializeObject(deeplinkRequest);
        }
        public static string PlusGradeAPIRequest(string pnr, string lastname, string apiKey)
        {
            JObject plusGradeRequest = new()
            {
                ["pnr"] = pnr,
                ["lastname"] = lastname,
                ["apiKey"] = apiKey,
            };

            return JsonConvert.SerializeObject(plusGradeRequest);
        }

    }
}
