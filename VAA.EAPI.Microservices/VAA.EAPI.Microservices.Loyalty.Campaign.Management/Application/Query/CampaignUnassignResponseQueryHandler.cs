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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignUnassignResponseQueryHandler : IRequestHandler<CampaignUnassignResponseQuery, Result<Response<CampaignUnassignResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignUnassignResponseQueryHandler> _logger;
        public CampaignUnassignResponseQueryHandler(IMediator mediator, ILogger<CampaignUnassignResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CampaignUnassignResponse>>> Handle(CampaignUnassignResponseQuery request, CancellationToken cancellationToken)
        {
            Response<CampaignUnassignResponse> campaignUnassignResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    _logger.LogInformation($"Retry Count:- {retryCounter}");
                    ServiceResponse result = await _mediator.Send(new ServiceResponseQuery
                    {
                        ConnectionString = ConfigReader.Get(Constants.Environment.CustomerTransactionMicroServiceDBKey),
                        TableName = ConfigReader.Get(Constants.Environment.CustomerTransactionMicroServiceResponseTableName),
                        CorrelationId = request.CorrelationId,
                        EventType = request.Type
                    });

                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            campaignUnassignResponse = CampaignUnassignQueryResponseMapping.CampaignUnassignQueryResponseMapper(result, request.CorrelationId);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Success(new Response<CampaignUnassignResponse>
                            {
                                Errors = CampaignHelper.GetErrorDetail(result.JsonResponseData),
                                Status = Status.BusinessException
                            });
                    }
                    retryCounter++;
                }
                return campaignUnassignResponse.Value != null ? Result.Success(campaignUnassignResponse) : Result.Failure<Response<CampaignUnassignResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Campaign Unassign Response Query Handler with CorrelationId:{request.CorrelationId}");
                return Result.Failure<Response<CampaignUnassignResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
