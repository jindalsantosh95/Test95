using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.MemberAccountDetails
{
    public class MemberAccountReferenceDataCommand : IRequest<Result<Response<MemberAccountReferenceData>>>
    {
    }
}
