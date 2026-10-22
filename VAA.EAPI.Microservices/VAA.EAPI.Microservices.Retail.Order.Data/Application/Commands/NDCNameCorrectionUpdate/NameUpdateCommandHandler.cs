using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCNameCorrectionUpdate
{
    public class NameUpdateCommandHandler : INameUpdateCommand
    {
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;

        public NameUpdateCommandHandler(OrderDocumentRepoQueryHandler orderDocumentRepoQueryHandler, CosmosRepoCommandHandler cosmosRepoCommandHandler)
        {
            _orderDocumentRepoQueryHandler = orderDocumentRepoQueryHandler ?? throw new ArgumentNullException(nameof(OrderDocumentRepoQueryHandler));
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler ?? throw new ArgumentNullException(nameof(CosmosRepoCommandHandler));
        }

        public async Task<Result<Unit>> Handle(CloudEvent request)
        {
            var requestData = Json.Deserialize<OrderDocument>(Convert.ToString(request.Data));
            var orderId = $"VS932{requestData.BookingReference.RLOC}{DateTime.UtcNow:yy}";

            var orderdoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = orderId }, CancellationToken.None);

            if (orderdoc.Any())
            {
                var historyOrderDoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = $"{orderId}-History" }, CancellationToken.None);

                var updatedHistoryOrderDocument = OrderHistoryDocumentUpdateMapper.Update(orderdoc.First(), historyOrderDoc?.FirstOrDefault());

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedHistoryOrderDocument, ModificationType = CosmosModificationType.Upsert }, true), CancellationToken.None);

                var updatedOrderDoc = NameOrderUpdateMapper.UpdateNameCorrectionDocument(requestData, orderdoc.First());

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedOrderDoc, ModificationType = CosmosModificationType.Upsert }, true), CancellationToken.None);
            }

            return Unit.Value;
        }
    }
}