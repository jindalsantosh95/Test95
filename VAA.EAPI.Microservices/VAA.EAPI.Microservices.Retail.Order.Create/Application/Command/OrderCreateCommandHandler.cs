using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Create.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Application.Command
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
        public async Task<Result<Response<string>>> Handle(OrderCreateCommand orderCreateCommand, CancellationToken cancellationToken)
        {
            var id = string.Empty;

            try
            {
                var orderCreateEvent = OrderCreateEventDataMapper.Mapper(orderCreateCommand.OrderCreateRequest);

                id = orderCreateEvent.Id;

                await _mediator.Send(new EventRequestInsertCommand { Id = orderCreateEvent.Id, Type = orderCreateEvent.Type, Data = orderCreateEvent.Data, Subject = orderCreateEvent.Subject }, cancellationToken);
                try
                {
                    await _mediator.Publish(orderCreateEvent, cancellationToken);
                }
                catch
                {
                    _logger.LogWarning("Publishing order-create event failed; rolling back event-store record for {EventId}", orderCreateEvent.Id);
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = orderCreateEvent.Id, Type = orderCreateEvent.Type, Data = orderCreateEvent.Data, Subject = orderCreateEvent.Subject }, cancellationToken);
                    throw;
                }

                var response = OrderCreateResponseMapper.ResponseMapper(orderCreateEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                return response != null ? Result.Success(response) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order create event request failed for {EventId}", id);
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                    (
                                        errorCode: EAPI.Infrastructure.Constants.Constants.TechnicalError,
                                        guid: id,
                                        errorFilePath: EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath
                                    );
                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}
