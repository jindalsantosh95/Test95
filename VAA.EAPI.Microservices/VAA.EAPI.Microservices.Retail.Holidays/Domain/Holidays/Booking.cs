using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class Booking
    {
        public ProductSummaryDetails Summary { get; set; }
        public BookingPayment Payment { get; set; }
        public string Status { get; set; }
        public JArray Data { get; set; }
        public IEnumerable<HolidayProductDetails> HolidayProducts { get; set; }
        public IEnumerable<TravellerParty> Travellers { get; set; }
        public PackageDetails HolidayPackageDetails { get; set; }
    }
}
