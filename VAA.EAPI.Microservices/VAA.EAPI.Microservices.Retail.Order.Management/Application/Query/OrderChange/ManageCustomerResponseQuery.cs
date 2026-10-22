using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Query.OrderChange
{
    public class ManageCustomerResponseQuery : IRequest<Result<Response<OrderView>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }

    public class ManageCustomerResponseQueryHandler : IRequestHandler<ManageCustomerResponseQuery, Result<Response<OrderView>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageCustomerResponseQueryHandler> _logger;
        public ManageCustomerResponseQueryHandler(IMediator mediator, ILogger<ManageCustomerResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<OrderView>>> Handle(ManageCustomerResponseQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"ManageCustomerResponse query-Start for Id {request.CorrelationId}");
            Response<OrderView> manageCustomerResponse = new();
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
                            manageCustomerResponse = ManageCustomerQueryResponseMapper.QueryResponseMapper(result);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Failure<Response<OrderView>>(result.JsonResponseData);
                    }
                    retryCounter++;
                }

                return manageCustomerResponse.Value != null ? Result.Success(manageCustomerResponse) : Result.Failure<Response<OrderView>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Manage Customer Response Query Handler for Co-relation id:{request.CorrelationId}");
                return Result.Failure<Response<OrderView>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"ManageCustomerResponse query-End");
            }
        }
    }
}
