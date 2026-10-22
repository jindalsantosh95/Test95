using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Query.OrderCreate
{
    public class OrderCreateResponseQueryHandler : IRequestHandler<OrderCreateResponseQuery, Result<Response<OrderResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateResponseQueryHandler> _logger;

        public OrderCreateResponseQueryHandler(IMediator mediator, ILogger<OrderCreateResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderResponse>>> Handle(OrderCreateResponseQuery request, CancellationToken cancellationToken)
        {
            Response<OrderResponse> orderResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMs = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type }, cancellationToken);
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMs * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan, cancellationToken);
                            break;

                        case ServiceResponseProcessStatus.Completed:
                            orderResponse = new()
                            {
                                Status = Status.OK,
                                Value = Json.Deserialize<Result<Response<OrderResponse>>>(result.JsonResponseData).Value.Value
                            };
                            responseProcessed = true;
                            break;

                        case ServiceResponseProcessStatus.Error:
                            return Result.Failure<Response<OrderResponse>>(result.JsonResponseData);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    retryCounter++;
                }

                return orderResponse.Value != null ? Result.Success(orderResponse) : Result.Failure<Response<OrderResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occurred in Order Create Response Query Handler with Co-relation id:{request.CorrelationId}");
                return Result.Failure<Response<OrderResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
