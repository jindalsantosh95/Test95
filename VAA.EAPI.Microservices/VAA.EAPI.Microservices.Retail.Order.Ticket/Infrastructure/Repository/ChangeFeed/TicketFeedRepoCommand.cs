using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository.ChangeFeed
{
    public class TicketFeedRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; }

        public TicketFeedRepoCommand(CosmosDocumentEvent cosmosEvent)
        {
            Event = cosmosEvent;
        }
    }
}
