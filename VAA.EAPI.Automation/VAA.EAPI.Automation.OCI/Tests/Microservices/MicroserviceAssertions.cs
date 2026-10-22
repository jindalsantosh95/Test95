using System.Text.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.OCI.Tests.Microservices
{
    public static class MicroserviceAssertions
    {
        public static bool ValidateSelectedPassengerFieldsNotNullOrEmpty(string selectPassengersRequest, JToken travelPartiesResult, out string failureMessage)
        {
            failureMessage = string.Empty;

            if (selectPassengersRequest.IsNullOrWhiteSpace())
            {
                failureMessage = "selectPassengersRequest is null/empty";
                return false;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(selectPassengersRequest);

                if (!document.RootElement.TryGetProperty("selectedPassenger", out JsonElement selectedPassengers))
                {
                    failureMessage = $"selectedPassenger is missing in selectPassengersRequest.\n{travelPartiesResult}";
                    return false;
                }

                if (selectedPassengers.ValueKind != JsonValueKind.Array)
                {
                    failureMessage = $"selectedPassenger is not an array in selectPassengersRequest.\n{travelPartiesResult}";
                    return false;
                }

                JsonElement[] selectedPassengerArray = selectedPassengers.EnumerateArray().ToArray();

                if (selectedPassengerArray.Length == 0)
                {
                    failureMessage = $"selectedPassenger is empty in selectPassengersRequest.\n{selectPassengersRequest}";
                    return false;
                }

                bool allSelectedPassengersValid = selectedPassengerArray.All(p =>
                    p.TryGetProperty("id", out JsonElement id) &&
                    !(id.GetString() ?? string.Empty).IsNullOrWhiteSpace() &&
                    p.TryGetProperty("customerId", out JsonElement customerId) &&
                    !(customerId.GetString() ?? string.Empty).IsNullOrWhiteSpace() &&
                    p.TryGetProperty("firstNIN", out JsonElement firstNIN) &&
                    !(firstNIN.GetString() ?? string.Empty).IsNullOrWhiteSpace() &&
                    p.TryGetProperty("lastNIN", out JsonElement lastNIN) &&
                    !(lastNIN.GetString() ?? string.Empty).IsNullOrWhiteSpace()
                );

                if (!allSelectedPassengersValid)
                {
                    failureMessage = $"One or more selectedPassenger fields are null or empty in travel parties response.\n{travelPartiesResult}";
                    return false;
                }

                return true;
            }
            catch (JsonException jsonException)
            {
                failureMessage = $"selectPassengersRequest is not valid JSON: {jsonException.Message}\n{travelPartiesResult}";
                return false;
            }
        }

        public static bool ValidateTravelRequirementsResponse(JToken json, string expectedPnrLocator, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (json is null)
            {
                result.Message.Add("Response JSON is null");
                return result.IsValid;
            }

            string requirementsId = json.SelectToken("$.requirementsId")?.ToString();
            if (requirementsId.IsNullOrWhiteSpace())
            {
                result.Message.Add("requirementsId is missing/empty");
                return result.IsValid;
            }

            string responsePnr = json.SelectToken("$.reference.pnrLocator")?.ToString();
            if (!string.Equals(expectedPnrLocator, responsePnr, StringComparison.OrdinalIgnoreCase))
            {
                result.Message.Add($"reference.pnrLocator mismatch. Expected: {expectedPnrLocator}, Actual: {responsePnr}");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool ValidateSlicesAndPassengerSliceRequirements(JToken json, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (json is null)
            {
                result.Message.Add("Response JSON is null");
                return result.IsValid;
            }

            JArray slices = json.SelectToken("$.slices") as JArray;
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
                .Where(id => !(id ?? string.Empty).IsNullOrWhiteSpace())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (sliceIds.Count == 0)
            {
                result.Message.Add("No sliceId values returned in slices");
                return result.IsValid;
            }

            JArray passengers = json.SelectToken("$.passengers") as JArray;
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
                string passengerId = passenger?["passengerId"]?.ToString();
                string passengerGivenNames = passenger?["givenNames"]?.ToString();
                string passengerSurname = passenger?["surname"]?.ToString();

                if (passengerId.IsNullOrWhiteSpace())
                {
                    result.Message.Add("passengers[].passengerId is missing/empty");
                    return result.IsValid;
                }

                if (passengerGivenNames.IsNullOrWhiteSpace())
                {
                    result.Message.Add($"givenNames missing/empty for passengerId: {passengerId}");
                    return result.IsValid;
                }

                if (passengerSurname.IsNullOrWhiteSpace())
                {
                    result.Message.Add($"surname missing/empty for passengerId: {passengerId}");
                    return result.IsValid;
                }

                JArray sliceRequirements = passenger?["sliceRequirements"] as JArray;
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
                    string srSliceId = sr?["sliceId"]?.ToString();
                    string overall = sr?["overall"]?.ToString();

                    if (srSliceId.IsNullOrWhiteSpace())
                    {
                        result.Message.Add($"sliceRequirements[].sliceId missing/empty for passengerId: {passengerId}");
                        return result.IsValid;
                    }

                    if (!sliceIds.Contains(srSliceId))
                    {
                        result.Message.Add($"sliceRequirements[].sliceId '{srSliceId}' not found in slices[] for passengerId: {passengerId}");
                        return result.IsValid;
                    }

                    if (overall.IsNullOrWhiteSpace())
                    {
                        result.Message.Add($"sliceRequirements[].overall missing/empty for passengerId: {passengerId}");
                        return result.IsValid;
                    }
                }
            }

            return result.IsValid;
        }
    }
}
