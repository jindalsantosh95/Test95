using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query
{
    ///<summary>
    ///OrderPaymentStatusResponse is not in use as per user story -  296010
    ///</summary>
    public class OrderPaymentStatusResponseQueryHandler : IRequestHandler<OrderPaymentStatusResponseQuery, Result<Response<OrderPaymentResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaymentStatusResponseQueryHandler> _logger;
        public OrderPaymentStatusResponseQueryHandler(IMediator mediator, ILogger<OrderPaymentStatusResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderPaymentResponse>>> Handle(OrderPaymentStatusResponseQuery request, CancellationToken cancellationToken)
        {
            var orderPaymentResponse = new Response<OrderPaymentResponse>();
            bool success = false;

            try
            {
                var mockEnabled = ConfigReader.Try(Constants.Environment.MockOrderPaymentService, false);

                if (mockEnabled)
                    return await OrderPaymentResponseQueryMock(request);

                var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type }, cancellationToken);

                switch (result.Status)
                {
                    case ServiceResponseProcessStatus.UpdatePending:
                    case ServiceResponseProcessStatus.Processing:
                        orderPaymentResponse = OrderPaymentQueryResponseMapper.OrderPaymentStatusResponseMapper(request.CorrelationId, Status.Accepted);
                        success = true;
                        break;
                    case ServiceResponseProcessStatus.Completed:
                        orderPaymentResponse = OrderPaymentQueryResponseMapper.OrderPaymentStatusQueryResponseMapper(result);
                        success = true;
                        break;
                    case ServiceResponseProcessStatus.Error:
                        return Result.Failure<Response<OrderPaymentResponse>>(result.JsonResponseData);
                }

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.CorrelationId, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return success ? Result.Success(orderPaymentResponse) : Result.Failure<Response<OrderPaymentResponse>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderPaymentResponse query handler for for CorrelationId: {request.CorrelationId}");

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.CorrelationId, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return Result.Failure<Response<OrderPaymentResponse>>(Json.Serialize(errorResponse));
            }
        }
        private static async Task<Result<Response<OrderPaymentResponse>>> OrderPaymentResponseQueryMock(OrderPaymentStatusResponseQuery request)
        {
            var jsonResponseData = new OrderPaymentResponse
            {
                CorrelationId = request.CorrelationId,
                Status = ServiceResponseProcessStatus.Completed.ToString(),
                Data = null
            };

            var result = new ServiceResponse
            {
                Id = request.CorrelationId,
                Status = ServiceResponseProcessStatus.Completed,
                Type = Constants.Application.OrderPaymentUpdateEventSubject,
                JsonResponseData = Json.Serialize(jsonResponseData)
            };

            var orderPaymentResponse = OrderPaymentQueryResponseMapper.OrderPaymentStatusQueryResponseMapper(result);
            return await Task.FromResult(Result.Success(orderPaymentResponse));
        }
    }
}