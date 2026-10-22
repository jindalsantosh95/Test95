using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ITtlEnabledCity
    {
        Task<IEnumerable<TtlEnabledCityData>> Handle(string airportCode, CancellationToken cancellationToken);
    }
}

