using MediatR;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command.PhoneUpdate
{
    public class CustomerSearchUpdateCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }

        public CustomerSearchUpdateCommand(Customer customer)
        {
            Customer = customer;
        }
    }
}
