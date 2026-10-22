using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.TTL;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Interface
{
    public interface ITtlEnabledCity
    {
        Task<IEnumerable<TtlEnabledCityData>> Handle(string airportCode, CancellationToken cancellationToken);
    }
}

