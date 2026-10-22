using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SSADelinkOnTierUpdateCommand : IRequest<Result<Response<string>>>
    {
        public string InitiatedBy { get; set; }
        public string CustomerId { get; set; }
        public string OldTier { get; set; }
        public string NewTier { get; set; }
    }
}