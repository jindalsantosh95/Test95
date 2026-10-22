using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Query.InflightOrders
{
    public class OrderAssociationResponseQueryHandler : IRequestHandler<OrderAssociationResponseQuery, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderAssociationResponseQueryHandler> _logger;
        public OrderAssociationResponseQueryHandler(IMediator mediator, ILogger<OrderAssociationResponseQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<string>>> Handle(OrderAssociationResponseQuery request, CancellationToken cancellationToken)
        {

            Response<string> orderAssociationResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))

                {
                    _logger.LogInformation($"Retry Count:- {retryCounter}");

                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.Correlationid, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            orderAssociationResponse = new Response<string>()
                            {
                                Status = Status.OK,
                                Value = result.JsonResponseData
                            };
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            break;
                    }

                    retryCounter++;
                }

                return orderAssociationResponse.Value != null ? Result.Success(orderAssociationResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<string>>(ex.Message);
            }
        }

    }
}
