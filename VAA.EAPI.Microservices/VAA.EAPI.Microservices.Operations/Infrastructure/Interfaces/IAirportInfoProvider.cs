using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces
{
    public interface IAirportInfoProvider
    {
        Task<ICollection<Domain.AirportInfo.AirportInfo>> GetAllAirportsAsync();
    }
}
