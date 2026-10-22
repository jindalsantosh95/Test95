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
using VAA.EAPI.Microservices.Retail.Order.Create.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Application.Query
{
    public class OrderCreateResponseQueryHandler : IRequestHandler<OrderCreateResponseQuery, Result<Response<OrderResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateResponseQueryHandler> _logger;

        public OrderCreateResponseQueryHandler(IMediator mediator, ILogger<OrderCreateResponseQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Response<OrderResponse>>> Handle(OrderCreateResponseQuery request, CancellationToken cancellationToken)
        {
            Response<OrderResponse> orderResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan, cancellationToken);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            orderResponse = Json.Deserialize<Result<Response<OrderResponse>>>(result.JsonResponseData).Value;
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Failure<Response<OrderResponse>>(result.JsonResponseData);
                    }

                    retryCounter++;
                }
                if (!responseProcessed)
                    _logger.LogWarning("Order create response polling exhausted for {EventId} after {AttemptCount} attempts", request?.CorrelationId, retryCounter - 1);
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return orderResponse.Value != null || orderResponse.Errors != null ? Result.Success(orderResponse) : Result.Failure<Response<OrderResponse>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order create response retrieval failed for {CorrelationId}", request?.CorrelationId);
                return Result.Failure<Response<OrderResponse>>(ex.Message);

            }
        }
    }
}
