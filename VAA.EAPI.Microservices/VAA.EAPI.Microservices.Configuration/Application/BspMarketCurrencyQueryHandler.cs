using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class BspMarketCurrencyQueryHandler : IRequestHandler<BspMarketCurrencyQuery, BspMarketCurrencyData>
    {
        private readonly IBspMarketCurrencyDetails _bspMarketCurrencyDetails;

        public BspMarketCurrencyQueryHandler(IBspMarketCurrencyDetails BspMarketCurrencyDetails)
        {
            _bspMarketCurrencyDetails = BspMarketCurrencyDetails ?? throw new ArgumentNullException(nameof(_bspMarketCurrencyDetails));
        }

        public async Task<BspMarketCurrencyData> Handle(BspMarketCurrencyQuery request, CancellationToken cancellationToken)
        {
            var bspMarketCurrencies = await _bspMarketCurrencyDetails.GetBspMarketCurrencyList();
            var result = (BspMarketCurrencyData)bspMarketCurrencies.FirstOrDefault(x => x.CountryCode == request.CountryCode);
            
            return result;
        }
    }
}
