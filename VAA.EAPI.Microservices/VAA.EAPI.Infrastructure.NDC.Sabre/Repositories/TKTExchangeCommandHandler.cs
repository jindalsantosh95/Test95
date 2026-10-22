using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class TKTExchangeCommandHandler : ITKTExchangeCommand
    {
        private readonly ITKTExchange _service;

        public TKTExchangeCommandHandler(ITKTExchange service)
        {
            _service = service;
        }

        public async Task<ExchangeRS> Exchange(ExchangeRQRequest exchangeRQRequest)
        {
            return await _service.ExchangeRQAsync(exchangeRQRequest);
        }
    }
}
