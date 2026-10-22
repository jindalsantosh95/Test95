using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    public class StoreManagerSlotAllocationsTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerSlotAllocationsTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_StoreManagerSlotAllocations(string storeId)
        {
            SoftAssertHelper softAssert = new();

            DateTimeOffset fromUtc = new(DateTime.UtcNow.Date);

            DateTimeOffset toUtc = fromUtc.AddDays(7).AddTicks(-1);

            string from = fromUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            string to = toUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            string bearer = _tokenFixture.GetToken("GraphToken");

            HttpResponseMessage slotsResponse = GraphQL.StoreManagerSlots(storeId, from, to, bearer).EnsureGQLResponseSuccess();

            string slotsResponseBody = await slotsResponse.Content.ReadAsStringAsync();

            StoreManagerSlotsResponse? slotsResult = JsonConvert.DeserializeObject<StoreManagerSlotsResponse>(slotsResponseBody);

            Assert.NotNull(slotsResult);

            Assert.NotNull(slotsResult.data);

            StoreManagerSlot[] slots = slotsResult.data.storeManagerSlots ?? Array.Empty<StoreManagerSlot>();

            Assert.NotEmpty(slots);

            StoreManagerSlot selectedSlot = slots.FirstOrDefault(slot =>
                !slot.id.IsNullOrWhiteSpace() &&
                slot.status == "AVAILABLE" &&
                slot.remainingCapacity > 0 &&
                slot.startAt >= fromUtc &&
                slot.endAt <= toUtc)
                ?? throw new Xunit.Sdk.XunitException($"No future AVAILABLE slot was returned for store {storeId}");

            string slotId = selectedSlot.id!;

            string startAt = selectedSlot.startAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

            HttpResponseMessage allocationResponse = GraphQL.StoreManagerSlotAllocations(storeId, slotId, startAt, bearer).EnsureGQLResponseSuccess();

            string allocationResponseBody = await allocationResponse.Content.ReadAsStringAsync();

            StoreManagerSlotAllocationsResponse? allocationResult = JsonConvert.DeserializeObject<StoreManagerSlotAllocationsResponse>(allocationResponseBody);

            Assert.NotNull(allocationResult);

            Assert.NotNull(allocationResult.data);

            StoreManagerSlotAllocationWindow apiWindow = allocationResult.data.storeManagerSlotAllocations
                ?? throw new Xunit.Sdk.XunitException($"No allocation window was returned for store {storeId}, slot {slotId}");

            StoreManagerSlotAllocation[] apiAllocations = apiWindow.allocations ?? Array.Empty<StoreManagerSlotAllocation>();

            Assert.NotEmpty(apiAllocations);

            List<StoreManagerSlotAllocationDbRecord> dbAllocations = StoreManagerSlotAllocationsDbHelper.GetSlotAllocations(storeId, slotId, selectedSlot.startAt);

            Assert.NotEmpty(dbAllocations);

            StoreManagerSlotAllocationDbRecord firstDbAllocation = dbAllocations[0];

            softAssert.AreEqual(storeId, apiWindow.storeId, $"Window StoreId mismatch for store {storeId}");

            softAssert.AreEqual(slotId, apiWindow.id, $"Window ID mismatch for store {storeId}");

            softAssert.AreEqual(slotId, apiWindow.windowId, $"WindowId mismatch for store {storeId}");

            softAssert.AreEqual(slotId, apiWindow.slotId, $"SlotId mismatch for store {storeId}");

            softAssert.AreEqual(firstDbAllocation.StartsAtUtc.DateTime, apiWindow.startAt.DateTime, $"Window startAt mismatch for store {storeId}");

            softAssert.AreEqual(firstDbAllocation.EndsAtUtc.DateTime, apiWindow.endAt.DateTime, $"Window endAt mismatch for store {storeId}");

            softAssert.AreEqual(firstDbAllocation.Capacity, apiWindow.capacity, $"Window capacity mismatch for store {storeId}");

            softAssert.AreEqual(firstDbAllocation.BookedCount, apiWindow.bookedCount, $"Window bookedCount mismatch for store {storeId}");

            softAssert.AreEqual(firstDbAllocation.RemainingCapacity, apiWindow.remainingCapacity, $"Window remainingCapacity mismatch for store {storeId}");

            softAssert.AreEqual(dbAllocations.Count, apiAllocations.Length, $"Allocation count mismatch for store {storeId}");

            int[] apiAllocationIndexes = apiAllocations
                .Select(allocation => allocation.allocationIndex)
                .OrderBy(index => index)
                .ToArray();

            int[] expectedAllocationIndexes = Enumerable.Range(1, apiAllocationIndexes.Length).ToArray();

            softAssert.True(apiAllocationIndexes.SequenceEqual(expectedAllocationIndexes), $"Allocation indexes are not sequential for store {storeId}");

            foreach (StoreManagerSlotAllocation apiAllocation in apiAllocations)
            {
                softAssert.True(!apiAllocation.id.IsNullOrWhiteSpace(), $"Allocation ID is empty for store {storeId}");

                softAssert.AreEqual(storeId, apiAllocation.storeId, $"Allocation StoreId mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(slotId, apiAllocation.windowId, $"Allocation WindowId mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(slotId, apiAllocation.slotId, $"Allocation SlotId mismatch for allocation {apiAllocation.id}");

                softAssert.True(apiAllocation.allocationIndex > 0, $"Invalid allocation index for allocation {apiAllocation.id}");

                softAssert.True(apiAllocation.status is "AVAILABLE" or "BOOKED" or "BLOCKED", $"Unexpected allocation status for allocation {apiAllocation.id}");

                StoreManagerSlotAllocationDbRecord? dbAllocation = dbAllocations.FirstOrDefault(databaseAllocation =>
                    databaseAllocation.AllocationId == apiAllocation.id);

                softAssert.True(dbAllocation != null, $"Database allocation {apiAllocation.id} was not returned by the API for store {storeId}");

                if (dbAllocation == null)
                {
                    continue;
                }

                softAssert.AreEqual(dbAllocation.StoreSlotId.ToString(), apiAllocation.slotId, $"Database SlotId mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(dbAllocation.AllocationId, apiAllocation.id, $"Allocation ID mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(dbAllocation.AllocationIndex, apiAllocation.allocationIndex, $"Allocation index mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(dbAllocation.Status, apiAllocation.status, $"Allocation status mismatch for allocation {apiAllocation.id}");

                string? expectedReason = dbAllocation.Reason.IsNullOrWhiteSpace() ? null : dbAllocation.Reason.Trim();

                string? actualReason = apiAllocation.reason.IsNullOrWhiteSpace() ? null : apiAllocation.reason.Trim();

                softAssert.AreEqual(expectedReason, actualReason, $"Allocation reason mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(dbAllocation.BookingId, apiAllocation.bookingId, $"BookingId mismatch for allocation {apiAllocation.id}");

                softAssert.AreEqual(dbAllocation.CustomerRef, apiAllocation.customerRef, $"CustomerRef mismatch for allocation {apiAllocation.id}");
            }

            softAssert.AssertAll();
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
