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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignCreateResponseQuery : IRequest<Result<Response<CampaignCreateResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }

    public class CampaignCreateResponseQueryHandler : IRequestHandler<CampaignCreateResponseQuery, Result<Response<CampaignCreateResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignCreateResponseQueryHandler> _logger;
        public CampaignCreateResponseQueryHandler(IMediator mediator, ILogger<CampaignCreateResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<CampaignCreateResponse>>> Handle(CampaignCreateResponseQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Campaign Create ({request.CorrelationId}) Response Query Handler - Start");
            Response<CampaignCreateResponse> CampaignCreateResponse = new();
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
                            CampaignCreateResponse = CampaignCreateQueryResponseMapping.CampaignCreateQueryResponseMapper(result);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Success(new Response<CampaignCreateResponse>
                            {
                                Errors = CampaignHelper.GetErrorDetail(result.JsonResponseData),
                                Status = Status.BusinessException
                            });
                    }
                    retryCounter++;
                }
                return CampaignCreateResponse.Value != null ? Result.Success(CampaignCreateResponse) : Result.Failure<Response<CampaignCreateResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Campaign Create Response Query Handler with CorrelationId:{request.CorrelationId}");
                return Result.Failure<Response<CampaignCreateResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"Campaign Create ({request.CorrelationId}) Response Query Handler - End");
            }
        }
    }
}
