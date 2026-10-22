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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignCommandResponseQuery : IRequest<Result<Response<CampaignCommandResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }

    public class CampaignCommandResponseQueryHandler : IRequestHandler<CampaignCommandResponseQuery, Result<Response<CampaignCommandResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignCommandResponseQueryHandler> _logger;
        public CampaignCommandResponseQueryHandler(IMediator mediator, ILogger<CampaignCommandResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<CampaignCommandResponse>>> Handle(CampaignCommandResponseQuery request, CancellationToken cancellationToken)
        {
            Response<CampaignCommandResponse> campaignCommandResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    _logger.LogInformation($"Retry Count:- {retryCounter}");
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            campaignCommandResponse = CampaignCommandQueryResponseMapping.CampaignCommandQueryResponseMapper(result);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Success(new Response<CampaignCommandResponse>
                            {
                                Errors = CampaignHelper.GetErrorDetail(result.JsonResponseData),
                                Status = Status.BusinessException
                            });
                    }
                    retryCounter++;
                }
                return campaignCommandResponse.Value != null ? Result.Success(campaignCommandResponse) : Result.Failure<Response<CampaignCommandResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Link Campaign to Promotion Response Query Handler with CorrelationId:{request.CorrelationId}");
                return Result.Failure<Response<CampaignCommandResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
