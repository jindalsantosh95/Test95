using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class BookingTests
    {
        public string env = Config.Environment;

        [Fact]
        public async Task TC01_Booking_OneWay_ReturnsSuccess()
        {
            // Default test-data convention (see docs/TEST_DATA_STRATEGY.md): pull a current-week
            // PNR from Azure storage rather than hard-coding it. Pick the simplest reusable
            // shape — OneWay/REVENUE — that matches the booking lookup payload (using/PNR).
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "REVENUE");

            if (pnrDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string pnr = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(
                !string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname),
                "PNR, First Name, and Last Name should not be null or empty");

            // Call the Gateway booking endpoint with the fetched values.
            HttpResponseMessage response = RestEndpoint.Booking(
                env,
                usingString: "PNR",
                confirmationNum: pnr,
                givenNames: givenNames,
                surname: surname
            ).EnsureIsSuccessStatusCode();

            // Parse the body for downstream assertions — mirrors the Trips microservice
            // test pattern (see Trips/Tests/TripsTests/Microservices/TripsTests.cs).
            // JObject.Parse throws on malformed JSON and never returns null — preferred
            // over JsonConvert.DeserializeObject<JObject> which returns a nullable.
            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "Booking response body was empty");
            JObject bookingResponse = JObject.Parse(responseBody);

            // Body-level error guards. The booking lookup can return 200 OK with these
            // markers present on a failed reservation retrieval, so a body scan is the
            // right gate even after EnsureIsSuccessStatusCode.
            string bodyString = bookingResponse.ToString();
            Assert.False(bodyString.Contains("NGTMYTR400"),
                $"Unexpected error: NGTMYTR400 - Unable to retrieve reservation. Body: {bodyString}");
            Assert.False(bodyString.Contains("Unable to retrieve reservation"),
                $"Unexpected error: {bodyString}");

            // PNR locator on the returned booking must match what we asked for.
            // The Gateway REST shim hoists the GraphQL `Booking` type fields to the
            // root — no `data`/`Booking` wrapper. RequireToken (Core.Extensions) throws
            // JsonException naming the missing path if any segment resolves to null —
            // far better diagnostics than NRE-at-line-N, and no JToken? null warnings.
            string actualPnrLocator = bookingResponse
                .RequireToken("product[0].reference.pnrLocator")
                .ToString();
            // .Is() is the codebase's idiom for case-insensitive string compare —
            // OrdinalIgnoreCase under the hood, with null-safety on both sides. Used
            // anywhere PNRs / FC numbers / env codes are compared (AGENTS.md § Design
            // conventions). Don't reach for string.Equals(..., OrdinalIgnoreCase)
            // directly when .Is() already wraps it.
            Assert.True(pnr.Is(actualPnrLocator),
                $"PNR locator does not match expected value. Expected: {pnr} Actual: {actualPnrLocator}");

            // Booking type for an airline reservation should be "AIRLINE".
            string bookingType = bookingResponse
                .RequireToken("product[0].bookingType")
                .ToString();
            Assert.True(bookingType == "AIRLINE",
                $"Expected bookingType to be AIRLINE but got {bookingType}");

            Assert.Equal("REVENUE", bookingResponse
                .RequireToken("reservationType")
                .ToString(), ignoreCase: true);

            // OneWay journey → exactly one slice on the booking product.
            JToken slicesArray = bookingResponse.RequireToken("product[0].slices");
            int slicesCount = slicesArray.Count();
            Assert.True(slicesCount == 1,
                $"OneWay journey should have exactly 1 slice but found {slicesCount}");
        }

        [Fact]
        public async Task TC02_Booking_Return_ReturnsSuccess()
        {
            // Default test-data convention (see docs/TEST_DATA_STRATEGY.md): pull a current-week
            // PNR from Azure storage rather than hard-coding it. Pick the simplest reusable
            // shape — Return/REVENUE — that matches the booking lookup payload (using/PNR).
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("Return", "REVENUE");

            if (pnrDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string pnr = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(
                !string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname),
                "PNR, First Name, and Last Name should not be null or empty");

            // Call the Gateway booking endpoint with the fetched values.
            HttpResponseMessage response = RestEndpoint.Booking(
                env,
                usingString: "PNR",
                confirmationNum: pnr,
                givenNames: givenNames,
                surname: surname
            ).EnsureIsSuccessStatusCode();

            // Parse the body for downstream assertions — mirrors the Trips microservice
            // test pattern (see Trips/Tests/TripsTests/Microservices/TripsTests.cs).
            // JObject.Parse throws on malformed JSON and never returns null — preferred
            // over JsonConvert.DeserializeObject<JObject> which returns a nullable.
            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "Booking response body was empty");
            JObject bookingResponse = JObject.Parse(responseBody);

            // Body-level error guards. The booking lookup can return 200 OK with these
            // markers present on a failed reservation retrieval, so a body scan is the
            // right gate even after EnsureIsSuccessStatusCode.
            string bodyString = bookingResponse.ToString();
            Assert.False(bodyString.Contains("NGTMYTR400"),
                $"Unexpected error: NGTMYTR400 - Unable to retrieve reservation. Body: {bodyString}");
            Assert.False(bodyString.Contains("Unable to retrieve reservation"),
                $"Unexpected error: {bodyString}");

            // PNR locator on the returned booking must match what we asked for.
            string actualPnrLocator = bookingResponse
                .RequireToken("product[0].reference.pnrLocator")
                .ToString();
            Assert.True(pnr.Is(actualPnrLocator),
                $"PNR locator does not match expected value. Expected: {pnr} Actual: {actualPnrLocator}");

            // Booking type for an airline reservation should be "AIRLINE".
            string bookingType = bookingResponse
                .RequireToken("product[0].bookingType")
                .ToString();
            Assert.True(bookingType == "AIRLINE",
                $"Expected bookingType to be AIRLINE but got {bookingType}");

            Assert.Equal("REVENUE", bookingResponse
                .RequireToken("reservationType")
                .ToString(), ignoreCase: true);

            // Return journey → exactly two slices on the booking product.
            JToken slicesArray = bookingResponse.RequireToken("product[0].slices");
            int slicesCount = slicesArray.Count();
            Assert.True(slicesCount == 2,
                $"Return journey should have exactly 2 slices but found {slicesCount}");

            // Legs assertions (structure + required fields) based on the Trips/Booking schema:
            // product -> slices[*] -> segments[*] -> legs[*].
            for (int sliceIndex = 0; sliceIndex < slicesCount; sliceIndex++)
            {
                string segmentsPath = $"product[0].slices[{sliceIndex}].segments";
                JToken segmentsArray = bookingResponse.RequireToken(segmentsPath);
                int segmentsCount = segmentsArray.Count();
                Assert.True(segmentsCount > 0,
                    $"Expected at least 1 segment at {segmentsPath} but found {segmentsCount}");

                for (int segmentIndex = 0; segmentIndex < segmentsCount; segmentIndex++)
                {
                    string legsPath = $"product[0].slices[{sliceIndex}].segments[{segmentIndex}].legs";
                    JToken legsArray = bookingResponse.RequireToken(legsPath);
                    int legsCount = legsArray.Count();
                    Assert.True(legsCount > 0,
                        $"Expected at least 1 leg at {legsPath} but found {legsCount}");

                    for (int legIndex = 0; legIndex < legsCount; legIndex++)
                    {
                        string legBasePath = $"{legsPath}[{legIndex}]";

                        string legId = bookingResponse.RequireToken($"{legBasePath}.id").ToString();
                        Assert.False(string.IsNullOrWhiteSpace(legId),
                            $"{legBasePath}.id should be populated");
                    }
                }
            }
             // Passenger assertions based on the Gateway booking schema:
            // passengers[*] -> slices[*] -> segments[*] -> legs[*].
            JToken passengersArray = bookingResponse.RequireToken("passengers");
            int passengersCount = passengersArray.Count();
            Assert.True(passengersCount > 0,
                $"Expected at least 1 passenger at passengers but found {passengersCount}");

            HashSet<string> passengerTypeCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> passengerIdToTypeCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            List<string> associatedInfantIds = new List<string>();

            for (int passengerIndex = 0; passengerIndex < passengersCount; passengerIndex++)
            {
                string passengerBasePath = $"passengers[{passengerIndex}]";

                string customerId = bookingResponse.RequireToken($"{passengerBasePath}.customerId").ToString();
                Assert.False(string.IsNullOrWhiteSpace(customerId),
                    $"{passengerBasePath}.customerId should be populated");

                string passengerId = bookingResponse.RequireToken($"{passengerBasePath}.passengerId").ToString();
                Assert.False(string.IsNullOrWhiteSpace(passengerId),
                    $"{passengerBasePath}.passengerId should be populated");

                string passengerTypeCode = bookingResponse.RequireToken($"{passengerBasePath}.passengerType.code").ToString();
                Assert.False(string.IsNullOrWhiteSpace(passengerTypeCode),
                    $"{passengerBasePath}.passengerType.code should be populated");
                passengerTypeCodes.Add(passengerTypeCode);

                if (passengerIdToTypeCode.ContainsKey(passengerId))
                {
                    Assert.Fail($"Duplicate passengerId found at {passengerBasePath}.passengerId: {passengerId}");
                }
                passengerIdToTypeCode.Add(passengerId, passengerTypeCode);

                string passengerTypeName = bookingResponse.RequireToken($"{passengerBasePath}.passengerType.name").ToString();
                Assert.False(string.IsNullOrWhiteSpace(passengerTypeName),
                    $"{passengerBasePath}.passengerType.name should be populated");

                 // associatedInfantId is present only for some passenger types (example: ADT with INF).
                JToken? associatedInfantToken = bookingResponse.SelectToken($"{passengerBasePath}.associatedInfantId");
                if (associatedInfantToken != null && associatedInfantToken.Type != JTokenType.Null)
                {
                    string associatedInfantId = associatedInfantToken.ToString();
                    Assert.False(string.IsNullOrWhiteSpace(associatedInfantId),
                        $"{passengerBasePath}.associatedInfantId should be populated when present");
                    associatedInfantIds.Add(associatedInfantId);
                }

                JToken passengerSlicesArray = bookingResponse.RequireToken($"{passengerBasePath}.slices");
                int passengerSlicesCount = passengerSlicesArray.Count();
                Assert.True(passengerSlicesCount > 0,
                    $"Expected at least 1 slice at {passengerBasePath}.slices but found {passengerSlicesCount}");

                for (int passengerSliceIndex = 0; passengerSliceIndex < passengerSlicesCount; passengerSliceIndex++)
                {
                    string passengerSliceBasePath = $"{passengerBasePath}.slices[{passengerSliceIndex}]";

                    string passengerSliceId = bookingResponse.RequireToken($"{passengerSliceBasePath}.id").ToString();
                    Assert.False(string.IsNullOrWhiteSpace(passengerSliceId),
                        $"{passengerSliceBasePath}.id should be populated");

                    string passengerSliceCheckinStatus = bookingResponse.RequireToken($"{passengerSliceBasePath}.checkinStatus").ToString();
                    Assert.False(string.IsNullOrWhiteSpace(passengerSliceCheckinStatus),
                        $"{passengerSliceBasePath}.checkinStatus should be populated");

                    string passengerSegmentsPath = $"{passengerSliceBasePath}.segments";
                    JToken passengerSegmentsArray = bookingResponse.RequireToken(passengerSegmentsPath);
                    int passengerSegmentsCount = passengerSegmentsArray.Count();
                    Assert.True(passengerSegmentsCount > 0,
                        $"Expected at least 1 segment at {passengerSegmentsPath} but found {passengerSegmentsCount}");

                    for (int passengerSegmentIndex = 0; passengerSegmentIndex < passengerSegmentsCount; passengerSegmentIndex++)
                    {
                        string passengerSegmentBasePath = $"{passengerSegmentsPath}[{passengerSegmentIndex}]";

                        string passengerSegmentId = bookingResponse.RequireToken($"{passengerSegmentBasePath}.id").ToString();
                        Assert.False(string.IsNullOrWhiteSpace(passengerSegmentId),
                            $"{passengerSegmentBasePath}.id should be populated");

                        string passengerSegmentCheckinStatus = bookingResponse.RequireToken($"{passengerSegmentBasePath}.checkinStatus").ToString();
                        Assert.False(string.IsNullOrWhiteSpace(passengerSegmentCheckinStatus),
                            $"{passengerSegmentBasePath}.checkinStatus should be populated");

                        JToken passengerSeatEligibleToken = bookingResponse.RequireToken($"{passengerSegmentBasePath}.seatEligible");
                        Assert.True(passengerSeatEligibleToken.Type == JTokenType.Boolean,
                            $"{passengerSegmentBasePath}.seatEligible should be a boolean");

                        string passengerLegsPath = $"{passengerSegmentBasePath}.legs";
                        JToken passengerLegsArray = bookingResponse.RequireToken(passengerLegsPath);
                        int passengerLegsCount = passengerLegsArray.Count();
                        Assert.True(passengerLegsCount > 0,
                            $"Expected at least 1 leg at {passengerLegsPath} but found {passengerLegsCount}");

                        for (int passengerLegIndex = 0; passengerLegIndex < passengerLegsCount; passengerLegIndex++)
                        {
                            string passengerLegBasePath = $"{passengerLegsPath}[{passengerLegIndex}]";

                            string passengerLegId = bookingResponse.RequireToken($"{passengerLegBasePath}.id").ToString();
                            Assert.False(string.IsNullOrWhiteSpace(passengerLegId),
                                $"{passengerLegBasePath}.id should be populated");
                        }
                    }
                }
            }

            Assert.True(passengerTypeCodes.Contains("ADT"),
                $"Expected at least one ADT passengerType.code but got [{string.Join(", ", passengerTypeCodes)}]");

            if (associatedInfantIds.Count > 0)
            {
                Assert.True(passengerTypeCodes.Contains("INF"),
                    $"Found associatedInfantId but no INF passengerType.code was present. Codes: [{string.Join(", ", passengerTypeCodes)}]");
            }

            foreach (string associatedInfantId in associatedInfantIds)
            {
                Assert.True(passengerIdToTypeCode.TryGetValue(associatedInfantId, out string? infantTypeCode),
                    $"associatedInfantId {associatedInfantId} did not match any passengers[*].passengerId");
                Assert.True("INF".Is(infantTypeCode),
                    $"associatedInfantId {associatedInfantId} should reference an INF passenger but referenced {infantTypeCode}");
            }
        }

        [Fact]
        public async Task TC03_Booking_AwardOneWay_ReturnsAwardReservationType()
        {
            Dictionary<string, string> pnrDetails =
                RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek("OneWay", "AWARD");

            if (pnrDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            HttpResponseMessage response = RestEndpoint.Booking(
                env,
                usingString: "PNR",
                confirmationNum: pnrDetails["pnr"],
                givenNames: pnrDetails["firstName"],
                surname: pnrDetails["lastName"]
            ).EnsureIsSuccessStatusCode();

            JObject bookingResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False(bookingResponse.ToString().Contains("Unable to retrieve reservation"),
                $"Unexpected booking retrieval error. Body: {bookingResponse}");
            Assert.Equal("AWARD", bookingResponse
                .RequireToken("reservationType")
                .ToString(), ignoreCase: true);
        }
    }
}
