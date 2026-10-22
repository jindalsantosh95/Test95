using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountValidationCommand:IRequest<Response<string>>
    {
        public string IndividualFcNumber { get; set; }
        public string LeaderFcNumber { get; set; }
    }
}
