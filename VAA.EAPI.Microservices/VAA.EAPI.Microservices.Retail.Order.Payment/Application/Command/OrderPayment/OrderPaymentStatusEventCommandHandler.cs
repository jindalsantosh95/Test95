using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentStatusEventCommandHandler : IRequestHandler<OrderPaymentStatusEventCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaymentStatusEventCommandHandler> _logger;
        public OrderPaymentStatusEventCommandHandler(IMediator mediator, ILogger<OrderPaymentStatusEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(OrderPaymentStatusEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"OrderPaymentStatus Event Subscribe Handler Start for Id: {request.Id}");
                var orderPaymentStatusRequestData = Json.Deserialize<OrderPaymentStatusRequest>(Convert.ToString(request.Data));

                var responseStatus = orderPaymentStatusRequestData?.Status != PaymentUpdateStatus.Cancelled.ToString() ? ServiceResponseProcessStatus.Completed : ServiceResponseProcessStatus.Error;
                object result = orderPaymentStatusRequestData?.Status != PaymentUpdateStatus.Cancelled.ToString()
                                 ? OrderPaymentResponseMapper.ResponseMapper(orderPaymentStatusRequestData, request.Id)
                                 : ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                _logger.LogDebug($"Store Service Response in response store - Start");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = responseStatus, Type = Constants.Application.OrderPaymentUpdateEventSubject }, cancellationToken);
                _logger.LogDebug($"Store Service Response in response store - End");

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured in OrderPaymentStatus Event Subscribe Command Handler for CorrelationId : {request.Id}");
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderPaymentUpdateEventSubject }, cancellationToken);
                throw;
            }
        }
    }
}
