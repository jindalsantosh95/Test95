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
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Query.OrderChange
{
    public class OrderChangeResponseQuery : IRequest<Result<Response<OrderChangeResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
    public class OrderChangeResponseQueryHandler : IRequestHandler<OrderChangeResponseQuery, Result<Response<OrderChangeResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeResponseQueryHandler> _logger;
        public OrderChangeResponseQueryHandler(IMediator mediator, ILogger<OrderChangeResponseQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<OrderChangeResponse>>> Handle(OrderChangeResponseQuery request, CancellationToken cancellationToken)
        {

            Response<OrderChangeResponse> orderChangeResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))

                {
                    _logger.LogDebug($"Retry Count:- {retryCounter}");
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);
                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            orderChangeResponse = OrderChangeQueryResponseMapping.OrderChangeQueryResponseMapper(result);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Failure<Response<OrderChangeResponse>>(result.JsonResponseData);
                    }
                    retryCounter++;
                }

                return orderChangeResponse.Value != null ? Result.Success(orderChangeResponse) : Result.Failure<Response<OrderChangeResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Order Change Response Query Handler with Co-relation id:{request.CorrelationId}");
                return Result.Failure<Response<OrderChangeResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

    }
}
