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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query
{
    public class ManageCustomerResponseQuery : IRequest<Result<Response<OrderDocument>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }

    public class ManageCustomerResponseQueryHandler : IRequestHandler<ManageCustomerResponseQuery, Result<Response<OrderDocument>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageCustomerResponseQueryHandler> _logger;

        public ManageCustomerResponseQueryHandler(IMediator mediator, ILogger<ManageCustomerResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<OrderDocument>>> Handle(ManageCustomerResponseQuery request, CancellationToken cancellationToken)
        {

            Response<OrderDocument> manageCustomerResponse = new();
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

                            manageCustomerResponse = ManageCustomerQueryResponseMapper.QueryResponseMapper(result);
                            responseProcessed = true;

                            break;
                        case ServiceResponseProcessStatus.Error:

                            responseProcessed = true;

                            return Result.Failure<Response<OrderDocument>>(result.JsonResponseData);
                    }

                    retryCounter++;
                }

                return manageCustomerResponse.Value != null ? Result.Success(manageCustomerResponse) : Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch
            {
                return Result.Failure<Response<OrderDocument>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            
        }
    }
}
