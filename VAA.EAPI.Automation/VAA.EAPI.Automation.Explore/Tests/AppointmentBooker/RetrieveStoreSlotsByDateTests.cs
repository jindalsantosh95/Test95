using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.AppointmentBooker
{
    public class RetrieveStoreSlotsByDateTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public RetrieveStoreSlotsByDateTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionAppointmentBooker)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_RetrieveStoreSlotsByDate(string storeId)
        {
            SoftAssertHelper softAssert = new();

            string bearer = _tokenFixture.GetToken("GraphToken");

            DateOnly bookingDate = DateOnly.FromDateTime(DataGenerator.RandomFutureDate(1096));

            string dateString = bookingDate.ToString("yyyy-MM-dd");

            // Step 1: Retrieve the expected store record from StoreMaster.
            StoreMasterRecord dbStore = ExploreStoreMasterDBHelper.GetStoreById(storeId) ?? throw new InvalidOperationException($"StoreMaster record not found for store {storeId}");

            // Step 2: Call the RetrieveStoreSlotsByDate API.
            HttpResponseMessage response = GraphQL.RetrieveStoreSlotsByDate(storeId, dateString).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            RetrieveStoreSlotsByDateResponse? apiResult = JsonConvert.DeserializeObject<RetrieveStoreSlotsByDateResponse>(responseBody);

            if (apiResult?.data?.RetrieveStoreSlotsByDate is not Retrievestoreslotsbydate slotsResponse)
            {
                throw new InvalidOperationException($"RetrieveStoreSlotsByDate returned no data for store {storeId} on {dateString}");
            }

            Slot[] apiSlots = slotsResponse.slots ?? Array.Empty<Slot>();

            // Step 3: Validate the response store ID.
            softAssert.AreEqual(storeId, slotsResponse.storeId, $"Store ID mismatch for store {storeId}");

            // Step 4: Closed stores must not return slots.
            if (dbStore.MarkedAsClosed)
            {
                softAssert.True(apiSlots.Length == 0, $"Closed store {storeId} returned slots");

                softAssert.AssertAll();

                return;
            }

            // Step 5: Retrieve weekly opening hours from the API.
            HttpResponseMessage openingHoursResponse = GraphQL.StoreManagerOpeningHours(storeId, bearer).EnsureGQLResponseSuccess();

            string openingHoursBody = await openingHoursResponse.Content.ReadAsStringAsync();

            StoreManagerOpeningHoursResponse? openingHoursResult = JsonConvert.DeserializeObject<StoreManagerOpeningHoursResponse>(openingHoursBody);

            StoreManagerOpeningHour? apiOpeningHour = openingHoursResult?.data?.storeManagerOpeningHours?.FirstOrDefault(hour => hour.dayOfWeek == (int)bookingDate.DayOfWeek);

            if (apiOpeningHour == null)
            {
                throw new InvalidOperationException($"Opening hours were not found for store {storeId} on {bookingDate.DayOfWeek}");
            }

            // Step 6: Retrieve the matching database weekly opening hours.
            string dayCode = bookingDate.DayOfWeek.ToString()[..3];

            DbWeeklyHour? dbOpeningHour = dbStore.WeeklyHours.FirstOrDefault(hour => hour.day.StartsWith(dayCode, StringComparison.OrdinalIgnoreCase));

            if (dbOpeningHour == null)
            {
                throw new InvalidOperationException($"Database opening hours were not found for store {storeId} on {bookingDate.DayOfWeek}");
            }

            bool databaseWeeklyClosed = dbOpeningHour.isClosed || (dbOpeningHour.openingTime == "00:00" && dbOpeningHour.closingTime == "00:00");

            bool apiWeeklyClosed = apiOpeningHour.opensAt.Is("closed") && apiOpeningHour.closesAt.Is("closed");

            // Step 7: Validate the weekly opening-hours status.
            softAssert.AreEqual(databaseWeeklyClosed, apiWeeklyClosed, $"Weekly closed status mismatch for store {storeId} on {dateString}");

            if (databaseWeeklyClosed || apiWeeklyClosed)
            {
                softAssert.True(apiSlots.Length == 0, $"Closed weekly day returned slots for store {storeId} on {dateString}");

                softAssert.AssertAll();

                return;
            }

            // Step 8: Validate weekly opening and closing times.
            TimeSpan databaseOpeningTime = TimeSpan.Parse(dbOpeningHour.openingTime);

            TimeSpan databaseClosingTime = TimeSpan.Parse(dbOpeningHour.closingTime);

            TimeSpan apiOpeningTime = TimeSpan.Parse(apiOpeningHour.opensAt);

            TimeSpan apiClosingTime = TimeSpan.Parse(apiOpeningHour.closesAt);

            softAssert.AreEqual(databaseOpeningTime, apiOpeningTime, $"Opening time mismatch for store {storeId} on {dateString}");

            softAssert.AreEqual(databaseClosingTime, apiClosingTime, $"Closing time mismatch for store {storeId} on {dateString}");

            // Step 9: Validate date-specific closures.
            bool databaseDateClosure = ExploreStoreMasterDBHelper.HasActiveStoreClosure(storeId, bookingDate);

            if (databaseDateClosure)
            {
                softAssert.True(apiSlots.Length == 0, $"Date-specific closure returned slots for store {storeId} on {dateString}");

                softAssert.AssertAll();

                return;
            }

            // Step 10: Validate slot existence against active database slots.
            bool hasActiveDatabaseSlots = ExploreStoreMasterDBHelper.HasActiveStoreSlotsForDay(storeId, dayCode);

            softAssert.AreEqual(hasActiveDatabaseSlots, apiSlots.Length > 0, $"Slot existence mismatch for store {storeId} on {dateString}");

            if (!hasActiveDatabaseSlots)
            {
                softAssert.AssertAll();

                return;
            }

            // Step 11: Validate every returned API slot.
            Slot? previousSlot = null;

            foreach (Slot apiSlot in apiSlots)
            {
                DateTimeOffset startTime = apiSlot.startTime;

                DateTimeOffset endTime = apiSlot.endTime;

                TimeSpan duration = endTime.UtcDateTime - startTime.UtcDateTime;

                softAssert.AreEqual(bookingDate, DateOnly.FromDateTime(startTime.UtcDateTime), $"Slot start date mismatch for store {storeId}");

                softAssert.AreEqual(bookingDate, DateOnly.FromDateTime(endTime.UtcDateTime), $"Slot end date mismatch for store {storeId}");

                softAssert.True(endTime > startTime, $"Slot end time is not after start time for store {storeId}");

                softAssert.True(startTime.TimeOfDay >= databaseOpeningTime, $"Slot starts before opening time for store {storeId}");

                softAssert.True(endTime.TimeOfDay <= databaseClosingTime, $"Slot ends after closing time for store {storeId}");

                softAssert.True(duration.TotalMinutes == 30 || duration.TotalMinutes == 60, $"Invalid slot duration for store {storeId}: {duration.TotalMinutes} minutes");

                if (previousSlot != null)
                {
                    softAssert.AreEqual(previousSlot.endTime.UtcDateTime, startTime.UtcDateTime, $"Slot continuity mismatch for store {storeId}");
                }

                // Step 12: Validate that the API slot exists in the database.
                StoreSlotBookingState? databaseSlotState = ExploreStoreMasterDBHelper.GetSlotBookingState(storeId, bookingDate, startTime, endTime);

                softAssert.True(databaseSlotState != null, $"API slot was not found as an active database slot for store {storeId}");

                if (databaseSlotState != null)
                {
                    bool expectedAvailability = !databaseSlotState.HasBlockedAppointment && databaseSlotState.BookedCount < databaseSlotState.Capacity;

                    softAssert.AreEqual(expectedAvailability, apiSlot.isAvailable, $"Availability mismatch for store {storeId}, slot {startTime:HH:mm}-{endTime:HH:mm}");
                }

                previousSlot = apiSlot;
            }

            // Step 13: Report all validation failures for the current store.
            softAssert.AssertAll();
        }

        // Returns every active store from StoreMaster as an individual test case.
        public static IEnumerable<object[]> GetActiveStoreIds()
        {
            List<string> activeStoreIds = ExploreStoreMasterDBHelper.GetAllActiveStoreIds();

            if (activeStoreIds.Count == 0)
            {
                throw new InvalidOperationException("No active stores were found in StoreMaster");
            }

            foreach (string storeId in activeStoreIds)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
