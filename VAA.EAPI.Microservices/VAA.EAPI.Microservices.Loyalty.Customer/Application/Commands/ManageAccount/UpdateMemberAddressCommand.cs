using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdateMemberAddressCommand : IRequest<Response<string>>
    {
        public string MemberId { get; set; }
        public AccountAddressDetail AddressDetail { get; set; }

        public string Id { get; set; }
    }
}
