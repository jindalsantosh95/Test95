using CSharpFunctionalExtensions;
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

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCArcDocumentUpdate
{
    public class ArcDocumentUpdateCommandHandler : IArcDocumentUpdateCommand
    {
        private readonly OrderDocumentRepoQueryHandler _orderDocumentRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;
        private readonly IArcDocumentRepoQuery _arcDocumentRepoQuery;

        public ArcDocumentUpdateCommandHandler(OrderDocumentRepoQueryHandler orderDocumentRepoQueryHandler, CosmosRepoCommandHandler cosmosRepoCommandHandler , IArcDocumentRepoQuery arcDocumentRepoQuery)
        {
            _orderDocumentRepoQueryHandler = orderDocumentRepoQueryHandler ?? throw new ArgumentNullException(nameof(orderDocumentRepoQueryHandler));
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler ?? throw new ArgumentNullException(nameof(cosmosRepoCommandHandler));
            _arcDocumentRepoQuery = arcDocumentRepoQuery ?? throw new ArgumentNullException(nameof(arcDocumentRepoQuery));
        }

        public async Task<Result> Handle(CloudEvent request)
        {
            var requestData = Json.Deserialize<OrderDocument>(Convert.ToString(request.Data));
            var orderId = $"VS932{requestData.BookingReference.RLOC}{DateTime.UtcNow:yy}";

            var orderdoc = await _orderDocumentRepoQueryHandler.Handle(new OrderDocumentRepoQuery() { OrderId = orderId }, CancellationToken.None);

            if (orderdoc.Any())
            {
                var arcDocument = await _arcDocumentRepoQuery.Handle($"{orderId}-ARC", CancellationToken.None);

                var updatedArcDocument = OrderArcDocumentUpdateMapper.Update(requestData, arcDocument);

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = updatedArcDocument, ModificationType = CosmosModificationType.Upsert }, true), CancellationToken.None);
            }

            return Result.Success();
        }
    }
}
