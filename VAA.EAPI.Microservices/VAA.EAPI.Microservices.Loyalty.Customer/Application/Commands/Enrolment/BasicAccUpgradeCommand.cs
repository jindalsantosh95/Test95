using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class BasicAccUpgradeCommand : IRequest<CreateCustomerEventResponse>
    {
        public CreateCustomerEntities CustomerEntities { get; set; }
        public BasicAccUpgradeCommand(CreateCustomerEntities customerEntities)
        {
            CustomerEntities = customerEntities;
        }
    }

}
