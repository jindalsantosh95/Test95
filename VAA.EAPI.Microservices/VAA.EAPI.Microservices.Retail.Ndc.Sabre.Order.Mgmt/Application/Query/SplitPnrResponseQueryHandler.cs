using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query
{
    public class SplitPnrResponseQueryHandler : ISplitPnrResponseQuery
    {

        private readonly ILogger<SplitPnrResponseQueryHandler> _logger;
        private readonly ServiceResponseQueryHandler _serviceResponseQueryHandler;

        public SplitPnrResponseQueryHandler(ILogger<SplitPnrResponseQueryHandler> logger, ServiceResponseQueryHandler serviceResponseQueryHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceResponseQueryHandler = serviceResponseQueryHandler ?? throw new ArgumentNullException(nameof(serviceResponseQueryHandler));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(string CorrelationId, string Type, CancellationToken cancellationToken)
        {
            Response<OrderDocument> splitPnrResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _serviceResponseQueryHandler.Handle(new ServiceResponseQuery { CorrelationId = CorrelationId, EventType = Type }, cancellationToken);
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS);

                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:

                            await Task.Delay(timespan);

                            break;
                        case ServiceResponseProcessStatus.Completed:

                            splitPnrResponse = SplitPnrQueryResponseMapper.QueryResponseMapper(result);
                            responseProcessed = true;

                            break;
                        case ServiceResponseProcessStatus.Error:

                            responseProcessed = true;

                            return Result.Failure<Response<OrderDocument>>(result.JsonResponseData);
                    }

                    retryCounter++;
                }

                return splitPnrResponse.Value != null ? Result.Success(splitPnrResponse) : Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch
            {
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
