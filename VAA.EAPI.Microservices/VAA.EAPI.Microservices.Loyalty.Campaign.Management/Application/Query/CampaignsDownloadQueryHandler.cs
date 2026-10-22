using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignsDownloadQueryHandler : IRequestHandler<CampaignsDownloadQuery, Result<Response<CampaignsDownloadResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ICampaignListDownloadService _campaignListDownloadService;
        private readonly ILogger<CampaignsDownloadQueryHandler> _logger;

        public CampaignsDownloadQueryHandler(IMediator mediator, ICampaignListDownloadService campaignListDownloadService, ILogger<CampaignsDownloadQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _campaignListDownloadService = campaignListDownloadService ?? throw new ArgumentNullException(nameof(campaignListDownloadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CampaignsDownloadResponse>>> Handle(CampaignsDownloadQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Result<Response<CampaignSearchResponse>> searchCampaign = await _mediator.Send(request: new CampaignSearchQuery { });

                if (searchCampaign.IsSuccess && searchCampaign.Value.Value != null)
                {
                    return new Response<CampaignsDownloadResponse>
                    {
                        Status = Status.OK,
                        Value = new CampaignsDownloadResponse { CampaignsFileURL = await _campaignListDownloadService.CreateXlStatementAndUploadToBlob(searchCampaign.Value.Value) },
                        IgnoreNullValues = false
                    };
                }
                else
                {
                    _logger.LogInformation("Download Campaigns Query Handler is Failed");
                    return Result.Failure<Response<CampaignsDownloadResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Download Campaigns Query Handler");
                return new Response<CampaignsDownloadResponse>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath),
                    IgnoreNullValues = false
                };
            }
        }
    }
}
