using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdateAddressEventCommand : IRequest<Response<string>>
    {
        public string MemberId { get; set; }
        public UpdateAddressRequest UpdateAddressRequest { get; set; }
    }
}
