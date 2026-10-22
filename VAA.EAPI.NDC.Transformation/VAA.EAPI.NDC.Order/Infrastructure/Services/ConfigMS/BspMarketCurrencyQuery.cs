using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.BspMarket;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS
{
    public class BspMarketCurrencyQuery : IRequest<BspMarketCurrencyData>
    {
        public string CountryCode { get; set; }

        public BspMarketCurrencyQuery(string countryCode)
        {
            CountryCode = countryCode;
        }

    }
}
