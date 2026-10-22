using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class PointOfSale
    {
        public string CityCode { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string DefaultCityCode { get; set; }
        public string CurrencyNumber { get; set; }
        public string NDCCityCodeLocation { get; set; }

        public static explicit operator PointOfSale(Pos source) =>
            source is null
                ? null
                : new PointOfSale
                {
                    CityCode = source.CityCode,
                    CountryCode = source.CountryCode,
                    CurrencyCode = source.CurrencyCode,
                    DefaultCityCode = source.DefaultCityCode,
                    CurrencyNumber = source.CurrencyNumber,
                    NDCCityCodeLocation = source.NDCCityCodeLocation
                };
    }
}
