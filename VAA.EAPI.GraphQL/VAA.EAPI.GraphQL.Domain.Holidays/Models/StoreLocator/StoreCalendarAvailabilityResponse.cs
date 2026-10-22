using StoreCalendarAvailabilityRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreCalendarAvailabilityRecord;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreCalendarAvailabilityResponse
    {
        public string StoreId { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public IEnumerable<StoreDayAvailability> Days { get; set; } = Enumerable.Empty<StoreDayAvailability>();

        public static explicit operator StoreCalendarAvailabilityResponse(StoreCalendarAvailabilityRecord infraRecord)
        {
            ArgumentNullException.ThrowIfNull(infraRecord);

            return new StoreCalendarAvailabilityResponse
            {
                StoreId = infraRecord.StoreId,
                Year = infraRecord.Year,
                Month = infraRecord.Month,
                Days = infraRecord.Days.Select(day => new StoreDayAvailability
                {
                    Date = day.Date,
                    HasAvailableSlot = day.HasAvailableSlot
                })
            };
        }
    }
}
