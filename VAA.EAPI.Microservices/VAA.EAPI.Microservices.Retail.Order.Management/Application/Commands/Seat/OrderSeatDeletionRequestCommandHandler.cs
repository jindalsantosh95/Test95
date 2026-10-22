using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.Seat
{
    public class OrderSeatDeletionRequestCommandHandler(IMediator mediator) : IRequestHandler<OrderSeatDeletionRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator = mediator;

        public async Task<Result<Response<string>>> Handle(OrderSeatDeletionRequestCommand request, CancellationToken cancellationToken)
        {
            Response<string> orderChangeResponse = new();

            try
            {
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = "", Status = ServiceResponseProcessStatus.Processing, Type = request.Type }, cancellationToken);

                var response = Json.Deserialize<SeatDeletionResponse>(request.Data.ToString());

                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize((OrderChangeResponse)response), Status = ServiceResponseProcessStatus.Completed, Type = request.Type }, cancellationToken);

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
