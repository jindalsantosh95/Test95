using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class BspMarketPaymentQuery : IRequest<BspMarketPaymentData>
    {
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }

        public BspMarketPaymentQuery(string countryCode, string currencyCode)
        {
            CountryCode = countryCode;
            CurrencyCode = currencyCode;
        }

    }
}
