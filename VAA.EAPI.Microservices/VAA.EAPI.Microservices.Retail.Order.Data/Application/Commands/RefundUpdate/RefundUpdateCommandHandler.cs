using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.RefundUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.RefundUpdate
{
    public class RefundUpdateCommandHandler : IRequestHandler<RefundUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RefundUpdateCommandHandler> _logger;
        public RefundUpdateCommandHandler(IMediator mediator, ILogger<RefundUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(RefundUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));


                var voidUpdateRequest = Json.DeserializeObject<VoidOrderUpdateData>((JToken)request.Data);
                await _mediator.Send(new RefundUpdateRepoCommand(voidUpdateRequest), cancellationToken);
            }
            catch (CosmosDBException ex)
            {
                _logger.LogError(ex, "Refund update failed in Cosmos DB with error code {ErrorCode} for event {EventId}", ErrorCodes.CosmosErorrCode, request?.Id); throw;
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Refund update failed domain validation with error code {ErrorCode} for event {EventId}", ErrorCodes.OrderRefErrorCode, request?.Id); throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund update failed with error code {ErrorCode}, exception type {ExceptionType} for event {EventId}",
                    ErrorCodes.UnableToUpdate, ex.GetType().Name, request?.Id); throw;

            }
            return Unit.Value;
        }
    }
}
