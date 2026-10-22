using MediatR;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command
{
    public class ChangeFeedProcesCommand : IRequest<bool>
    {
        public CustomerSearch CustomerSearch { get; set; }

        public ChangeFeedProcesCommand(CustomerSearch custSearch)
        {
            CustomerSearch = custSearch;
        }
    }
}
