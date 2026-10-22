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
    public class StoreManagerUpdateSlotAllocationStatusTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private const string AvailableStatus = "AVAILABLE";
        private const string BlockedStatus = "BLOCKED";

        public StoreManagerUpdateSlotAllocationStatusTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreMutations)]
        public async Task TC01_StoreManagerUpdateSlotAllocationStatus(string storeId)
        {
            SoftAssertHelper softAssert = new();

            bool blockSucceeded = false;

            bool allocationRestored = false;

            DateTimeOffset fromUtc = new(DateTime.UtcNow.Date);

            DateTimeOffset toUtc = fromUtc.AddDays(7).AddTicks(-1);

            string from = fromUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            string to = toUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            string bearer = _tokenFixture.GetToken("GraphToken");

            string? slotId = null;

            string? allocationId = null;

            DateTimeOffset selectedSlotStartAt = default;

            string? startAt = null;

            try
            {
                // Step 1: Retrieve future slots and select one AVAILABLE slot dynamically.

                HttpResponseMessage slotsResponse = GraphQL.StoreManagerSlots(storeId, from, to, bearer).EnsureGQLResponseSuccess();

                string slotsResponseBody = await slotsResponse.Content.ReadAsStringAsync();

                StoreManagerSlotsResponse? slotsResult = JsonConvert.DeserializeObject<StoreManagerSlotsResponse>(slotsResponseBody);

                Assert.NotNull(slotsResult);

                StoreManagerSlot[] slots = slotsResult.data?.storeManagerSlots ?? Array.Empty<StoreManagerSlot>();

                Assert.NotEmpty(slots);

                StoreManagerSlot selectedSlot = slots.FirstOrDefault(slot =>
                    !slot.id.IsNullOrWhiteSpace() &&
                    slot.status == AvailableStatus &&
                    slot.remainingCapacity > 0 &&
                    slot.startAt >= fromUtc &&
                    slot.endAt <= toUtc)
                    ?? throw new Xunit.Sdk.XunitException($"No future AVAILABLE slot was returned for store {storeId}");

                slotId = selectedSlot.id;

                selectedSlotStartAt = selectedSlot.startAt;

                startAt = selectedSlot.startAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

                // Step 2: Retrieve the allocations for the selected slot.

                StoreManagerSlotAllocationWindow initialWindow = await GetAllocationWindow(storeId, slotId, startAt, bearer);

                StoreManagerSlotAllocation[] initialApiAllocations = initialWindow.allocations ?? Array.Empty<StoreManagerSlotAllocation>();

                Assert.NotEmpty(initialApiAllocations);

                // Step 3: Select one AVAILABLE allocation dynamically.

                StoreManagerSlotAllocation selectedAllocation = initialApiAllocations.FirstOrDefault(allocation =>
                    allocation.status == AvailableStatus &&
                    !allocation.id.IsNullOrWhiteSpace())
                    ?? throw new Xunit.Sdk.XunitException($"No AVAILABLE allocation was returned for store {storeId}, slot {slotId}");

                allocationId = selectedAllocation.id;

                // Step 4: Confirm the selected allocation is AVAILABLE in the database.

                List<StoreManagerSlotAllocationDbRecord> initialDbAllocations = StoreManagerSlotAllocationsDbHelper.GetSlotAllocations(storeId, slotId, selectedSlotStartAt);

                StoreManagerSlotAllocationDbRecord initialDbAllocation = initialDbAllocations.FirstOrDefault(allocation =>
                    allocation.AllocationId == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Database allocation {allocationId} was not found for store {storeId}");

                softAssert.AreEqual(AvailableStatus, initialDbAllocation.Status, $"Initial database status mismatch for allocation {allocationId}");

                // Step 5: Change the allocation from AVAILABLE to BLOCKED.

                StoreManagerSlotAllocationStatusUpdateRequestInput blockRequest = new()
                {
                    status = BlockedStatus,

                    reason = "Automation test allocation block"
                };

                HttpResponseMessage blockResponse = GraphQL.StoreManagerUpdateSlotAllocationStatus(storeId, slotId, allocationId, blockRequest, bearer).EnsureGQLResponseSuccess();

                blockSucceeded = true;

                string blockResponseBody = await blockResponse.Content.ReadAsStringAsync();

                StoreManagerUpdateSlotAllocationStatusResponse? blockResult = JsonConvert.DeserializeObject<StoreManagerUpdateSlotAllocationStatusResponse>(blockResponseBody);

                Assert.NotNull(blockResult);

                StoreManagerSlotAllocationWindow blockedMutationWindow = blockResult.data?.storeManagerUpdateSlotAllocationStatus
                    ?? throw new Xunit.Sdk.XunitException($"Blocked mutation returned no allocation window for store {storeId}");

                StoreManagerSlotAllocation blockedMutationAllocation = blockedMutationWindow.allocations?.FirstOrDefault(allocation =>
                    allocation.id == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Blocked mutation did not return allocation {allocationId} for store {storeId}");

                // Step 6: Validate the BLOCKED mutation response.

                softAssert.AreEqual(BlockedStatus, blockedMutationAllocation.status, $"Mutation status was not BLOCKED for allocation {allocationId}");

                softAssert.AreEqual("Automation test allocation block", blockedMutationAllocation.reason, $"Mutation reason mismatch for allocation {allocationId}");

                softAssert.AreEqual(storeId, blockedMutationAllocation.storeId, $"Mutation StoreId mismatch for allocation {allocationId}");

                softAssert.AreEqual(slotId, blockedMutationAllocation.slotId, $"Mutation SlotId mismatch for allocation {allocationId}");

                // Step 7: Re-query allocations after the BLOCKED mutation.

                StoreManagerSlotAllocationWindow blockedQueryWindow = await GetAllocationWindow(storeId, slotId, startAt, bearer);

                StoreManagerSlotAllocation blockedQueryAllocation = blockedQueryWindow.allocations?.FirstOrDefault(allocation =>
                    allocation.id == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Blocked allocation {allocationId} was not returned by the follow-up query for store {storeId}");

                softAssert.AreEqual(BlockedStatus, blockedQueryAllocation.status, $"Follow-up query status was not BLOCKED for allocation {allocationId}");

                softAssert.AreEqual("Automation test allocation block", blockedQueryAllocation.reason, $"Follow-up query reason mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedMutationAllocation.status, blockedQueryAllocation.status, $"Mutation and follow-up query status mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedMutationAllocation.reason, blockedQueryAllocation.reason, $"Mutation and follow-up query reason mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedMutationWindow.remainingCapacity, blockedQueryWindow.remainingCapacity, $"Mutation and follow-up query remaining capacity mismatch for allocation {allocationId}");

                // Step 8: Validate the BLOCKED state against the database.

                List<StoreManagerSlotAllocationDbRecord> blockedDbAllocations = StoreManagerSlotAllocationsDbHelper.GetSlotAllocations(storeId, slotId, selectedSlotStartAt);

                StoreManagerSlotAllocationDbRecord blockedDbAllocation = blockedDbAllocations.FirstOrDefault(allocation =>
                    allocation.AllocationId == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Blocked database allocation {allocationId} was not found for store {storeId}");

                softAssert.AreEqual(blockedDbAllocation.Status, blockedQueryAllocation.status, $"Database and API status mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedDbAllocation.Reason, blockedQueryAllocation.reason, $"Database and API reason mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedDbAllocation.RemainingCapacity, blockedQueryWindow.remainingCapacity, $"Database and API remaining capacity mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedDbAllocation.Capacity, blockedQueryWindow.capacity, $"Database and API capacity mismatch for allocation {allocationId}");

                softAssert.AreEqual(blockedDbAllocation.BookedCount, blockedQueryWindow.bookedCount, $"Database and API booked count mismatch for allocation {allocationId}");

                // Step 9: Validate the BLOCKED state through the availability API.

                string availabilityDate = selectedSlotStartAt.Date.ToString("yyyy-MM-dd");

                StoreManagerAvailabilityRangeItem blockedAvailabilityWindow = await GetAvailabilityWindow(storeId, availabilityDate, bearer, selectedSlotStartAt);

                List<StoreManagerAvailabilityRangeDbRecord> blockedDbAvailability = StoreManagerAvailabilityRangeDbHelper.GetAvailabilityRange(storeId, selectedSlotStartAt.Date, selectedSlotStartAt.Date);

                StoreManagerAvailabilityRangeDbRecord blockedDbAvailabilityWindow = blockedDbAvailability.FirstOrDefault(window =>
                    window.StartsAtUtc.DateTime == selectedSlotStartAt.DateTime)
                    ?? throw new Xunit.Sdk.XunitException($"Blocked database availability window was not found for store {storeId}");

                softAssert.AreEqual(blockedDbAvailabilityWindow.RemainingCapacity, blockedAvailabilityWindow.remainingCapacity, $"Blocked availability remaining capacity mismatch for store {storeId}");

                // Step 10: Restore the same allocation from BLOCKED to AVAILABLE.

                StoreManagerSlotAllocationStatusUpdateRequestInput restoreRequest = new()
                {
                    status = AvailableStatus,

                    reason = null
                };

                HttpResponseMessage restoreResponse = GraphQL.StoreManagerUpdateSlotAllocationStatus(storeId, slotId, allocationId, restoreRequest, bearer).EnsureGQLResponseSuccess();

                string restoreResponseBody = await restoreResponse.Content.ReadAsStringAsync();

                StoreManagerUpdateSlotAllocationStatusResponse? restoreResult = JsonConvert.DeserializeObject<StoreManagerUpdateSlotAllocationStatusResponse>(restoreResponseBody);

                Assert.NotNull(restoreResult);

                StoreManagerSlotAllocationWindow restoredMutationWindow = restoreResult.data?.storeManagerUpdateSlotAllocationStatus
                    ?? throw new Xunit.Sdk.XunitException($"Restore mutation returned no allocation window for store {storeId}");

                StoreManagerSlotAllocation restoredMutationAllocation = restoredMutationWindow.allocations?.FirstOrDefault(allocation =>
                    allocation.id == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Restore mutation did not return allocation {allocationId} for store {storeId}");

                allocationRestored = true;

                // Step 11: Validate the AVAILABLE restoration response.

                softAssert.AreEqual(AvailableStatus, restoredMutationAllocation.status, $"Restore mutation status was not AVAILABLE for allocation {allocationId}");

                softAssert.True(restoredMutationAllocation.reason.IsNullOrWhiteSpace(), $"Restore mutation reason was not cleared for allocation {allocationId}");

                // Step 12: Re-query allocations after the AVAILABLE restoration.

                StoreManagerSlotAllocationWindow restoredQueryWindow = await GetAllocationWindow(storeId, slotId, startAt, bearer);

                StoreManagerSlotAllocation restoredQueryAllocation = restoredQueryWindow.allocations?.FirstOrDefault(allocation =>
                    allocation.id == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Restored allocation {allocationId} was not returned by the follow-up query for store {storeId}");

                softAssert.AreEqual(AvailableStatus, restoredQueryAllocation.status, $"Follow-up query status was not AVAILABLE after restoration for allocation {allocationId}");

                softAssert.True(restoredQueryAllocation.reason.IsNullOrWhiteSpace(), $"Follow-up query reason was not cleared after restoration for allocation {allocationId}");

                softAssert.AreEqual(restoredMutationAllocation.status, restoredQueryAllocation.status, $"Restore mutation and follow-up query status mismatch for allocation {allocationId}");

                softAssert.AreEqual(restoredMutationAllocation.reason, restoredQueryAllocation.reason, $"Restore mutation and follow-up query reason mismatch for allocation {allocationId}");

                softAssert.AreEqual(restoredMutationWindow.remainingCapacity, restoredQueryWindow.remainingCapacity, $"Restore mutation and follow-up query remaining capacity mismatch for allocation {allocationId}");

                // Step 13: Validate the AVAILABLE restoration against the database.

                List<StoreManagerSlotAllocationDbRecord> restoredDbAllocations = StoreManagerSlotAllocationsDbHelper.GetSlotAllocations(storeId, slotId, selectedSlotStartAt);

                StoreManagerSlotAllocationDbRecord restoredDbAllocation = restoredDbAllocations.FirstOrDefault(allocation =>
                    allocation.AllocationId == allocationId)
                    ?? throw new Xunit.Sdk.XunitException($"Restored database allocation {allocationId} was not found for store {storeId}");

                softAssert.AreEqual(restoredDbAllocation.Status, restoredQueryAllocation.status, $"Restored database and API status mismatch for allocation {allocationId}");

                softAssert.True(restoredDbAllocation.Reason.IsNullOrWhiteSpace(), $"Restored database reason was not cleared for allocation {allocationId}");

                softAssert.AreEqual(restoredDbAllocation.RemainingCapacity, restoredQueryWindow.remainingCapacity, $"Restored database and API remaining capacity mismatch for allocation {allocationId}");

                softAssert.AreEqual(restoredDbAllocation.Capacity, restoredQueryWindow.capacity, $"Restored database and API capacity mismatch for allocation {allocationId}");

                softAssert.AreEqual(restoredDbAllocation.BookedCount, restoredQueryWindow.bookedCount, $"Restored database and API booked count mismatch for allocation {allocationId}");

                // Step 14: Validate the AVAILABLE restoration through the availability API.

                StoreManagerAvailabilityRangeItem restoredAvailabilityWindow = await GetAvailabilityWindow(storeId, availabilityDate, bearer, selectedSlotStartAt);

                List<StoreManagerAvailabilityRangeDbRecord> restoredDbAvailability = StoreManagerAvailabilityRangeDbHelper.GetAvailabilityRange(storeId, selectedSlotStartAt.Date, selectedSlotStartAt.Date);

                StoreManagerAvailabilityRangeDbRecord restoredDbAvailabilityWindow = restoredDbAvailability.FirstOrDefault(window =>
                    window.StartsAtUtc.DateTime == selectedSlotStartAt.DateTime)
                    ?? throw new Xunit.Sdk.XunitException($"Restored database availability window was not found for store {storeId}");

                softAssert.AreEqual(restoredDbAvailabilityWindow.RemainingCapacity, restoredAvailabilityWindow.remainingCapacity, $"Restored availability remaining capacity mismatch for store {storeId}");
            }
            finally
            {
                // Safety cleanup: restore the allocation if the test failed after the BLOCKED mutation.

                if (blockSucceeded && !allocationRestored && slotId != null && allocationId != null)
                {
                    try
                    {
                        StoreManagerSlotAllocationStatusUpdateRequestInput cleanupRequest = new()
                        {
                            status = AvailableStatus,

                            reason = null
                        };

                        GraphQL.StoreManagerUpdateSlotAllocationStatus(storeId, slotId, allocationId, cleanupRequest, bearer).EnsureGQLResponseSuccess();
                    }
                    catch (Exception cleanupException)
                    {
                        softAssert.True(false, $"Cleanup failed for store {storeId}, allocation {allocationId}: {cleanupException.Message}");
                    }
                }
            }

            softAssert.AssertAll();
        }

        private static async Task<StoreManagerSlotAllocationWindow> GetAllocationWindow(string storeId, string slotId, string startAt, string bearer)
        {
            HttpResponseMessage response = GraphQL.StoreManagerSlotAllocations(storeId, slotId, startAt, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerSlotAllocationsResponse? result = JsonConvert.DeserializeObject<StoreManagerSlotAllocationsResponse>(responseBody);

            Assert.NotNull(result);

            StoreManagerSlotAllocationWindow allocationWindow = result.data?.storeManagerSlotAllocations
                ?? throw new Xunit.Sdk.XunitException($"No allocation window was returned for store {storeId}, slot {slotId}");

            return allocationWindow;
        }

        private static async Task<StoreManagerAvailabilityRangeItem> GetAvailabilityWindow(string storeId, string date, string bearer, DateTimeOffset expectedStartAt)
        {
            HttpResponseMessage response = GraphQL.StoreManagerAvailabilityRange(storeId, date, date, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerAvailabilityRangeResponse? result = JsonConvert.DeserializeObject<StoreManagerAvailabilityRangeResponse>(responseBody);

            Assert.NotNull(result);

            StoreManagerAvailabilityRangeItem[] windows = result.data?.storeManagerAvailabilityRange
                ?? Array.Empty<StoreManagerAvailabilityRangeItem>();

            StoreManagerAvailabilityRangeItem availabilityWindow = windows.FirstOrDefault(window =>
                window.startAt.DateTime == expectedStartAt.DateTime)
                ?? throw new Xunit.Sdk.XunitException($"No availability window was returned for store {storeId}, start time {expectedStartAt:O}");

            return availabilityWindow;
        }

        public static IEnumerable<object[]> GetActiveStoreIds()
        {
            List<string> storeIds = ExploreStoreMasterDBHelper.GetActiveStoresWithSlotRulesAndSlots();

            Assert.NotEmpty(storeIds);

            foreach (string storeId in storeIds)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
