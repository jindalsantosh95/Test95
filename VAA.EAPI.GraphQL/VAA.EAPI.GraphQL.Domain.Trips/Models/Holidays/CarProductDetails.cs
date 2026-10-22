using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class CarProductDetails
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public RateInfo? Rate { get; set; }

        internal CarProductDetails Clone()
        {
            return new CarProductDetails
            {
                Id = this.Id,
                Code = this.Code,
                Name = this.Name,
                Rate = this.Rate?.Clone()
            };
        }

        public static explicit operator CarProductDetails(Infrastructure.Models.HolidaysMS.CarProductDetails source)
        {
            if (source == null)
                return null;

            return new()
            {
                Id = source.Id,
                Code = source.Code,
                Name = source.Name,
                Rate = source.Rate
            };
        }
    }
}
