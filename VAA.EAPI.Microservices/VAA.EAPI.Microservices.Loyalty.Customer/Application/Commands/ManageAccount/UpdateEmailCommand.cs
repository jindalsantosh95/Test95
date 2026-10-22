using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdateEmailCommand : IRequest<Response<string>>
    {
        public string MemberNumber { get; set; }
        public string NewEmailAddress { get; set; }
    }
}
