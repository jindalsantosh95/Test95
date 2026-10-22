using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Notification.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Application.Query
{
    ///<summary>
    ///OrderNotificationStatus is not in use as per user story -  296010
    ///</summary>
    public class OrderNotificationStatusQueryHandler : IRequestHandler<OrderNotificationStatusQuery, Result<Response<OrderNotificationStatusResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderNotificationStatusQueryHandler> _logger;
        public OrderNotificationStatusQueryHandler(IMediator mediator, ILogger<OrderNotificationStatusQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Response<OrderNotificationStatusResponse>>> Handle(OrderNotificationStatusQuery request, CancellationToken cancellationToken)
        {
            Response<OrderNotificationStatusResponse> orderNotificationStatusResponse = new();
            string correlationId = request?.CorrelationId;
            bool responseProcessed = false;
            try
            {
                if (!(ConfigReader.Get(Constants.Environment.MockResponseEnabled) == "Y"))
                {
                    int retryCounter = 1;
                    int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                    while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))

                    {
                        var result = await _mediator.Send
                        (
                            request: new ServiceResponseQuery
                            {
                                CorrelationId = request.CorrelationId,
                                EventType = request.Type
                            },
                            cancellationToken: cancellationToken
                      );
                        var timespan = TimeSpan.FromMilliseconds(value: responseDelayTimeInMS * retryCounter);
                        switch (result.Status)
                        {

                            case ServiceResponseProcessStatus.UpdatePending:
                            case ServiceResponseProcessStatus.Processing:
                                await Task.Delay(delay: timespan, cancellationToken: cancellationToken);
                                break;
                            case ServiceResponseProcessStatus.Completed:
                                orderNotificationStatusResponse = OrderNotificationStatusResponseMapper.ResponseMapper(serviceResponse: result);
                                responseProcessed = true;
                                break;

                            case ServiceResponseProcessStatus.Error:
                                return Result.Failure<Response<OrderNotificationStatusResponse>>(error: result.JsonResponseData);
                        }
                        retryCounter++;
                    }
                    var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                        (
                         errorCode: Infrastructure.Constants.Constants.TechnicalError,
                         guid: request.CorrelationId,
                         errorFilePath: Infrastructure.Constants.Constants.GlobalExceptionFilePath
                        );
                    return responseProcessed ? Result.Success(orderNotificationStatusResponse) : Result.Failure<Response<OrderNotificationStatusResponse>>(Json.Serialize(instance: errorResponse));
                }
                else
                {
                    //HardCoded Mock Response
                    orderNotificationStatusResponse.Status = Status.OK;
                    orderNotificationStatusResponse.Value = new OrderNotificationStatusResponse()
                    {
                        Payload = null,
                        CorrelationId = request.CorrelationId,
                        Status = "Succeeded"
                    };
                    return orderNotificationStatusResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OrderNotificationStatusQueryHandler Error | ErrorId: {correlationId}");

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                    (
                       errorCode: Infrastructure.Constants.Constants.TechnicalError,
                       guid: correlationId,
                       errorFilePath: Infrastructure.Constants.Constants.GlobalExceptionFilePath
                    );
                return Result.Failure<Response<OrderNotificationStatusResponse>>(Json.Serialize(instance: errorResponse));
            }
        }
    }
}
