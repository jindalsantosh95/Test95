using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ITtlEnabledCityData
    {
        Task<IEnumerable<TtlEnabledCityDbEntity>> GetTtlEnabledCityData();
    }
}

