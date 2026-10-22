using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCTicketUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper.NDCTicketUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCTicketUpdate
{
    public class TicketUpdateCommandHandler : IRequestHandler<TicketUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator; 
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;
        public TicketUpdateCommandHandler(IMediator mediator, OrderDocumentRepoQueryHandler orderDocumentRepoQueryHandler, CosmosRepoCommandHandler cosmosRepoCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator)); 
            _orderDocumentRepoQueryHandler = orderDocumentRepoQueryHandler ?? throw new ArgumentException(nameof(orderDocumentRepoQueryHandler));
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler ?? throw new ArgumentNullException(nameof(cosmosRepoCommandHandler));

        }

        public async Task<Result<Unit>> Handle(TicketUpdateCommand request, CancellationToken cancellationToken)
        {
            var requestData = Json.Deserialize<IssueTicketResponse>(request.Data.ToString());
            var orderId = $"VS932{requestData.BookingReference.RLOC}{DateTime.UtcNow:yy}";

            var orderDoc = await _mediator.Send(new OrderDocumentRepoQuery() { OrderId = orderId }, cancellationToken);

            if (orderDoc?.Count > 0)
            {
                if (requestData.ChangeType != null && requestData.ChangeType == ChangeTypeEnum.DocumentIssuance)
                {
                    var historyOrderDoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = $"{orderId}-History" }, cancellationToken);

                    var updatedHistoryOrderDocument = OrderHistoryDocumentUpdateMapper.Update(orderDoc.First(), historyOrderDoc?.FirstOrDefault());

                    await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedHistoryOrderDocument, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);
                }

                var updatedOrderDoc = TicketUpdateMapper.MapOrderDocument(orderDoc.First(), requestData);

                await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedOrderDoc, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
