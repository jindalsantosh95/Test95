using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SupplementarySilverAccountLinkCommand : IRequest<Result<Response<SupplementarySilverAccountLinkingResponse>>>
    {
        public string InitiatedBy { get; set; }
        public string HeadMemberNumber { get; set; }
        public string HeadCustomerId { get; set; }
        public string ChildMemberNumber { get; set; }
        public string ChildCustomerId { get; set; }
    }
}