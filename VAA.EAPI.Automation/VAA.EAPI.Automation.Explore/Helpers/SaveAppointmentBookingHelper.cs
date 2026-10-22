using System.Globalization;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class AvailableSlotSelection
    {
        public DateOnly BookingDate { get; set; }

        public Slot SelectedSlot { get; set; } = null!;
    }

    public static class SaveAppointmentBookingHelper
    {
        public static async Task<Slot[]> GetSlotsWithOpeningHours(string locationId, string dateString, string token)
        {
            Slot[] slots = await GetSlots(locationId, dateString);

            HttpResponseMessage openingHoursResponse = GraphQL.StoreManagerOpeningHours(locationId, token).EnsureGQLResponseSuccess();

            StoreManagerOpeningHoursResponse? openingHoursResult = JsonConvert.DeserializeObject<StoreManagerOpeningHoursResponse>(await openingHoursResponse.Content.ReadAsStringAsync());

            DateTime bookingDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            StoreManagerOpeningHour? openingHour = openingHoursResult?.data?.storeManagerOpeningHours?.FirstOrDefault(hour => hour.dayOfWeek == (int)bookingDate.DayOfWeek);

            if (openingHour == null)
            {
                throw new InvalidOperationException($"Opening hours were not found for store {locationId} on {dateString}.");
            }

            if (openingHour.opensAt.Is("closed") && openingHour.closesAt.Is("closed"))
            {
                if (slots.Length > 0)
                {
                    throw new InvalidOperationException($"Slots were returned for a closed store {locationId}.");
                }

                return slots;
            }

            TimeSpan openingTime = TimeSpan.Parse(openingHour.opensAt);

            TimeSpan closingTime = TimeSpan.Parse(openingHour.closesAt);

            DateTime openingDateTime = bookingDate.Date.Add(openingTime);

            DateTime closingDateTime = bookingDate.Date.Add(closingTime);

            if (slots.Length > 0 && slots[0].startTime.DateTime < openingDateTime)
            {
                throw new InvalidOperationException($"First slot is before opening time for store {locationId}.");
            }

            if (slots.Length > 0 && slots[^1].endTime.DateTime > closingDateTime)
            {
                throw new InvalidOperationException($"Last slot is after closing time for store {locationId}.");
            }

            foreach (Slot slot in slots)
            {
                TimeSpan duration = slot.endTime.UtcDateTime - slot.startTime.UtcDateTime;

                if (duration.TotalMinutes != 30 && duration.TotalMinutes != 60)
                {
                    throw new InvalidOperationException($"Invalid slot duration {duration.TotalMinutes} minutes for store {locationId}.");
                }
            }

            return slots;
        }

        public static async Task<Slot[]> GetSlots(string locationId, string dateString)
        {
            HttpResponseMessage response = GraphQL.RetrieveStoreSlotsByDate(locationId, dateString).EnsureGQLResponseSuccess();

            RetrieveStoreSlotsByDateResponse? result = JsonConvert.DeserializeObject<RetrieveStoreSlotsByDateResponse>(await response.Content.ReadAsStringAsync());

            if (result?.data?.RetrieveStoreSlotsByDate?.slots == null)
            {
                throw new InvalidOperationException($"RetrieveStoreSlotsByDate returned no slot data for store {locationId}.");
            }

            return result.data.RetrieveStoreSlotsByDate.slots;
        }

        public static Slot? FindSlot(Slot[] slots, DateTimeOffset start, DateTimeOffset end)
        {
            return slots.FirstOrDefault(slot => slot.startTime.UtcDateTime == start.UtcDateTime && slot.endTime.UtcDateTime == end.UtcDateTime);
        }

        public static async Task<Slot?> WaitForSlotAvailability(string locationId, string dateString, DateTimeOffset start, DateTimeOffset end, bool expectedAvailability)
        {
            const int maximumAttempts = 10;

            const int delayMilliseconds = 1000;

            Slot? matchingSlot = null;

            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                Slot[] slots = await GetSlots(locationId, dateString);

                matchingSlot = FindSlot(slots, start, end);

                if (matchingSlot != null && matchingSlot.isAvailable == expectedAvailability)
                {
                    return matchingSlot;
                }

                if (attempt < maximumAttempts - 1)
                {
                    await Task.Delay(delayMilliseconds);
                }
            }

            return matchingSlot;
        }

        public static async Task<AvailableSlotSelection> GetNextAvailableSlot(string locationId, string token)
        {
            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

            const int maximumMonthsToSearch = 36;

            for (int monthOffset = 0; monthOffset < maximumMonthsToSearch; monthOffset++)
            {
                DateOnly monthDate = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(monthOffset);

                HttpResponseMessage calendarResponse = GraphQL.RetrieveStoreCalendarAvailability(locationId, monthDate.Year, monthDate.Month).EnsureGQLResponseSuccess();

                RetrieveStoreCalendarAvailabilityResponse? calendarResult = JsonConvert.DeserializeObject<RetrieveStoreCalendarAvailabilityResponse>(await calendarResponse.Content.ReadAsStringAsync());

                if (calendarResult?.data?.RetrieveStoreCalendarAvailability?.days == null)
                {
                    throw new InvalidOperationException($"Calendar availability response is missing for store {locationId}, {monthDate:yyyy-MM}.");
                }

                Day? availableDay = calendarResult.data.RetrieveStoreCalendarAvailability.days.Where(day => DateOnly.FromDateTime(day.date) >= startDate && day.hasAvailableSlot).OrderBy(day => day.date).FirstOrDefault();

                if (availableDay == null)
                {
                    continue;
                }

                DateOnly bookingDate = DateOnly.FromDateTime(availableDay.date);

                string dateString = bookingDate.ToString("yyyy-MM-dd");

                Slot[] slots = await GetSlotsWithOpeningHours(locationId, dateString, token);

                Slot? availableSlot = slots.FirstOrDefault(slot => slot.isAvailable);

                if (availableSlot == null)
                {
                    throw new InvalidOperationException($"Calendar reported availability, but RetrieveStoreSlotsByDate returned no available slot for store {locationId} on {dateString}.");
                }

                return new AvailableSlotSelection
                {
                    BookingDate = bookingDate,
                    SelectedSlot = availableSlot
                };
            }

            throw new InvalidOperationException($"No available slot was found for store {locationId} within the next {maximumMonthsToSearch} months.");
        }
    }
}
