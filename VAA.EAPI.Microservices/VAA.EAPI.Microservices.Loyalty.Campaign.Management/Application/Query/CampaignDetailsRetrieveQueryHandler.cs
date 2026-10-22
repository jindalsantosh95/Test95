using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignDetailsRetrieveQueryHandler : IRequestHandler<CampaignDetailsRetrieveQuery, Result<Response<CampaignRetrieveResponse>>>
    {
        private readonly ILogger<CampaignDetailsRetrieveQueryHandler> _logger;
        private readonly IReportingDBServices _reportingDBServices;
        private readonly IMediator _mediator;

        public CampaignDetailsRetrieveQueryHandler(IMediator mediator, ILogger<CampaignDetailsRetrieveQueryHandler> logger, IReportingDBServices reportingDBServices)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reportingDBServices = reportingDBServices ?? throw new ArgumentNullException(nameof(reportingDBServices));
        }

        public async Task<Result<Response<CampaignRetrieveResponse>>> Handle(CampaignDetailsRetrieveQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var campaignRetrieveRequestValidator = new CampaignRetrieveRequestValidator().Validate(request);
                if (campaignRetrieveRequestValidator.IsValid)
                {

                    Result<Response<CampaignSearchResponse>> searchCampaign = await _mediator.Send(request: new CampaignSearchQuery { CampaignId = request.CampaignId, CampaignCode = request.CampaignCode });

                    if (searchCampaign.IsSuccess && searchCampaign.Value.Value != null)
                    {
                        CampaignData campaignDetails = searchCampaign.Value.Value.Campaigns.FirstOrDefault();

                        return new Response<CampaignRetrieveResponse>
                        {
                            Status = Status.OK,
                            Value = new CampaignRetrieveResponse()
                            {
                                Campaign = campaignDetails,
                                AssociatedMemberCount = await _reportingDBServices.GetAssociatedMemberCount(campaignDetails.CampaignCode)
                            },
                            IgnoreNullValues = false
                        };
                    }
                    else
                    {
                        _logger.LogInformation("Campaign Retrieve from Reference DB is Failed");
                        return Result.Failure<Response<CampaignRetrieveResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                    }
                }
                else
                {
                    return Result.Success(new Response<CampaignRetrieveResponse>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Campaign Retrieve Response Query Handler");
                return Result.Failure<Response<CampaignRetrieveResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
