using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class TKTExchange : ITKTExchange
    {
        private readonly ExchangePortTypeClient _client;

        public TKTExchange(ExchangePortTypeClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<ExchangeRS> ExchangeRQAsync(ExchangeRQRequest request)
        {
            var response = await _client.ExchangeRQAsync(request.MessageHeader , request.Security , request.ExchangeRQ);
            return response.ExchangeRS;
        }
    }
}
