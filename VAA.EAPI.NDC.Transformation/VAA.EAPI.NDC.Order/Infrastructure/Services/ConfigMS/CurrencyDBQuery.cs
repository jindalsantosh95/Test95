using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Currency;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS
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
