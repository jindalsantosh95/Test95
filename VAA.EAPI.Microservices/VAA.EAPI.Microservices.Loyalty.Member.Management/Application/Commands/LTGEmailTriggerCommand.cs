using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class LTGEmailTriggerCommand : IRequest<Result<Response<string>>>
    {
        public string? InitiatedBy { get; set; }
        public string? CustomerId { get; set; }
        public string? MemberId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}
