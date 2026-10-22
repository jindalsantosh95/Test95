using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public class PaymentPos
    {
        public string CountryCode { get; set; }
        public string CityCode { get; set; }
        public string CurrencyCode {  get; set; }
        public string IpAddress { get; set; }
        public string CurrencyNumber { get; set; }

        public static explicit operator PaymentPos((Pos pos, string ipAddress, IEnumerable<CurrencyNumberRefData> currencyNumbers) source)
        {
            var (pos, ipAddress, currencyNumbers) = source;
            return new()
            {
                CountryCode = pos.CountryCode,
                CityCode = pos.CityCode,
                CurrencyCode = pos.CurrencyCode,
                IpAddress = ipAddress,
                CurrencyNumber = currencyNumbers.FirstOrDefault(c => c.CurrencyCode == pos.CurrencyCode)?.CurrencyNumber
            };
        }

    }
}