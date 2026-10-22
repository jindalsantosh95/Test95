using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.PartnersRetrieval
{
    public class PartnersRetrieveCommand : IRequest<Result<Response<PartnersResponse>>>
    {
    }
}
