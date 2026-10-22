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
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore
{
    public class NDCOrderStoreRequestCommandHandler : IRequestHandler<NDCOrderStoreRequestCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NDCOrderStoreRequestCommandHandler> _logger;

        public NDCOrderStoreRequestCommandHandler(IMediator mediator, ILogger<NDCOrderStoreRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(NDCOrderStoreRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderDoc = Json.Deserialize<OrderDocument>(Convert.ToString(request.Data));

                await _mediator.Send(new CosmosRepoCommand(new CosmosDocumentEvent { Document = orderDoc, ModificationType = CosmosModificationType.Upsert }, true), cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosException)
                {
                    _logger.LogError(ex, "NDCOrderStore Cosmos DB upsert failed with error code {ErrorCode}, status {StatusCode} for event {EventId}",
                        ErrorCodes.CosmosErorrCode, (int)cosmosException.StatusCode, request?.Id);
                }
                else
                {
                    _logger.LogError(ex, "NDCOrderStore failed with error code {ErrorCode}, exception type {ExceptionType} for event {EventId}",
                        ErrorCodes.CosmosErorrCode, ex.GetType().Name, request?.Id);
                }
                throw;
            }

            return Unit.Value;
        }
    }
}
