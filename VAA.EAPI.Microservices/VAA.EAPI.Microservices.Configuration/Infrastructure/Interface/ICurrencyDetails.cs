using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ICurrencyDetails
    {
        Task<CurrencyDbEntity> GetCurrencyDetails(string rowKey);
    }
}
