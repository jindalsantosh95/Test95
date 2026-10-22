using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands
{
    public class OrderCreateCommandHandler : IRequestHandler<OrderCreateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateCommandHandler> _logger;

        public OrderCreateCommandHandler(IMediator mediator, ILogger<OrderCreateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(OrderCreateCommand request, CancellationToken cancellationToken)
        {
            var id = string.Empty;
            var publishFailureLogged = false;
            var rollbackCompleted = false;

            try
            {
                if (request.OrderCreateRequest == null) throw new ArgumentNullException(nameof(request.OrderCreateRequest));

                var orderCreateCloudEvent = OrderCreateDataMapper.EventMapper(request.OrderCreateRequest);

                id = orderCreateCloudEvent.Id;

                await _mediator.Send(new EventRequestInsertCommand { Id = orderCreateCloudEvent.Id, Type = orderCreateCloudEvent.Type, Data = request.OrderCreateRequest?.SelectedOffers, Subject = orderCreateCloudEvent.Subject }, cancellationToken);

                try
                {
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = orderCreateCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);
                }
                catch (Exception publishException)
                {
                    publishFailureLogged = true;

                    _logger.LogError(publishException, "OrderCreate event publication failed; rolling back the event-store request. {EventId}", id);

                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = orderCreateCloudEvent.Id, Type = orderCreateCloudEvent.Type, Data = orderCreateCloudEvent.Data, Subject = orderCreateCloudEvent.Subject }, cancellationToken);
                    rollbackCompleted = true;

                    throw;
                }

                var response = OrderCreateDataMapper.ResponseMapper(orderCreateCloudEvent.Id);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                if (!publishFailureLogged)
                {
                    _logger.LogError(ex, "OrderCreate command failed. {EventId}", id);
                }
                else if (!rollbackCompleted)
                {
                    _logger.LogError(ex, "OrderCreate event-store rollback failed after publication failure. {EventId}", id);
                }

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}