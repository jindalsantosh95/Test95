using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL
{
    public class GraphQLAssertions
    {
        public static bool ValidateTravelRequirementsResponse(JToken graphQLResponseJson, string expectedPnrLocator, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (graphQLResponseJson is null)
            {
                result.Message.Add("Response JSON is null");
                return result.IsValid;
            }

            JToken? travelRequirements = graphQLResponseJson.SelectToken("$.data.GetTravelRequirements");
            if (travelRequirements is null)
            {
                result.Message.Add("data.GetTravelRequirements is missing");
                return result.IsValid;
            }

            string? travelRequirementsId = travelRequirements.SelectToken("$.travelRequirementsId")?.ToString();
            if (string.IsNullOrWhiteSpace(travelRequirementsId))
            {
                result.Message.Add("travelRequirementsId is missing/empty");
                return result.IsValid;
            }

            string? responsePnr = travelRequirements.SelectToken("$.reference.pnrLocator")?.ToString();
            if (!string.Equals(expectedPnrLocator, responsePnr, StringComparison.OrdinalIgnoreCase))
            {
                result.Message.Add($"reference.pnrLocator mismatch. Expected: {expectedPnrLocator}, Actual: {responsePnr}");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool ValidateSlicesAndPassengerSliceRequirements(JToken graphQLResponseJson, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (graphQLResponseJson is null)
            {
                result.Message.Add("Response JSON is null");
                return result.IsValid;
            }

            JToken? travelRequirements = graphQLResponseJson.SelectToken("$.data.GetTravelRequirements");
            if (travelRequirements is null)
            {
                result.Message.Add("data.GetTravelRequirements is missing");
                return result.IsValid;
            }

            JArray? slices = travelRequirements.SelectToken("$.slices") as JArray;
            if (slices is null)
            {
                result.Message.Add("slices is missing");
                return result.IsValid;
            }

            if (slices.Count == 0)
            {
                result.Message.Add("slices is empty");
                return result.IsValid;
            }

            HashSet<string> sliceIds = slices
                .Select(s => s?["sliceId"]?.ToString())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (sliceIds.Count == 0)
            {
                result.Message.Add("No sliceId values returned in slices");
                return result.IsValid;
            }

            JArray? passengers = travelRequirements.SelectToken("$.passengers") as JArray;
            if (passengers is null)
            {
                result.Message.Add("passengers is missing");
                return result.IsValid;
            }

            if (passengers.Count == 0)
            {
                result.Message.Add("passengers is empty");
                return result.IsValid;
            }

            foreach (JToken passenger in passengers)
            {
                string? passengerId = passenger?["passengerId"]?.ToString();
                string? passengerGivenNames = passenger?["givenNames"]?.ToString();
                string? passengerSurname = passenger?["surname"]?.ToString();

                if (string.IsNullOrWhiteSpace(passengerId))
                {
                    result.Message.Add("passengers[].passengerId is missing/empty");
                    return result.IsValid;
                }

                if (string.IsNullOrWhiteSpace(passengerGivenNames))
                {
                    result.Message.Add($"givenNames missing/empty for passengerId: {passengerId}");
                    return result.IsValid;
                }

                if (string.IsNullOrWhiteSpace(passengerSurname))
                {
                    result.Message.Add($"surname missing/empty for passengerId: {passengerId}");
                    return result.IsValid;
                }

                JArray? sliceRequirements = passenger?["sliceRequirements"] as JArray;
                if (sliceRequirements is null)
                {
                    result.Message.Add($"sliceRequirements is missing for passengerId: {passengerId}");
                    return result.IsValid;
                }

                if (sliceRequirements.Count == 0)
                {
                    result.Message.Add($"sliceRequirements is empty for passengerId: {passengerId}");
                    return result.IsValid;
                }

                foreach (JToken sr in sliceRequirements)
                {
                    string? srSliceId = sr?["sliceId"]?.ToString();
                    string? overall = sr?["overall"]?.ToString();

                    if (string.IsNullOrWhiteSpace(srSliceId))
                    {
                        result.Message.Add($"sliceRequirements[].sliceId missing/empty for passengerId: {passengerId}");
                        return result.IsValid;
                    }

                    if (!sliceIds.Contains(srSliceId))
                    {
                        result.Message.Add($"sliceRequirements[].sliceId '{srSliceId}' not found in slices[] for passengerId: {passengerId}");
                        return result.IsValid;
                    }

                    if (string.IsNullOrWhiteSpace(overall))
                    {
                        result.Message.Add($"sliceRequirements[].overall missing/empty for passengerId: {passengerId}");
                        return result.IsValid;
                    }
                }
            }

            return result.IsValid;
        }

        public static bool CheckAirshipLinksExists(JToken generateBoardingPassResult)
        {
            IEnumerable<JToken> boardingPasses = generateBoardingPassResult.SelectTokens("$..boardingPasses[*]");

            return boardingPasses.Any() &&
                   boardingPasses.All(bp =>
                       !string.IsNullOrWhiteSpace(bp["androidBoardingPassWalletLink"]?.ToString()) &&
                       !string.IsNullOrWhiteSpace(bp["iosBoardingPassWalletLink"]?.ToString()));
        }

        public static bool ValidateShowSkyPriority(JToken generateBoardingPassResult)
        {
            // Select all boarding passes
            List<JToken> boardingPasses = generateBoardingPassResult.SelectTokens("$..boardingPasses[*]").ToList();

            foreach (JToken boardingPass in boardingPasses)
            {
                string templateCode = boardingPass["templateCode"]?.ToString();

                bool showSkyPriority = boardingPass["showSkyPriority"]?.ToObject<bool>() ?? false;

                // Rule: If templateCode is VS_PREMIUM → showSkyPriority must be true
                if (templateCode == "VS_PREMIUM" && showSkyPriority != true)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidatePaymentEligibilityCheckResponse(HttpResponseMessage jsonResponse, BaggageSelectionType baggageSelectionType)
        {
            JToken paymentEligibilityCheckJson = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult())!;

            JToken? paymentEligibility = paymentEligibilityCheckJson["data"]?["PaymentEligibilityCheck"];

            if (paymentEligibility is null)
                return false;

            string? checkInId = paymentEligibility.Value<string>("checkInId");

            bool paymentPending = paymentEligibility.Value<bool?>("paymentPending") ?? false;

            string? paymentUrl = paymentEligibility.Value<string>("paymentUrl");

            string? hPPLink = paymentEligibility.Value<string>("hPPLink");

            string? paymentReferenceId = paymentEligibility.Value<string>("paymentReferenceId");

            JToken? amount = paymentEligibility["amount"];

            string? currencyCode = amount?.Value<string>("currency");

            decimal totalAmount = amount?.Value<decimal?>("totalAmount") ?? 0m;

            bool isFreeBaggageResponseValid = !paymentPending && paymentUrl is null && hPPLink is null && paymentReferenceId is null && currencyCode is null && totalAmount == 0m;

            bool isPaidBaggageResponseValid = paymentPending && !string.IsNullOrEmpty(paymentUrl) && !string.IsNullOrEmpty(hPPLink) && !string.IsNullOrEmpty(paymentReferenceId) && !string.IsNullOrEmpty(currencyCode) && totalAmount > 0m;

            return baggageSelectionType == BaggageSelectionType.FreeOnly ? isFreeBaggageResponseValid : isPaidBaggageResponseValid;
        }

        public static bool CheckBaggageAdvisoryMessageExistsForEachPassenger(JToken getBaggageResult)
        {
            if (getBaggageResult == null)
                return false;

            IEnumerable<JToken> passengers = getBaggageResult.SelectTokens("$.data.BaggageAllowance.passengers[*]");

            if (!passengers.Any())
                return false;

            return passengers.All(p => !string.IsNullOrEmpty(p["baggageAdvisory"]?["message"]?.Value<string>()));
        }

        public static bool ValidateShowLoyaltyStatusFirstPassenger(JToken retrieveBoardingPassResult)
        {
            JToken? firstPassenger = retrieveBoardingPassResult?["data"]?["GetBoardingPass"]?["boardingPasses"]?.FirstOrDefault();

            return firstPassenger != null && !string.IsNullOrWhiteSpace(firstPassenger["tierCode"]?.ToString())
                   && !string.IsNullOrWhiteSpace(firstPassenger["tierName"]?.ToString());
        }

        public static bool ValidateQrCodes(JToken boardingPassResult)
        {
            JToken? boardingPasses = boardingPassResult.SelectToken("data.*.boardingPasses");

            return boardingPasses?.All(bp => !string.IsNullOrWhiteSpace(bp["qrCode"]?.ToString())) == true;
        }

        public static bool ValidateDesktopBoardingPassBase64(JToken boardingPassResult)
        {
            if (boardingPassResult is null)
            {
                return false;
            }

            IEnumerable<JToken> boardingPasses = boardingPassResult.SelectTokens("$.data.*.boardingPasses[*]");

            return boardingPasses.Any()
                && boardingPasses.All(boardingPass => HasDesktopBoardingPassBase64(boardingPass["desktopBoardingPassBase64"]));
        }

        private static bool HasDesktopBoardingPassBase64(JToken? desktopBoardingPassBase64)
        {
            if (desktopBoardingPassBase64 is JArray desktopBoardingPassBase64Array)
            {
                return desktopBoardingPassBase64Array.Count > 0
                    && desktopBoardingPassBase64Array.All(entry =>
                        entry.Type == JTokenType.String && !entry.ToString().IsNullOrWhiteSpace());
            }

            return desktopBoardingPassBase64?.Type == JTokenType.String
                && !desktopBoardingPassBase64.ToString().IsNullOrWhiteSpace();
        }

        public static bool ValidateTotalPriceAndCurrencyForSelectedBaggage(string saveBaggageRequest, HttpResponseMessage confirmBaggageResponse)
        {
            JObject requestJson = JObject.Parse(saveBaggageRequest);

            JArray offers = requestJson["variables"]?["request"]?["selectedBaggageOffer"] as JArray;

            if (offers is null || offers.Count == 0)
                return false;

            decimal expectedTotalPrice = offers.Sum(o => o["totalPrice"]?.Value<decimal?>() ?? 0m);

            string expectedCurrency = offers.FirstOrDefault()?.Value<string>("currencyCode");

            if (string.IsNullOrWhiteSpace(expectedCurrency))
                return false;

            string responseContent = confirmBaggageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            JToken? confirmBaggage = JObject.Parse(responseContent)["data"]?["ConfirmBaggage"];

            if (confirmBaggage == null)
            {
                return false;
            }

            decimal actualTotalPrice = confirmBaggage.Value<decimal>("totalPaidAmount");

            string actualCurrency = confirmBaggage.Value<string>("currencyCode");

            return actualTotalPrice == expectedTotalPrice && string.Equals(actualCurrency, expectedCurrency, StringComparison.OrdinalIgnoreCase);
        }

        public static bool DoAllPassengersHaveEmergencyContact(JToken response)
        {
            JArray? passengers = response["data"]?["SaveEmergencyContactTravelRequirements"]?["passengers"] as JArray;

            if (passengers == null || !passengers.Any())
                return false;

            return passengers.All(passenger => passenger["emergencyContact"] != null &&
                passenger["emergencyContact"]!.Type != JTokenType.Null);
        }
    }
}
