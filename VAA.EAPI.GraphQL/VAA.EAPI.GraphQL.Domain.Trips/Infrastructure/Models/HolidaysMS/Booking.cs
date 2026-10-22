namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class Booking
    {
        public BookingPayment Payment { get; set; }
        public IEnumerable<HolidayProductDetails> HolidayProducts { get; set; }
        public ProductSummaryDetails Summary { get; set; }
        public IEnumerable<TravellerParty> Travellers { get; set; }
        public PackageDetails HolidayPackageDetails { get; set; }
        public BookingSummary? BookingSummary { get; set; }
    }
}
