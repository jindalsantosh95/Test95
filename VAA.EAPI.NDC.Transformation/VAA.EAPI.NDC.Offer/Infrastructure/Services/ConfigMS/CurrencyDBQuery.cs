using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Currency;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class CurrencyDBQuery : IRequest<CurrencyData>
    {
        public string CountryCode { get; set; }

        public CurrencyDBQuery(string countryCode)
        {
            CountryCode = countryCode;
        }
    }
}
