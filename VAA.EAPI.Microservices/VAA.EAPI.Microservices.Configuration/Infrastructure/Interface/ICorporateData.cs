using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ICorporateData
    {
        Task<IEnumerable<CorporateDbEntity>> GetCorporateData(CancellationToken cancellationToken);
        Task RefreshCorporateCache(string blobContent,CancellationToken cancellationToken);
    }
}