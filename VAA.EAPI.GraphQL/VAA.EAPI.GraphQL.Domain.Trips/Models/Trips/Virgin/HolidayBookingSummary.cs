namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class HolidayBookingSummary
    {
        public string? BrandName { get; set; }
        public string? BrandCode { get; set; }

        public static explicit operator HolidayBookingSummary(Infrastructure.Models.HolidaysMS.BookingSummary source)
        {
            if (source == null) return default;

            return new()
            {
                BrandCode = source.BrandCode,
                BrandName = source.BrandName
            };
        }
    }
}
