using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountDelinkingCommand : IRequest<Result<Response<HouseHoldAccountDelinkingResponse>>>
    {
        public string InitiatedBy { get; set; }
        public HouseHoldAccount HeadAccount { get; set; }
        public HouseHoldAccount ChildAccount { get; set; }
        public bool RestrictEmailTrigger { get; set; }
    }
}
