using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class DriverDetails
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseCountry { get; set; }

        public static explicit operator DriverDetails((CarDetails Car, ProductSummaryDetails Summary) v)
        {
            return new();
        }
    }
}
