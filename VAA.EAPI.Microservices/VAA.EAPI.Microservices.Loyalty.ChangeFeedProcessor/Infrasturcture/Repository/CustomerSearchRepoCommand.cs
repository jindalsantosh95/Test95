using MediatR;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository
{
    public class CustomerSearchRepoCommand : IRequest<bool>
    {
        public CustomerSearch CustomerSearchDocument { get; set; }

        public CustomerSearchRepoCommand(CustomerSearch customerSearchDocument)
        {
            CustomerSearchDocument = customerSearchDocument;
        }
    }
}