using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCancelUpdate
{
    public class CancelOrderUpdateCommandHandler : IRequestHandler<CancelOrderUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;

        public CancelOrderUpdateCommandHandler(IMediator mediator, OrderDocumentRepoQueryHandler orderDocumentRepoQueryHandler, CosmosRepoCommandHandler cosmosRepoCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderDocumentRepoQueryHandler = orderDocumentRepoQueryHandler ?? throw new ArgumentException(nameof(orderDocumentRepoQueryHandler));
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler ?? throw new ArgumentNullException(nameof(cosmosRepoCommandHandler));
        }

        public async Task<Result<Unit>> Handle(CancelOrderUpdateCommand request, CancellationToken cancellationToken)
        {
            var cancelPnr = Json.Deserialize<Pnrdata>(Convert.ToString(request.Data));
            var orderId = $"VS932{cancelPnr.Rloc}{DateTime.UtcNow:yy}";
            var orderDoc = await _mediator.Send(new OrderDocumentRepoQuery() { OrderId = orderId }, cancellationToken);
            bool isNonRFC = !(cancelPnr.IsRFC ?? true) && cancelPnr.Operation == Pnrdata.Operations.OrderCancel;

            if (orderDoc?.Count > 0)
            {
                var historyOrderDoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = $"{orderId}-History" }, cancellationToken);

                var updatedHistoryOrderDocument = OrderHistoryDocumentUpdateMapper.Update(orderDoc.First(), historyOrderDoc?.FirstOrDefault());

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedHistoryOrderDocument, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);

                var updatedOrderDoc = CancelOrderUpdateMapper.UpdateCancelOrderStatus(orderDoc.First(), Constants.Application.CancelStatus, isNonRFC, cancelPnr.IsCancelDisruption);

                await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedOrderDoc, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
