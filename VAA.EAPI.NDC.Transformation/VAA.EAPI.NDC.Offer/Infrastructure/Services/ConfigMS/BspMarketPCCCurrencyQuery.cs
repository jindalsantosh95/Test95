using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class BspMarketPCCCurrencyQuery : IRequest<PccCurrencyData>
    {
        public string CurrencyCode { get; set; }

        public BspMarketPCCCurrencyQuery(string currencyCode)
        {
            CurrencyCode = currencyCode;
        }

    }
}
