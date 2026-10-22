using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces
{
    public interface IRefServiceProvider
    {
        Task<VAA.EAPI.GraphQL.Domain.RefData.Models.MobileFeatures> RetrieveMobileFeaturesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync();
        Task<IEnumerable<FQTProgrammeRefData>> RetrieveFQTProgrammesAsync();
    }
}
