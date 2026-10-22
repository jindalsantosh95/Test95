using MediatR;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Repository
{
    public class OrderRepoCommand : IRequest<bool>
    {
        public List<CustomerOrderDetails> CustomerOrderDetails { get; }
        public OrderRepoCommand(List<CustomerOrderDetails> _CustomerOrderDetails)
        {
            CustomerOrderDetails = _CustomerOrderDetails;

        }
    }
}
