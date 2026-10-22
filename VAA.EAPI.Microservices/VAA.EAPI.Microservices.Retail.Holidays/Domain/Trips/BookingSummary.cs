using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
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

        public static explicit operator BookingSummary(JToken bookingSummary)
        {
            if (bookingSummary == null) return null;

            return new()
            {
                BookingType = bookingSummary["bookingType"]?.Value<string>(),
                BookingTypeName = bookingSummary["bookingTypeName"]?.Value<string>(),
                BrandCode = bookingSummary["brand"]?.Value<string>(),
                BrandName = bookingSummary["brandName"]?.Value<string>(),
                Company = bookingSummary["company"]?.Value<string>(),
                CompanyName = bookingSummary["companyName"]?.Value<string>(),
                LeadPassengerName = bookingSummary["leadName"]?.Value<string>(),
                SellingCurrency = bookingSummary["sellingCurrency"]?.Value<string>(),
                DistributionChannel = bookingSummary["distributionChannel"]?.Value<string>(),
                DistributionChannelName = bookingSummary["distributionChannelName"]?.Value<string>(),
                Division = bookingSummary["division"]?.Value<string>(),
                DivisionName = bookingSummary["divisionName"]?.Value<string>(),
                SourceMarketCountry = bookingSummary["sourceMarketCountry"]?.Value<string>(),
                StoreLocation = (StoreLocationDetails)bookingSummary["storeLocation"],
                IsVIPBooking = bookingSummary["vipBooking"]?.Value<bool>()
            };
        }
    }
}