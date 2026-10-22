using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.Seat
{
    public class OrderSeatFulfilmentRequestCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
    }

    public class OrderSeatFulfilmentRequestCommandHandler : IRequestHandler<OrderSeatFulfilmentRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;

        public OrderSeatFulfilmentRequestCommandHandler(IMediator mediator, ILogger<OrderSeatFulfilmentRequestCommandHandler> logger, ICachedChannel cachedChannel)
        {
            _mediator = mediator;
        }

        public async Task<Result<Response<string>>> Handle(OrderSeatFulfilmentRequestCommand request, CancellationToken cancellationToken)
        {
            Response<string> orderChangeResponse = new();

            try
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = "", Status = ServiceResponseProcessStatus.Processing, Type = request.Type }, cancellationToken);

                var seatFulfillmentRs = Json.Deserialize<SeatFulfilmentResponse>(request.Data.ToString());

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(OrderChangeSeatResponseMapper.ResponseMapper(seatFulfillmentRs)), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);

                return orderChangeResponse;
            }
            catch (Exception ex)
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = request.Type }, cancellationToken);

                return Result.Failure<Response<string>>(ex.Message);
            }
        }
    }
}
