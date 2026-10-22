using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;

public interface ITitleDataQueryHandler
{
    Task<Result<Response<IReadOnlyCollection<TitleDetails>>>> Handle(CancellationToken cancellationToken);
}
