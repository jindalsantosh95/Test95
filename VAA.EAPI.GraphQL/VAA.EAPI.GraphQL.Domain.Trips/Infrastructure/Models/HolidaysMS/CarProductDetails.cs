
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class CarProductDetails
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public RateInfo? Rate { get; set; }

        public static explicit operator CarProductDetails(CarDetails v)
        {
            throw new NotImplementedException();
        }
    }
}