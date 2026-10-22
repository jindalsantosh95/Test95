using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ITKTExchange
    {
        Task<ExchangeRS> ExchangeRQAsync(ExchangeRQRequest request);
    }
}
