using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query
{
    public class OrderCancelResponseQuery : IRequest<Result<Response<string>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }

    public class OrderCancelResponseQueryHandler : IRequestHandler<OrderCancelResponseQuery, Result<Response<string>>>
    {
        private readonly IMediator _mediator;

        public OrderCancelResponseQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<string>>> Handle(OrderCancelResponseQuery request, CancellationToken cancellationToken)
        {

            Response<string> orderCancelResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS);

                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:

                            await Task.Delay(timespan);

                            break;
                        case ServiceResponseProcessStatus.Completed:

                            orderCancelResponse =  new Response<string>
                            {
                                Status = Status.OK,                                
                            };
                            responseProcessed = true;

                            break;
                        case ServiceResponseProcessStatus.Error:

                            responseProcessed = true;

                            return Result.Failure<Response<string>>(result.JsonResponseData);
                    }

                    retryCounter++;
                }

                return orderCancelResponse.Status == Status.OK ? Result.Success(orderCancelResponse) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch
            {
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

        }
    }
}
