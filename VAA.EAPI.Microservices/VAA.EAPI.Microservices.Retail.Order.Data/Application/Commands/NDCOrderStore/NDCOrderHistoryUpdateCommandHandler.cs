using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore
{
    public class NDCOrderHistoryUpdateCommandHandler : INDCOrderHistoryUpdateCommand
    {
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;

        public NDCOrderHistoryUpdateCommandHandler(CosmosRepoCommandHandler cosmosRepoCommandHandler, OrderDocumentRepoQueryHandler orderDocumentRepoQueryHandler)
        {
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler;
            _orderDocumentRepoQueryHandler = orderDocumentRepoQueryHandler;
        }

        public async Task<Result<Unit>> Handle(CloudEvent cloudEvent)
        {
            var requestData = Json.Deserialize<OrderDocument>(Convert.ToString(cloudEvent.Data));
            var orderId = $"VS932{requestData.BookingReference.RLOC}{DateTime.UtcNow:yy}";

            var orderDoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = orderId }, CancellationToken.None);

            if (orderDoc?.Count > 0)
            {
                var historyOrderDoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = $"{orderId}-History" }, CancellationToken.None);

                var updatedHistoryOrderDocument = OrderHistoryDocumentUpdateMapper.Update(orderDoc.First(), historyOrderDoc?.FirstOrDefault());

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedHistoryOrderDocument, ModificationType = CosmosModificationType.Upsert }, true), CancellationToken.None);

                var updatedOrderDoc = OrderChangeHistoryUpdateMapper.UpdateOrderDocument( orderDoc.First(), requestData);

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedOrderDoc, ModificationType = CosmosModificationType.Upsert }, true), CancellationToken.None);
            }

            return Unit.Value;
        }

    }

}

