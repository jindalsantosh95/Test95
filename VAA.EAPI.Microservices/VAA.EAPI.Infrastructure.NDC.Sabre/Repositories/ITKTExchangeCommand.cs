using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public interface ITKTExchangeCommand
    {
        Task<ExchangeRS> Exchange(ExchangeRQRequest exchangeRQRequest);
    }
}
