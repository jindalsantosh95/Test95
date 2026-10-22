using NodaTime;


namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class TravelDatesDetails
    {
        public LocalDateTime? DepartureDate { get; set; }
        public DateTime? DepartureDateUtc { get; set; }
        public LocalDateTime? ReturnDate { get; set; }
        public DateTime? ReturnDateUtc { get; set; }

        public static explicit operator TravelDatesDetails(Infrastructure.Models.HolidaysMS.ProductSummaryDetails source)
        {
            if (source == null)
                return null;

            if (source.TravelDates != null)
            {
                return new()
                {
                    DepartureDate = source.TravelDates.StartDate.HasValue ? LocalDateTime.FromDateTime(source.TravelDates.StartDate.Value) : null,
                    ReturnDate = source.TravelDates.EndDate.HasValue ? LocalDateTime.FromDateTime(source.TravelDates.EndDate.Value) : null,
                    DepartureDateUtc = source.TravelDates.StartDate,
                    ReturnDateUtc = source.TravelDates.EndDate
                };
            }

            return new()
            {
                DepartureDate = source.DepartureDate.HasValue ? LocalDateTime.FromDateTime(source.DepartureDate.Value) : null,
                ReturnDate = source.ReturnDate.HasValue ? LocalDateTime.FromDateTime(source.ReturnDate.Value) : null,
                DepartureDateUtc = source.DepartureDate,
                ReturnDateUtc = source.ReturnDate
            };
        }
    }
}
