using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderStore
{
    public class OrderStoreRequestCommandHandler : IRequestHandler<OrderStoreRequestCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderStoreRequestCommandHandler> _logger;

        public OrderStoreRequestCommandHandler(IMediator mediator, ILogger<OrderStoreRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(OrderStoreRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderDoc = Json.Deserialize<OrderDocument>(Convert.ToString(request.Data));

                await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = orderDoc, ModificationType = CosmosModificationType.Create }), cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosException)
                {
                    _logger.LogError(ex, "OrderStore Cosmos DB create failed with error code {ErrorCode}, status {StatusCode} for event {EventId}",
                        ErrorCodes.CosmosErorrCode, (int)cosmosException.StatusCode, request?.Id);
                }
                else
                {
                    _logger.LogError(ex, "OrderStore failed with exception type {ExceptionType} for event {EventId}",
                        ex.GetType().Name, request?.Id);
                }
                throw;
            }
            return Unit.Value;
        }
    }
}
