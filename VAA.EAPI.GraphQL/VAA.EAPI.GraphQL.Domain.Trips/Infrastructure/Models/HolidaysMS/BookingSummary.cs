namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class BookingSummary
    {
        public string? BrandName { get; set; }
        public string? BrandCode { get; set; }
        public string? Company { get; set; }
        public string? CompanyName { get; set; }
        public string? Division { get; set; }
        public string? DivisionName { get; set; }
        public string? BookingType { get; set; }
        public string? BookingTypeName { get; set; }
        public string? SourceMarketCountry { get; set; }
        public string? DistributionChannel { get; set; }
        public string? DistributionChannelName { get; set; }
        public string? SellingCurrency { get; set; }
        public StoreLocationDetails? StoreLocation { get; set; }
        public string? LeadPassengerName { get; set; }
        public bool? IsVIPBooking { get; set; }
    }
}
