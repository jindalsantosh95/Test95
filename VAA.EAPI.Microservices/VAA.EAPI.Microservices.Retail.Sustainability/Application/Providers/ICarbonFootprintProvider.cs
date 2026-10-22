using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Sustainability.Domain;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Application.Providers
{
    public interface ICarbonFootprintProvider
    {
        Task<IEnumerable<CarbonFootprint>> FetchCarbonFootprintsAsync(CarbonRequest request, CancellationToken cancellationToken);
    }
}
