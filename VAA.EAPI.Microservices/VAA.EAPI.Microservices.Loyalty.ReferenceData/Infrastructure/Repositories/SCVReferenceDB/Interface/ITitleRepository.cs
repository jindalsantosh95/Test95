using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;

public interface ITitleRepository
{
    Task<IReadOnlyCollection<TitleDetails>> GetAllTitlesAsync(CancellationToken cancellationToken);
}
