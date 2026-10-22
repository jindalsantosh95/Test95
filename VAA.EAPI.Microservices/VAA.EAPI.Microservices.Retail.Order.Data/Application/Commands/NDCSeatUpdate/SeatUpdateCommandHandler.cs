using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCSeatUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper.NDCSeatUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCSeatUpdate
{
    public class SeatUpdateCommandHandler : IRequestHandler<SeatUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;

        public SeatUpdateCommandHandler(IMediator mediator, OrderDocumentRepoQueryHandler orderDocumentRepoQueryHandler, CosmosRepoCommandHandler cosmosRepoCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderDocumentRepoQueryHandler = orderDocumentRepoQueryHandler ?? throw new ArgumentNullException(nameof(OrderDocumentRepoQueryHandler));
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler ?? throw new ArgumentNullException(nameof(CosmosRepoCommandHandler));

        }

        public async Task<Result<Unit>> Handle(SeatUpdateCommand request, CancellationToken cancellationToken)
        {
            var requestData = Json.Deserialize<SeatFulfilmentResponse>(request.Data.ToString());
            var orderDoc = await _mediator.Send(new OrderDocumentRepoQuery() { OrderId = requestData.OrderId }, cancellationToken);

            if(orderDoc.Count > 0)
            {
                if(requestData.OrderItems.Any(x => x.SeatAssignmentType.Is("PostSale")))
                {
                    var historyOrderDoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = $"{orderDoc.First().OrderNumber.OrderId}-History" }, cancellationToken);

                    var updatedHistoryOrderDocument = OrderHistoryDocumentUpdateMapper.Update(orderDoc.First(), historyOrderDoc?.FirstOrDefault());

                    await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedHistoryOrderDocument, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);
                }

                var updatedOrderDoc = SeatUpdateMapper.UpdateOrderDocument(orderDoc.First(), requestData);

                await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedOrderDoc, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
