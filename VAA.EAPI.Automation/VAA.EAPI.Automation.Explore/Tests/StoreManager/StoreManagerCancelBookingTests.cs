using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Factory;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    [Collection("Sequential")]
    public class StoreManagerCancelBookingTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public StoreManagerCancelBookingTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreBookings)]
        public async Task TC01_StoreManagerCancelBooking(string storeId)
        {
            SoftAssertHelper softAssert = new();

            string bearer = _tokenFixture.GetToken("GraphToken");

            string bookingReference = $"BR-AUTO-CANCEL-{storeId}-{Guid.NewGuid():N}";

            AvailableSlotSelection slotSelection = await SaveAppointmentBookingHelper.GetNextAvailableSlot(storeId, bearer);

            DateOnly bookingDate = slotSelection.BookingDate;

            string dateString = bookingDate.ToString("yyyy-MM-dd");

            Slot selectedSlot = slotSelection.SelectedSlot;

            string? appointmentReferenceId = null;

            bool bookingCancelled = false;

            StoreSlotBookingState? initialSlotState = ExploreStoreMasterDBHelper.GetSlotBookingState(storeId, bookingDate, selectedSlot.startTime, selectedSlot.endTime);

            try
            {
                // Step 1: Build a dynamic booking request using the available slot.

                Bookingdetails bookingDetails = SaveAppointmentBookingFactory.Create(
                    "InStore",
                    "Manage Existing Booking",
                    bookingDate,
                    selectedSlot.startTime,
                    selectedSlot.endTime,
                    storeId,
                    "Automation",
                    "CancelTest",
                    $"automation.cancel.{storeId}@fly.virgin.com",
                    $"0344472{storeId}",
                    bookingReference);

                // Step 2: Create the booking.

                HttpResponseMessage saveResponse = GraphQL.SaveAppointmentBooking(bookingDetails).EnsureGQLResponseSuccess();

                string saveResponseBody = await saveResponse.Content.ReadAsStringAsync();

                SaveAppointmentBookingResponse? saveResult = JsonConvert.DeserializeObject<SaveAppointmentBookingResponse>(saveResponseBody);

                Assert.NotNull(saveResult);

                Assert.NotNull(saveResult.data);

                Assert.NotNull(saveResult.data.SaveAppointmentBooking);

                appointmentReferenceId = saveResult.data.SaveAppointmentBooking.appointmentReferenceId;

                Assert.False(appointmentReferenceId.IsNullOrWhiteSpace());

                // Step 3: Validate the created booking response.

                softAssert.AreEqual(bookingDate, DateOnly.Parse(saveResult.data.SaveAppointmentBooking.date), $"Booking date mismatch for store {storeId}");

                // Step 4: Validate the BOOKED database record.

                StoreAppointmentRecord? bookedDbAppointment = ExploreStoreMasterDBHelper.GetAppointmentById(appointmentReferenceId);

                softAssert.True(bookedDbAppointment != null, $"Created booking was not found in the database for store {storeId}");

                if (bookedDbAppointment != null)
                {
                    softAssert.AreEqual(appointmentReferenceId, bookedDbAppointment.Id, $"Created booking ID mismatch for store {storeId}");

                    softAssert.AreEqual(storeId, bookedDbAppointment.LocationId, $"Created booking store mismatch for store {storeId}");

                    softAssert.AreEqual(bookingDate, bookedDbAppointment.Date, $"Created booking date mismatch for store {storeId}");

                    softAssert.AreEqual(selectedSlot.startTime.TimeOfDay, bookedDbAppointment.SlotStart, $"Created booking start time mismatch for store {storeId}");

                    softAssert.AreEqual(selectedSlot.endTime.TimeOfDay, bookedDbAppointment.SlotEnd, $"Created booking end time mismatch for store {storeId}");

                    softAssert.AreEqual(bookingReference, bookedDbAppointment.BookingReference, $"Created booking customer reference mismatch for store {storeId}");

                    softAssert.AreEqual("BOOKED", bookedDbAppointment.AppointmentStatus, $"Created booking status mismatch for store {storeId}");
                }

                // Step 5: Query the booking through CancelledBookings using BOOKED status.

                HttpResponseMessage bookedQueryResponse = GraphQL.CancelledBookings(storeId, dateString, "BOOKED", bearer).EnsureGQLResponseSuccess();

                string bookedQueryBody = await bookedQueryResponse.Content.ReadAsStringAsync();

                CancelledBookingsResponse? bookedQueryResult = JsonConvert.DeserializeObject<CancelledBookingsResponse>(bookedQueryBody);

                Assert.NotNull(bookedQueryResult);

                CancelledBooking? bookedApiBooking = bookedQueryResult.data?.storeManagerBookings?.FirstOrDefault(booking =>
                    booking.id == appointmentReferenceId);

                softAssert.True(bookedApiBooking != null, $"Created booking was not returned by CancelledBookings with BOOKED status for store {storeId}");

                if (bookedApiBooking != null)
                {
                    softAssert.AreEqual(appointmentReferenceId, bookedApiBooking.id, $"BOOKED query booking ID mismatch for store {storeId}");

                    softAssert.AreEqual(storeId, bookedApiBooking.storeId, $"BOOKED query store ID mismatch for store {storeId}");

                    softAssert.AreEqual("BOOKED", bookedApiBooking.status, $"BOOKED query status mismatch for store {storeId}");

                    softAssert.AreEqual(bookingReference, bookedApiBooking.customerRef, $"BOOKED query customer reference mismatch for store {storeId}");

                    softAssert.True(bookedApiBooking.windowStartAt < bookedApiBooking.windowEndAt, $"BOOKED query window is invalid for store {storeId}");

                    softAssert.True(bookedApiBooking.windowCapacity > 0, $"BOOKED query capacity is invalid for store {storeId}");
                }

                // Step 6: Cancel the specific booking using StoreManagerCancelBooking.

                StoreManagerCancelledBooking cancelledBooking = await StoreManagerBookingHelper.CancelBooking(storeId, appointmentReferenceId, bearer);

                bookingCancelled = true;

                // Step 7: Validate the cancellation response.

                softAssert.AreEqual(appointmentReferenceId, cancelledBooking.id, $"Cancelled booking ID mismatch for store {storeId}");

                softAssert.AreEqual(storeId, cancelledBooking.storeId, $"Cancelled booking store ID mismatch for store {storeId}");

                softAssert.AreEqual("CANCELLED", cancelledBooking.status, $"Cancelled booking status mismatch for store {storeId}");

                softAssert.AreEqual(selectedSlot.startTime.UtcDateTime, cancelledBooking.windowStartAt.UtcDateTime, $"Cancelled booking start time mismatch for store {storeId}");

                softAssert.AreEqual(selectedSlot.endTime.UtcDateTime, cancelledBooking.windowEndAt.UtcDateTime, $"Cancelled booking end time mismatch for store {storeId}");

                softAssert.True(cancelledBooking.windowCapacity > 0, $"Cancelled booking capacity is invalid for store {storeId}");

                softAssert.True(cancelledBooking.updatedAt >= cancelledBooking.createdAt, $"Cancelled booking UpdatedAt is earlier than CreatedAt for store {storeId}");

                // Step 8: Validate the CANCELLED database record.

                StoreAppointmentRecord? cancelledDbAppointment = ExploreStoreMasterDBHelper.GetAppointmentById(appointmentReferenceId);

                softAssert.True(cancelledDbAppointment != null, $"Cancelled booking was not found in the database for store {storeId}");

                if (cancelledDbAppointment != null)
                {
                    softAssert.AreEqual(appointmentReferenceId, cancelledDbAppointment.Id, $"Cancelled database booking ID mismatch for store {storeId}");

                    softAssert.AreEqual(storeId, cancelledDbAppointment.LocationId, $"Cancelled database store mismatch for store {storeId}");

                    softAssert.AreEqual(bookingDate, cancelledDbAppointment.Date, $"Cancelled database date mismatch for store {storeId}");

                    softAssert.AreEqual(selectedSlot.startTime.TimeOfDay, cancelledDbAppointment.SlotStart, $"Cancelled database start time mismatch for store {storeId}");

                    softAssert.AreEqual(selectedSlot.endTime.TimeOfDay, cancelledDbAppointment.SlotEnd, $"Cancelled database end time mismatch for store {storeId}");

                    softAssert.AreEqual(bookingReference, cancelledDbAppointment.BookingReference, $"Cancelled database customer reference mismatch for store {storeId}");

                    softAssert.AreEqual("CANCELLED", cancelledDbAppointment.AppointmentStatus, $"Cancelled database status mismatch for store {storeId}");
                }

                // Step 9: Query the booking through CancelledBookings using CANCELLED status.

                HttpResponseMessage cancelledQueryResponse = GraphQL.CancelledBookings(storeId, dateString, "CANCELLED", bearer).EnsureGQLResponseSuccess();

                string cancelledQueryBody = await cancelledQueryResponse.Content.ReadAsStringAsync();

                CancelledBookingsResponse? cancelledQueryResult = JsonConvert.DeserializeObject<CancelledBookingsResponse>(cancelledQueryBody);

                Assert.NotNull(cancelledQueryResult);

                CancelledBooking? cancelledApiBooking = cancelledQueryResult.data?.storeManagerBookings?.FirstOrDefault(booking =>
                    booking.id == appointmentReferenceId);

                softAssert.True(cancelledApiBooking != null, $"Cancelled booking was not returned by CancelledBookings with CANCELLED status for store {storeId}");

                if (cancelledApiBooking != null)
                {
                    softAssert.AreEqual(appointmentReferenceId, cancelledApiBooking.id, $"CANCELLED query booking ID mismatch for store {storeId}");

                    softAssert.AreEqual(storeId, cancelledApiBooking.storeId, $"CANCELLED query store ID mismatch for store {storeId}");

                    softAssert.AreEqual("CANCELLED", cancelledApiBooking.status, $"CANCELLED query status mismatch for store {storeId}");

                    softAssert.AreEqual(bookingReference, cancelledApiBooking.customerRef, $"CANCELLED query customer reference mismatch for store {storeId}");

                    softAssert.True(cancelledApiBooking.windowStartAt < cancelledApiBooking.windowEndAt, $"CANCELLED query window is invalid for store {storeId}");

                    softAssert.True(cancelledApiBooking.windowCapacity > 0, $"CANCELLED query capacity is invalid for store {storeId}");
                }

                // Step 10: Poll the slots API until the cancelled slot becomes available again.

                Slot? restoredSlot = null;

                const int maxAttempts = 10;

                const int delayMilliseconds = 1000;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    Slot[] updatedSlots = await SaveAppointmentBookingHelper.GetSlots(storeId, dateString);

                    restoredSlot = SaveAppointmentBookingHelper.FindSlot(updatedSlots, selectedSlot.startTime, selectedSlot.endTime);

                    if (restoredSlot != null && restoredSlot.isAvailable)
                    {
                        break;
                    }

                    await Task.Delay(delayMilliseconds);
                }

                // Step 11: Confirm that the cancelled slot is available again.

                softAssert.True(restoredSlot != null, $"Cancelled slot was not returned after cancellation for store {storeId}");

                if (restoredSlot != null)
                {
                    softAssert.True(restoredSlot.isAvailable, $"Cancelled slot is still unavailable after cancellation for store {storeId}");
                }

                // Step 12: Validate the database slot booking state after cancellation.

                StoreSlotBookingState? restoredSlotState = ExploreStoreMasterDBHelper.GetSlotBookingState(storeId, bookingDate, selectedSlot.startTime, selectedSlot.endTime);

                softAssert.True(restoredSlotState != null, $"Restored slot state was not found in the database for store {storeId}");

                if (initialSlotState != null && restoredSlotState != null)
                {
                    softAssert.AreEqual(initialSlotState.BookedCount, restoredSlotState.BookedCount, $"Database booked count was not restored for store {storeId}");

                    softAssert.AreEqual(initialSlotState.Capacity, restoredSlotState.Capacity, $"Database slot capacity changed after cancellation for store {storeId}");

                    softAssert.AreEqual(initialSlotState.HasBlockedAppointment, restoredSlotState.HasBlockedAppointment, $"Database blocked state changed after cancellation for store {storeId}");
                }
            }
            finally
            {
                // Safety cleanup: cancel the booking if creation succeeded but cancellation did not complete.

                if (!appointmentReferenceId.IsNullOrWhiteSpace() && !bookingCancelled)
                {
                    try
                    {
                        await StoreManagerBookingHelper.CancelBooking(storeId, appointmentReferenceId, bearer);
                    }
                    catch (Exception cleanupException)
                    {
                        softAssert.True(false, $"Cleanup cancellation failed for booking {appointmentReferenceId} and store {storeId}: {cleanupException.Message}");
                    }
                }
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
