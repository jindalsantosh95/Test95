using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignTypeRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignTypeRetrievalQuery : IRequest<Result<Response<CampaignTypeResponse>>>
    {

    }

    public class CampaignTypeRetrievalResponseQueryHandler : IRequestHandler<CampaignTypeRetrievalQuery, Result<Response<CampaignTypeResponse>>>
    {
        private readonly ILogger<CampaignTypeRetrievalResponseQueryHandler> _logger;
        private readonly IMediator _mediator;
        public CampaignTypeRetrievalResponseQueryHandler(IMediator mediator, ILogger<CampaignTypeRetrievalResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<CampaignTypeResponse>>> Handle(CampaignTypeRetrievalQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Campaign Type Retrieval Query handler process started");

            try
            {
                var campaignTypeResponse = await _mediator.Send(new CampaignTypesRetrieveCommand { });
                return campaignTypeResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in Campaign Type Retrieval Query handler");
                return Result.Failure<Response<CampaignTypeResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation("Campaign Type Retrieval Query handler process ended");
            }
        }
    }
}
