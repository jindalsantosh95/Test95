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
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Create.Domain;
using VAA.EAPI.Microservices.Retail.Order.Create.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Create.Constants;
using VAA.EAPI.Microservices.Retail.Order.Create.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Application.Command
{
    public class OrderCreateSubscriberCommandHandler : IRequestHandler<OrderCreateSubscriberCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateSubscriberCommandHandler> _logger;

        public OrderCreateSubscriberCommandHandler(IMediator mediator, ILogger<OrderCreateSubscriberCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(OrderCreateSubscriberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = "", Status = ServiceResponseProcessStatus.Processing, Type = request.Type }, cancellationToken);

                var orderCreateRequest = Json.DeserializeObject<OrderCreateRequest>((JToken)request.Data);
                var result = await _mediator.Send(new OrderCreateRepoCommand(orderCreateRequest), cancellationToken);

                var orderResponse = new OrderResponse()
                {
                    BookingReference = result
                };

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(OrderCreateEventDataMapper.ResponseMapper(orderResponse)), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError("Delta order create failed for {EventId}", request?.Id);

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)), Status = ServiceResponseProcessStatus.Error, Type = request.Type }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order create event processing failed for {EventId}", request?.Id);

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);

            }

            return Unit.Value;
        }
    }
}
