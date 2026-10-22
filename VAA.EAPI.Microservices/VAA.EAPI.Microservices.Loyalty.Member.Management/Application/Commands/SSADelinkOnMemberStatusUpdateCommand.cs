using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SSADelinkOnMemberStatusUpdateCommand : IRequest<Result<Response<string>>>
    {
        public string InitiatedBy { get; set; }
        public string HeadCustomerId { get; set; }
    }
}
