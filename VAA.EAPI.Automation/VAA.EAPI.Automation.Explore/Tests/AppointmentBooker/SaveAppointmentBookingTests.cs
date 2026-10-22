using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Factory;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.AppointmentBooker
{
    [Collection("Sequential")]
    public class SaveAppointmentBookingTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public SaveAppointmentBookingTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [Trait("Category", Traits.ExploreGraphqlRegressionAppointmentBooker)]
        [Trait("Suite", Traits.ExploreBookings)]
        [InlineData("InStore", "Manage Existing Booking", "1013", "Alekhya", "Test", "alekhya.bussa@fly.virgin.com", "990822805912")]
        [InlineData("CallBack", "Plan your next holiday", "1033", "Alekhya", "Test", "test@fly.virgin.com", "667762805912")]
        [InlineData("CallBack", "Manage Existing Booking", "1002", "Alekhya", "Test", "alekhya.bussa@fly.virgin.com", "955332805912")]
        [InlineData("InStore", "Plan your next holiday", "1029", "Alekhya", "Test", "test@fly.virgin.com", "800122805912")]
        [InlineData("InStore", "Manage Existing Booking", "1025", "Alekhya", "Test", "test@fly.virgin.com", "760122805912")]

        public async Task TC01_SaveAppointmentBooking(string appointmentType, string appointmentReason, string locationId, string firstName, string lastName, string email, string phone)
        {
            SoftAssertHelper softAssert = new();

            string bearer = _tokenFixture.GetToken("GraphToken");

            string bookingReference = $"BR-AUTO-{locationId}-{Guid.NewGuid():N}";

            AvailableSlotSelection slotSelection = await SaveAppointmentBookingHelper.GetNextAvailableSlot(locationId, bearer);

            DateOnly bookingDate = slotSelection.BookingDate;

            string dateString = bookingDate.ToString("yyyy-MM-dd");

            Slot selectedSlot = slotSelection.SelectedSlot;

            string? appointmentReferenceId = null;

            bool bookingCancelled = false;

            // Step 1: Read the initial database capacity and booking count.
            StoreSlotBookingState? initialState = ExploreStoreMasterDBHelper.GetSlotBookingState(locationId, bookingDate, selectedSlot.startTime, selectedSlot.endTime);

            Assert.NotNull(initialState);

            softAssert.True(initialState.BookedCount < initialState.Capacity, $"Selected slot is already full for store {locationId}.");

            softAssert.AssertAll();

            // Step 2: Build a complete and unique booking request.
            Bookingdetails bookingDetails = SaveAppointmentBookingFactory.Create(appointmentType, appointmentReason, bookingDate, selectedSlot.startTime, selectedSlot.endTime, locationId, firstName, lastName, email, phone, bookingReference);

            bookingDetails.visitPurpose = "Collect Tickets";

            bookingDetails.visitPurposeDetails = "Automation visit purpose details";

            bookingDetails.additionalDetails = "Automation booking additional details";

            try
            {
                // Step 3: Create the appointment.
                HttpResponseMessage saveResponse = GraphQL.SaveAppointmentBooking(bookingDetails).EnsureGQLResponseSuccess();

                SaveAppointmentBookingResponse? saveResult = JsonConvert.DeserializeObject<SaveAppointmentBookingResponse>(await saveResponse.Content.ReadAsStringAsync());

                Assert.NotNull(saveResult?.data?.SaveAppointmentBooking);

                SaveAppointmentBooking savedBooking = saveResult.data.SaveAppointmentBooking;

                Assert.NotNull(savedBooking.slotDetails);

                Assert.False(savedBooking.appointmentReferenceId.IsNullOrWhiteSpace(), $"Appointment reference ID is missing for store {locationId}.");

                string appointmentId = savedBooking.appointmentReferenceId;

                appointmentReferenceId = appointmentId;

                // Step 4: Validate the SaveAppointmentBooking response.
                StoreMasterRecord? store = ExploreStoreMasterDBHelper.GetStoreById(locationId);

                Assert.NotNull(store);

                softAssert.AreEqual(bookingDate, DateOnly.Parse(savedBooking.date), $"Booking date mismatch for store {locationId}.");

                softAssert.AreEqual(store.Name, savedBooking.store, $"Store name mismatch for store {locationId}.");

                softAssert.AreEqual(selectedSlot.startTime.UtcDateTime, savedBooking.slotDetails.startTime.UtcDateTime, $"Slot start mismatch for store {locationId}.");

                softAssert.AreEqual(selectedSlot.endTime.UtcDateTime, savedBooking.slotDetails.endTime.UtcDateTime, $"Slot end mismatch for store {locationId}.");

                softAssert.True(savedBooking.slotDetails.isAvailable, $"SaveAppointmentBooking should confirm that the selected slot was available for store {locationId}.");

                // Step 5: Validate the saved appointment directly in dbo.StoreAppointment.
                StoreAppointmentRecord? databaseBooking = ExploreStoreMasterDBHelper.GetAppointmentById(appointmentId);

                Assert.NotNull(databaseBooking);

                softAssert.AreEqual(appointmentId, databaseBooking.Id, $"Appointment ID mismatch for store {locationId}.");

                softAssert.AreEqual(bookingDetails.appointmentType?.ToString(), databaseBooking.AppointmentType, $"Appointment type mismatch for store {locationId}.");

                softAssert.AreEqual(bookingDetails.appointmentReason, databaseBooking.AppointmentReason, $"Appointment reason mismatch for store {locationId}.");

                softAssert.AreEqual(bookingReference, databaseBooking.BookingReference, $"Booking reference mismatch for store {locationId}.");

                softAssert.AreEqual(bookingDate, databaseBooking.Date, $"Database booking date mismatch for store {locationId}.");

                softAssert.AreEqual(selectedSlot.startTime.TimeOfDay, databaseBooking.SlotStart, $"Database slot start mismatch for store {locationId}.");

                softAssert.AreEqual(selectedSlot.endTime.TimeOfDay, databaseBooking.SlotEnd, $"Database slot end mismatch for store {locationId}.");

                softAssert.AreEqual(locationId, databaseBooking.LocationId, $"Database location mismatch for store {locationId}.");

                softAssert.AreEqual(bookingDetails.visitPurpose, databaseBooking.VisitPurpose, $"Visit purpose mismatch for store {locationId}.");

                softAssert.AreEqual(bookingDetails.visitPurposeDetails, databaseBooking.VisitPurposeDetails, $"Visit purpose details mismatch for store {locationId}.");

                softAssert.AreEqual(bookingDetails.additionalDetails, databaseBooking.AdditionalDetails, $"Additional details mismatch for store {locationId}.");

                softAssert.AreEqual("BOOKED", databaseBooking.AppointmentStatus, $"Appointment status mismatch for store {locationId}.");

                softAssert.True(!databaseBooking.AllocationId.IsNullOrWhiteSpace(), $"Allocation ID is missing for store {locationId}.");

                // Step 6: Confirm that RetrieveStoreSlotsByDate reflects the booking.
                bool expectedAvailabilityAfterBooking = initialState.BookedCount + 1 < initialState.Capacity;

                Slot? bookedSlot = await SaveAppointmentBookingHelper.WaitForSlotAvailability(locationId, dateString, selectedSlot.startTime, selectedSlot.endTime, expectedAvailabilityAfterBooking);

                softAssert.True(bookedSlot != null, $"Booked slot was not returned for store {locationId}.");

                if (bookedSlot != null)
                {
                    softAssert.AreEqual(expectedAvailabilityAfterBooking, bookedSlot.isAvailable, $"Slot availability after booking mismatch for store {locationId}.");
                }

                // Step 7: Cancel the appointment.
                StoreManagerCancelledBooking cancelledBooking = await StoreManagerBookingHelper.CancelBooking(locationId, appointmentId, bearer);

                bookingCancelled = true;

                // Step 8: Validate the cancellation response.
                softAssert.AreEqual(appointmentId, cancelledBooking.id, $"Cancelled appointment ID mismatch for store {locationId}.");

                softAssert.AreEqual(locationId, cancelledBooking.storeId, $"Cancelled store ID mismatch for store {locationId}.");

                softAssert.AreEqual(bookingReference, cancelledBooking.customerRef, $"Customer reference mismatch for store {locationId}.");

                softAssert.AreEqual("CANCELLED", cancelledBooking.status, $"Cancellation status mismatch for store {locationId}.");

                softAssert.AreEqual(selectedSlot.startTime.UtcDateTime, cancelledBooking.windowStartAt.UtcDateTime, $"Cancelled start mismatch for store {locationId}.");

                softAssert.AreEqual(selectedSlot.endTime.UtcDateTime, cancelledBooking.windowEndAt.UtcDateTime, $"Cancelled end mismatch for store {locationId}.");

                softAssert.True(cancelledBooking.windowCapacity > 0, $"Cancellation capacity is invalid for store {locationId}.");

                softAssert.True(cancelledBooking.updatedAt >= cancelledBooking.createdAt, $"Cancellation timestamp is invalid for store {locationId}.");

                // Step 9: Confirm that the database row remains but is marked CANCELLED.
                StoreAppointmentRecord? cancelledDatabaseBooking = ExploreStoreMasterDBHelper.GetAppointmentById(appointmentId);

                Assert.NotNull(cancelledDatabaseBooking);

                softAssert.AreEqual("CANCELLED", cancelledDatabaseBooking.AppointmentStatus, $"Database cancellation status mismatch for store {locationId}.");

                softAssert.True(cancelledDatabaseBooking.AllocationId.IsNullOrWhiteSpace(), $"Allocation ID was not cleared for store {locationId}.");

                // Step 10: Confirm that slot availability is restored after cancellation.
                bool expectedAvailabilityAfterCancellation = initialState.BookedCount < initialState.Capacity;

                Slot? restoredSlot = await SaveAppointmentBookingHelper.WaitForSlotAvailability(locationId, dateString, selectedSlot.startTime, selectedSlot.endTime, expectedAvailabilityAfterCancellation);

                softAssert.True(restoredSlot != null, $"Restored slot was not returned for store {locationId}.");

                if (restoredSlot != null)
                {
                    softAssert.AreEqual(expectedAvailabilityAfterCancellation, restoredSlot.isAvailable, $"Slot availability after cancellation mismatch for store {locationId}.");
                }
            }
            finally
            {
                // Step 11: Clean up if booking succeeded but cancellation did not complete.
                if (!appointmentReferenceId.IsNullOrWhiteSpace() && !bookingCancelled)
                {
                    await StoreManagerBookingHelper.CancelBooking(locationId, appointmentReferenceId, bearer);
                }
            }

            // Step 12: Report all soft assertion failures.
            softAssert.AssertAll();
        }
    }
}
