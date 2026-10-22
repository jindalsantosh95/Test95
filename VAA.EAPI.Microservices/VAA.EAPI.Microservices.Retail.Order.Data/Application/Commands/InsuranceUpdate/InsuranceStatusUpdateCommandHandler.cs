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
    public class InsuranceStatusUpdateCommandHandler : IRequestHandler<InsuranceStatusUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InsuranceStatusUpdateCommandHandler> _logger;

        public InsuranceStatusUpdateCommandHandler(IMediator mediator, ILogger<InsuranceStatusUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(InsuranceStatusUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderDoc = Json.Deserialize<OrderDocument>(Convert.ToString(request.Data));

                await _mediator.Send(new CosmosUpdateRepoCommand(new CosmosDocumentUpdateEvent { Document = orderDoc  }, true), cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosException)
                {
                    _logger.LogError(ex, "InsuranceStatusUpdate Cosmos DB patch failed with error code {ErrorCode}, status {StatusCode} for correlation {CorrelationId}",
                        ErrorCodes.CosmosErorrCode, (int)cosmosException.StatusCode, AsyncContext.Current.CorrelationId);
                }
                else
                {
                    _logger.LogError(ex, "InsuranceStatusUpdate failed with exception type {ExceptionType} for correlation {CorrelationId}",
                        ex.GetType().Name, AsyncContext.Current.CorrelationId);
                }
                throw;

            }
            return Unit.Value;
        }
    }
}
