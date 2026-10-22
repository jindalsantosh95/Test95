using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.SFPD;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Query
{
    public class SFPDSyncStatusQuery : EventStatusBaseQuery, IRequest<Result<Response<SFPDSyncStatusQueryResponse>>>
    {
       
    }
}
