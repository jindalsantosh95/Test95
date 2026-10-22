using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    [Collection("Sequential")]
    public class StoreManagerReplaceSlotRulesTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerReplaceSlotRulesTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [SkippableTheory]
        [MemberData(nameof(GetStores))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreMutations)]
        public async Task TC01_StoreManagerReplaceSlotRules(string storeId)
        {
            Console.WriteLine($"Running StoreManagerReplaceSlotRules for store {storeId}");

            string bearer = _tokenFixture.GetToken("GraphToken");

            SoftAssertHelper softAssert = new();

            bool rulesUpdated = false;

            // Step 1: Capture the original active slot rules
            List<StoreManagerSlotRulesHelper> originalRules = ExploreStoreMasterDBHelper.GetActiveSlotRulesByStoreId(storeId);

            Assert.NotEmpty(originalRules);

            // Step 2: Capture the original StoreSlot records
            List<StoreSlotRecord> originalSlots = ExploreStoreMasterDBHelper.GetStoreSlotsByStoreId(storeId);

            Assert.NotEmpty(originalSlots);

            // Step 3: Fetch opening hours before changing slot rules
            HttpResponseMessage openingHoursResponse = GraphQL.StoreManagerOpeningHours(storeId, bearer).EnsureGQLResponseSuccess();

            string openingHoursBody = await openingHoursResponse.Content.ReadAsStringAsync();

            StoreManagerOpeningHoursResponse? openingHoursResult = JsonConvert.DeserializeObject<StoreManagerOpeningHoursResponse>(openingHoursBody);

            Assert.NotNull(openingHoursResult);

            Assert.NotNull(openingHoursResult.data);

            List<StoreManagerOpeningHour> openingHours = openingHoursResult.data.storeManagerOpeningHours?.ToList() ?? new List<StoreManagerOpeningHour>();

            Assert.NotEmpty(openingHours);

            List<string> invalidOriginalRules = FindInvalidOriginalRules(originalRules, openingHours);

            Skip.If(invalidOriginalRules.Count > 0, $"Test skipped for store {storeId} because the original slot rules cannot be safely restored: {string.Join("; ", invalidOriginalRules)}.");

            // Step 4: Build updated rules only from existing active rules.
            SlotRuleReplaceRequestInput updatedRequest = BuildUpdatedRequest(storeId, originalRules, openingHours, softAssert);

            softAssert.AssertAll();

            Assert.NotEmpty(updatedRequest.rules);

            // Step 5: Validate the existing slot data against each rule time window
            ValidateExistingSlotData(storeId, originalRules, originalSlots, softAssert);

            try
            {
                // Step 6: Replace the existing slot rules.
                HttpResponseMessage mutationResponse = GraphQL.StoreManagerReplaceSlotRules(storeId, updatedRequest, bearer);

                string mutationBody = await mutationResponse.Content.ReadAsStringAsync();

                if (mutationBody.Contains("ACTIVE_BOOKINGS_EXIST", StringComparison.OrdinalIgnoreCase))
                {
                    Skip.If(true, $"Test skipped for store {storeId} because active bookings exist.");
                }

                mutationResponse.EnsureGQLResponseSuccess();

                rulesUpdated = true;

                StoreManagerReplaceSlotRulesResponse? mutationResult = JsonConvert.DeserializeObject<StoreManagerReplaceSlotRulesResponse>(mutationBody);

                Assert.NotNull(mutationResult);

                Assert.NotNull(mutationResult.data);

                List<StoreManagerReplaceSlotRule> mutationRules = mutationResult.data.storeManagerReplaceSlotRules?.ToList() ?? new List<StoreManagerReplaceSlotRule>();

                // Step 7: Validate the mutation response.
                ValidateMutationResponse(updatedRequest.rules, mutationRules, storeId, softAssert);

                // Step 8: Wait until the exact updated rules are visible through the query API.
                List<StoreManagerSlotRule> persistedRules = await WaitForApiRules(storeId, bearer, updatedRequest.rules);

                ValidateApiRules(updatedRequest.rules, persistedRules, storeId, softAssert);

                ValidateEvenSlotDivision(persistedRules, storeId, softAssert);

                // Step 9: Validate generated StoreSlot records.
                List<StoreSlotRecord> generatedSlots = ExploreStoreMasterDBHelper.GetStoreSlotsByStoreId(storeId);

                ValidateGeneratedSlots(storeId, updatedRequest.rules, generatedSlots, softAssert);

                softAssert.AssertAll();
            }
            finally
            {
                if (rulesUpdated)
                {
                    // Step 10: Build the request containing the exact original active rules.
                    SlotRuleReplaceRequestInput restoreRequest = new()
                    {
                        rules = originalRules.Select(rule => new SlotRuleReplace
                        {
                            dayOfWeek = rule.DayOfWeek,

                            startTime = FormatRuleTime(rule.StartTime),

                            endTime = FormatRuleTime(rule.EndTime),

                            slotDurationMinutes = rule.SlotDurationMinutes,

                            slotCapacity = rule.SlotCapacity
                        }).ToList()
                    };

                    try
                    {
                        // Step 11: Restore the original slot rules.
                        HttpResponseMessage restoreResponse = GraphQL.StoreManagerReplaceSlotRules(storeId, restoreRequest, bearer);

                        string restoreBody = await restoreResponse.Content.ReadAsStringAsync();

                        if (restoreBody.Contains("ACTIVE_BOOKINGS_EXIST", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException($"Original slot rules could not be restored for store {storeId} because active bookings exist.");
                        }

                        restoreResponse.EnsureGQLResponseSuccess();

                        // Step 12: Wait until the exact restored rules are visible through the query API.
                        List<StoreManagerSlotRule> restoredRules = await WaitForApiRules(storeId, bearer, restoreRequest.rules);

                        Console.WriteLine($"Restored API rules for store {storeId}: {string.Join("; ", restoredRules.Select(rule => $"Day={rule.dayOfWeek}, Start={rule.startTime}, End={rule.endTime}, Duration={rule.slotDurationMinutes}, Capacity={rule.slotCapacity}"))}");

                        SoftAssertHelper restoreAssert = new();

                        // Step 13: Validate that the original rules were restored.
                        ValidateApiRules(restoreRequest.rules, restoredRules, storeId, restoreAssert);

                        // Step 14: Validate generated slots after restoration.
                        List<StoreSlotRecord> restoredSlots = ExploreStoreMasterDBHelper.GetStoreSlotsByStoreId(storeId);

                        ValidateGeneratedSlots(storeId, restoreRequest.rules, restoredSlots, restoreAssert);

                        restoreAssert.AssertAll();
                    }
                    catch (Exception restoreException)
                    {
                        throw new InvalidOperationException($"Restoration failed for store {storeId}: {restoreException.Message}", restoreException);
                    }
                }
            }
        }

        private static List<string> FindInvalidOriginalRules(List<StoreManagerSlotRulesHelper> originalRules, List<StoreManagerOpeningHour> openingHours)
        {
            List<string> invalidRules = new();

            foreach (StoreManagerSlotRulesHelper rule in originalRules)
            {
                StoreManagerOpeningHour? openingHour = openingHours.FirstOrDefault(hour => hour.dayOfWeek == rule.DayOfWeek);

                if (openingHour == null || openingHour.opensAt.Is("closed") || openingHour.closesAt.Is("closed"))
                {
                    invalidRules.Add($"day {rule.DayOfWeek} has no valid opening-hours window");

                    continue;
                }

                bool validRuleStart = TimeSpan.TryParse(rule.StartTime, out TimeSpan ruleStart);

                bool validRuleEnd = TimeSpan.TryParse(rule.EndTime, out TimeSpan ruleEnd);

                bool validOpeningStart = TimeSpan.TryParse(openingHour.opensAt, out TimeSpan openingStart);

                bool validOpeningEnd = TimeSpan.TryParse(openingHour.closesAt, out TimeSpan openingEnd);

                if (!validRuleStart || !validRuleEnd || !validOpeningStart || !validOpeningEnd)
                {
                    invalidRules.Add($"day {rule.DayOfWeek} contains invalid time data");

                    continue;
                }

                if (rule.SlotDurationMinutes <= 0)
                {
                    invalidRules.Add($"day {rule.DayOfWeek} has invalid slot duration {rule.SlotDurationMinutes}");

                    continue;
                }

                int ruleWindowMinutes = (int)(ruleEnd - ruleStart).TotalMinutes;

                if (ruleWindowMinutes <= 0 || ruleWindowMinutes % rule.SlotDurationMinutes != 0)
                {
                    invalidRules.Add($"day {rule.DayOfWeek} has non-divisible rule {rule.StartTime}-{rule.EndTime} with duration {rule.SlotDurationMinutes}");

                    continue;
                }

                if (ruleStart < openingStart || ruleEnd > openingEnd)
                {
                    invalidRules.Add($"day {rule.DayOfWeek} rule is outside opening hours");

                    continue;
                }
            }

            return invalidRules;
        }

        private static SlotRuleReplaceRequestInput BuildUpdatedRequest(string storeId, List<StoreManagerSlotRulesHelper> originalRules, List<StoreManagerOpeningHour> openingHours, SoftAssertHelper softAssert)
        {
            List<SlotRuleReplace> updatedRules = new();

            foreach (StoreManagerSlotRulesHelper originalRule in originalRules)
            {
                StoreManagerOpeningHour? openingHour = openingHours.FirstOrDefault(hour => hour.dayOfWeek == originalRule.DayOfWeek);

                softAssert.True(openingHour != null, $"Opening hours are missing for day {originalRule.DayOfWeek} in store {storeId}.");

                if (openingHour == null || openingHour.opensAt.Is("closed") || openingHour.closesAt.Is("closed"))
                {
                    softAssert.True(false, $"Existing active rule belongs to a closed day for store {storeId}, day {originalRule.DayOfWeek}.");

                    continue;
                }

                bool validOpeningTime = TimeSpan.TryParse(openingHour.opensAt, out TimeSpan openingTime);

                bool validClosingTime = TimeSpan.TryParse(openingHour.closesAt, out TimeSpan closingTime);

                bool validRuleStart = TimeSpan.TryParse(originalRule.StartTime, out TimeSpan originalStart);

                bool validRuleEnd = TimeSpan.TryParse(originalRule.EndTime, out TimeSpan originalEnd);

                softAssert.True(validOpeningTime && validClosingTime && validRuleStart && validRuleEnd, $"Invalid time data for store {storeId}, day {originalRule.DayOfWeek}.");

                if (!validOpeningTime || !validClosingTime || !validRuleStart || !validRuleEnd)
                {
                    continue;
                }

                int slotDurationMinutes = originalRule.SlotDurationMinutes;

                softAssert.True(slotDurationMinutes > 0, $"Invalid slot duration for store {storeId}, day {originalRule.DayOfWeek}.");

                if (slotDurationMinutes <= 0 || closingTime <= openingTime)
                {
                    continue;
                }

                int originalWindowMinutes = (int)(originalEnd - originalStart).TotalMinutes;

                softAssert.True(originalWindowMinutes > 0 && originalWindowMinutes % slotDurationMinutes == 0, $"Original rule is not evenly divisible for store {storeId}, day {originalRule.DayOfWeek}.");

                if (originalWindowMinutes <= 0 || originalWindowMinutes % slotDurationMinutes != 0)
                {
                    continue;
                }

                (TimeSpan Start, TimeSpan End)? selectedCandidate = FindChangedWindow(originalRule, originalRules, openingTime, closingTime);

                Skip.If(!selectedCandidate.HasValue, $"Test skipped for store {storeId}, day {originalRule.DayOfWeek} because no valid replacement window exists.");

                if (!selectedCandidate.HasValue)
                {
                    continue;
                }

                TimeSpan newStart = selectedCandidate.Value.Start;

                TimeSpan newEnd = selectedCandidate.Value.End;

                int newWindowMinutes = (int)(newEnd - newStart).TotalMinutes;

                softAssert.True(newWindowMinutes > 0 && newWindowMinutes % slotDurationMinutes == 0, $"Generated replacement rule is not evenly divisible for store {storeId}, day {originalRule.DayOfWeek}.");

                if (newWindowMinutes <= 0 || newWindowMinutes % slotDurationMinutes != 0)
                {
                    continue;
                }

                updatedRules.Add(new SlotRuleReplace
                {
                    dayOfWeek = originalRule.DayOfWeek,

                    startTime = newStart.ToString(@"hh\:mm"),

                    endTime = newEnd.ToString(@"hh\:mm"),

                    slotDurationMinutes = slotDurationMinutes,

                    slotCapacity = originalRule.SlotCapacity
                });
            }

            if (originalRules.Count != updatedRules.Count)
            {
                throw new global::System.InvalidOperationException($"Replacement rule count mismatch before mutation for store {storeId}.");
            }

            return new SlotRuleReplaceRequestInput
            {
                rules = updatedRules
            };
        }

        private static (TimeSpan Start, TimeSpan End)? FindChangedWindow(StoreManagerSlotRulesHelper originalRule, List<StoreManagerSlotRulesHelper> allRules, TimeSpan openingTime, TimeSpan closingTime)
        {
            int slotDurationMinutes = originalRule.SlotDurationMinutes;

            TimeSpan slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

            if (closingTime <= openingTime || slotDuration <= TimeSpan.Zero)
            {
                return null;
            }

            if (!TimeSpan.TryParse(originalRule.StartTime, out TimeSpan originalStart) || !TimeSpan.TryParse(originalRule.EndTime, out TimeSpan originalEnd))
            {
                return null;
            }

            List<(TimeSpan Start, TimeSpan End)> otherRuleIntervals = allRules
                .Where(rule => rule.SlotRuleId != originalRule.SlotRuleId && rule.DayOfWeek == originalRule.DayOfWeek)
                .Where(rule => TimeSpan.TryParse(rule.StartTime, out _) && TimeSpan.TryParse(rule.EndTime, out _))
                .Select(rule =>
                {
                    TimeSpan.TryParse(rule.StartTime, out TimeSpan otherStart);
                    TimeSpan.TryParse(rule.EndTime, out TimeSpan otherEnd);
                    return (Start: otherStart, End: otherEnd);
                })
                .ToList();

            List<TimeSpan> candidateStarts = new();

            for (TimeSpan start = openingTime; start + slotDuration <= closingTime; start = start.Add(slotDuration))
            {
                candidateStarts.Add(start);
            }

            if (originalStart >= openingTime && originalStart + slotDuration <= closingTime && !candidateStarts.Contains(originalStart))
            {
                candidateStarts.Add(originalStart);
            }

            (TimeSpan Start, TimeSpan End)? bestCandidate = null;

            int bestDifference = int.MaxValue;

            foreach (TimeSpan candidateStart in candidateStarts)
            {
                for (TimeSpan candidateEnd = candidateStart.Add(slotDuration); candidateEnd <= closingTime; candidateEnd = candidateEnd.Add(slotDuration))
                {
                    bool isOriginalWindow = candidateStart == originalStart && candidateEnd == originalEnd;

                    if (isOriginalWindow)
                    {
                        continue;
                    }

                    bool overlapsAnotherRule = otherRuleIntervals.Any(interval =>
                           candidateStart < interval.End &&
                           candidateEnd > interval.Start);

                    if (overlapsAnotherRule)
                    {
                        continue;
                    }

                    int candidateDurationMinutes = (int)(candidateEnd - candidateStart).TotalMinutes;

                    bool durationIsValid = candidateDurationMinutes > 0 && candidateDurationMinutes % slotDurationMinutes == 0;

                    if (!durationIsValid)
                    {
                        continue;
                    }

                    int differenceFromOriginal = Math.Abs((int)(candidateStart - originalStart).TotalMinutes) + Math.Abs((int)(candidateEnd - originalEnd).TotalMinutes);

                    if (differenceFromOriginal < bestDifference)
                    {
                        bestDifference = differenceFromOriginal;

                        bestCandidate = (candidateStart, candidateEnd);
                    }
                }
            }

            return bestCandidate;
        }

        private static async Task<List<StoreManagerSlotRule>> GetApiRules(string storeId, string bearer)
        {
            HttpResponseMessage response = GraphQL.StoreManagerSlotRules(storeId, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerSlotRulesResponse? result = JsonConvert.DeserializeObject<StoreManagerSlotRulesResponse>(responseBody);

            Assert.NotNull(result);

            Assert.NotNull(result.data);

            return result.data.storeManagerSlotRules?.ToList() ?? new List<StoreManagerSlotRule>();
        }

        private static async Task<List<StoreManagerSlotRule>> WaitForApiRules(string storeId, string bearer, List<SlotRuleReplace> expectedRules)
        {
            List<StoreManagerSlotRule> latestRules = new();

            for (int attempt = 1; attempt <= 10; attempt++)
            {
                latestRules = await GetApiRules(storeId, bearer);

                if (RulesMatch(expectedRules, latestRules))
                {
                    return latestRules;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine($"Final API rules for store {storeId}: {string.Join("; ", latestRules.Select(rule => $"Day={rule.dayOfWeek}, Start={rule.startTime}, End={rule.endTime}, Duration={rule.slotDurationMinutes}, Capacity={rule.slotCapacity}"))}");

            return latestRules;
        }

        private static bool RulesMatch(List<SlotRuleReplace> expectedRules, List<StoreManagerSlotRule> actualRules)
        {
            if (expectedRules.Count != actualRules.Count)
            {
                return false;
            }

            List<SlotRuleReplace> orderedExpectedRules = expectedRules.OrderBy(rule => rule.dayOfWeek).ThenBy(rule => TimeSpan.Parse(rule.startTime)).ToList();

            List<StoreManagerSlotRule> orderedActualRules = actualRules.OrderBy(rule => rule.dayOfWeek).ThenBy(rule => ParseTimeOrMaxValue(rule.startTime)).ToList();

            for (int index = 0; index < orderedExpectedRules.Count; index++)
            {
                SlotRuleReplace expectedRule = orderedExpectedRules[index];

                StoreManagerSlotRule actualRule = orderedActualRules[index];

                if (actualRule.dayOfWeek != expectedRule.dayOfWeek || !RuleTimesMatch(actualRule.startTime, actualRule.endTime, expectedRule.startTime, expectedRule.endTime) || actualRule.slotDurationMinutes != expectedRule.slotDurationMinutes || actualRule.slotCapacity != expectedRule.slotCapacity)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateMutationResponse(List<SlotRuleReplace> expectedRules, List<StoreManagerReplaceSlotRule> actualRules, string storeId, SoftAssertHelper softAssert)
        {
            softAssert.AreEqual(expectedRules.Count, actualRules.Count, $"Mutation rule count mismatch for store {storeId}.");

            foreach (StoreManagerReplaceSlotRule rule in actualRules)
            {
                softAssert.True(TryParseRuleTimes(rule.startTime, rule.endTime, out _, out _), $"Invalid startTime or endTime returned for mutation rule {rule.id} in store {storeId}.");
            }

            foreach (SlotRuleReplace expectedRule in expectedRules)
            {
                StoreManagerReplaceSlotRule? actualRule = actualRules.FirstOrDefault(rule => rule.dayOfWeek == expectedRule.dayOfWeek && RuleTimesMatch(rule.startTime, rule.endTime, expectedRule.startTime, expectedRule.endTime));

                softAssert.True(actualRule != null, $"Mutation response missing rule for store {storeId}, day {expectedRule.dayOfWeek}, window {expectedRule.startTime}-{expectedRule.endTime}.");

                if (actualRule == null)
                {
                    continue;
                }

                softAssert.AreEqual(expectedRule.startTime, actualRule.startTime, $"Start time mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");

                softAssert.AreEqual(expectedRule.endTime, actualRule.endTime, $"End time mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");

                softAssert.AreEqual(expectedRule.slotDurationMinutes, actualRule.slotDurationMinutes, $"Duration mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");

                softAssert.AreEqual(expectedRule.slotCapacity, actualRule.slotCapacity, $"Capacity mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");
            }
        }

        private static void ValidateApiRules(List<SlotRuleReplace> expectedRules, List<StoreManagerSlotRule> actualRules, string storeId, SoftAssertHelper softAssert)
        {
            softAssert.AreEqual(expectedRules.Count, actualRules.Count, $"Persisted rule count mismatch for store {storeId}.");

            foreach (StoreManagerSlotRule rule in actualRules)
            {
                softAssert.True(TryParseRuleTimes(rule.startTime, rule.endTime, out _, out _), $"Invalid startTime or endTime returned for persisted rule {rule.id} in store {storeId}.");
            }

            foreach (SlotRuleReplace expectedRule in expectedRules)
            {
                StoreManagerSlotRule? actualRule = actualRules.FirstOrDefault(rule => rule.dayOfWeek == expectedRule.dayOfWeek && RuleTimesMatch(rule.startTime, rule.endTime, expectedRule.startTime, expectedRule.endTime));

                softAssert.True(actualRule != null, $"Persisted rule missing for store {storeId}, day {expectedRule.dayOfWeek}, window {expectedRule.startTime}-{expectedRule.endTime}.");

                if (actualRule == null)
                {
                    continue;
                }

                softAssert.AreEqual(expectedRule.startTime, actualRule.startTime, $"Persisted start time mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");

                softAssert.AreEqual(expectedRule.endTime, actualRule.endTime, $"Persisted end time mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");

                softAssert.AreEqual(expectedRule.slotDurationMinutes, actualRule.slotDurationMinutes, $"Persisted duration mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");

                softAssert.AreEqual(expectedRule.slotCapacity, actualRule.slotCapacity, $"Persisted capacity mismatch for store {storeId}, day {expectedRule.dayOfWeek}.");
            }
        }

        private static void ValidateExistingSlotData(string storeId, List<StoreManagerSlotRulesHelper> rules, List<StoreSlotRecord> slots, SoftAssertHelper softAssert)
        {
            List<int> validatedDays = new();

            foreach (StoreManagerSlotRulesHelper rule in rules)
            {
                string dayCode = GetDayCode(rule.DayOfWeek);

                bool timesParsed = TryParseRuleTimes(rule.StartTime, rule.EndTime, out TimeSpan ruleStart, out TimeSpan ruleEnd);

                softAssert.True(timesParsed, $"Invalid startTime or endTime for existing rule in store {storeId}, day {rule.DayOfWeek}.");

                if (!timesParsed)
                {
                    continue;
                }

                List<StoreSlotRecord> ruleSlots = slots.Where(slot => slot.IsActive && slot.DayCode.Is(dayCode) && slot.SlotStartTime >= ruleStart && slot.SlotEndTime <= ruleEnd).OrderBy(slot => slot.SlotStartTime).ToList();

                foreach (StoreSlotRecord slot in ruleSlots)
                {
                    int actualDuration = (int)(slot.SlotEndTime - slot.SlotStartTime).TotalMinutes;

                    softAssert.AreEqual(rule.SlotDurationMinutes, actualDuration, $"Existing duration mismatch for store {storeId}, day {rule.DayOfWeek}, window {rule.StartTime}-{rule.EndTime}.");
                }

                if (!validatedDays.Contains(rule.DayOfWeek))
                {
                    List<StoreSlotRecord> daySlots = slots.Where(slot => slot.IsActive && slot.DayCode.Is(dayCode)).OrderBy(slot => slot.SlotStartTime).ToList();

                    ValidateNoOverlaps(storeId, rule.DayOfWeek, daySlots, softAssert);

                    validatedDays.Add(rule.DayOfWeek);
                }
            }
        }

        private static void ValidateGeneratedSlots(string storeId, List<SlotRuleReplace> rules, List<StoreSlotRecord> slots, SoftAssertHelper softAssert)
        {
            List<int> validatedDays = new();

            foreach (SlotRuleReplace rule in rules)
            {
                string dayCode = GetDayCode(rule.dayOfWeek);

                bool timesParsed = TryParseRuleTimes(rule.startTime, rule.endTime, out TimeSpan ruleStart, out TimeSpan ruleEnd);

                softAssert.True(timesParsed, $"Invalid startTime or endTime for generated rule in store {storeId}, day {rule.dayOfWeek}.");

                if (!timesParsed)
                {
                    continue;
                }

                List<StoreSlotRecord> ruleSlots = slots.Where(slot => slot.IsActive && slot.DayCode.Is(dayCode) && slot.SlotStartTime >= ruleStart && slot.SlotEndTime <= ruleEnd).OrderBy(slot => slot.SlotStartTime).ToList();

                int expectedSlotCount = (int)((ruleEnd - ruleStart).TotalMinutes / rule.slotDurationMinutes);

                softAssert.AreEqual(expectedSlotCount, ruleSlots.Count, $"Generated slot count mismatch for store {storeId}, day {rule.dayOfWeek}, window {rule.startTime}-{rule.endTime}.");

                foreach (StoreSlotRecord slot in ruleSlots)
                {
                    int actualDuration = (int)(slot.SlotEndTime - slot.SlotStartTime).TotalMinutes;

                    softAssert.AreEqual(rule.slotDurationMinutes, actualDuration, $"Generated duration mismatch for store {storeId}, day {rule.dayOfWeek}, window {rule.startTime}-{rule.endTime}.");

                    softAssert.AreEqual(rule.slotCapacity, slot.Capacity, $"Generated capacity mismatch for store {storeId}, day {rule.dayOfWeek}, window {rule.startTime}-{rule.endTime}.");

                    softAssert.True(slot.Status.Is("AVAILABLE"), $"Generated slot status is invalid for store {storeId}, day {rule.dayOfWeek}, window {rule.startTime}-{rule.endTime}.");
                }

                if (!validatedDays.Contains(rule.dayOfWeek))
                {
                    List<StoreSlotRecord> daySlots = slots.Where(slot => slot.IsActive && slot.DayCode.Is(dayCode)).OrderBy(slot => slot.SlotStartTime).ToList();

                    ValidateNoOverlaps(storeId, rule.dayOfWeek, daySlots, softAssert);

                    validatedDays.Add(rule.dayOfWeek);
                }
            }
        }

        private static void ValidateNoOverlaps(string storeId, int dayOfWeek, List<StoreSlotRecord> slots, SoftAssertHelper softAssert)
        {
            for (int index = 1; index < slots.Count; index++)
            {
                StoreSlotRecord previousSlot = slots[index - 1];

                StoreSlotRecord currentSlot = slots[index];

                softAssert.True(currentSlot.SlotStartTime >= previousSlot.SlotEndTime, $"Overlapping slots found for store {storeId}, day {dayOfWeek}.");
            }
        }

        private static string GetDayCode(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Sun",
                1 => "Mon",
                2 => "Tue",
                3 => "Wed",
                4 => "Thu",
                5 => "Fri",
                6 => "Sat",
                _ => string.Empty
            };
        }

        private static string FormatRuleTime(string time)
        {
            if (!TimeSpan.TryParse(time, out TimeSpan parsedTime))
            {
                throw new Xunit.Sdk.XunitException($"Invalid database rule time: {time}");
            }

            return parsedTime.ToString(@"hh\:mm");
        }

        private static bool TryParseRuleTimes(string? startTime, string? endTime, out TimeSpan parsedStartTime, out TimeSpan parsedEndTime)
        {
            bool startParsed = TimeSpan.TryParse(startTime, out parsedStartTime);

            bool endParsed = TimeSpan.TryParse(endTime, out parsedEndTime);

            return startParsed && endParsed;
        }

        private static bool RuleTimesMatch(string? actualStartTime, string? actualEndTime, string expectedStartTime, string expectedEndTime)
        {
            return TryParseRuleTimes(actualStartTime, actualEndTime, out TimeSpan parsedActualStartTime, out TimeSpan parsedActualEndTime) && TryParseRuleTimes(expectedStartTime, expectedEndTime, out TimeSpan parsedExpectedStartTime, out TimeSpan parsedExpectedEndTime) && parsedActualStartTime == parsedExpectedStartTime && parsedActualEndTime == parsedExpectedEndTime;
        }

        private static TimeSpan ParseTimeOrMaxValue(string? value)
        {
            return TimeSpan.TryParse(value, out TimeSpan parsedTime) ? parsedTime : TimeSpan.MaxValue;
        }

        private static void ValidateEvenSlotDivision(List<StoreManagerSlotRule> rules, string storeId, SoftAssertHelper softAssert)
        {
            foreach (StoreManagerSlotRule rule in rules)
            {
                if (rule.startTime.IsNullOrWhiteSpace() || rule.endTime.IsNullOrWhiteSpace() || rule.slotDurationMinutes <= 0)
                {
                    continue;
                }

                bool timesParsed = TryParseRuleTimes(rule.startTime, rule.endTime, out TimeSpan start, out TimeSpan end);

                softAssert.True(timesParsed, $"Invalid startTime or endTime for persisted rule {rule.id} in store {storeId}.");

                if (!timesParsed)
                {
                    continue;
                }

                double durationMinutes = (end - start).TotalMinutes;

                softAssert.True(durationMinutes % rule.slotDurationMinutes == 0, $"SlotDuration {rule.slotDurationMinutes} does not divide evenly into persisted rule hours for store {storeId}, rule {rule.id}.");
            }
        }

        public static IEnumerable<object[]> GetStores()
        {
            List<string> storeIds = ExploreStoreMasterDBHelper.GetActiveStoresWithSlotRulesAndSlots().ToList();

            Console.WriteLine($"StoreManagerReplaceSlotRules selected stores: {string.Join(", ", storeIds)}");

            foreach (string storeId in storeIds)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
