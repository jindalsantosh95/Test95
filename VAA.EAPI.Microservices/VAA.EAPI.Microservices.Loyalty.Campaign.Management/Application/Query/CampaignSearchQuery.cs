using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignSearchQuery : CampaignSearchRequestData, IRequest<Result<Response<CampaignSearchResponse>>>
    {
    }

    public class CampaignSearchResponseQueryHandler : IRequestHandler<CampaignSearchQuery, Result<Response<CampaignSearchResponse>>>
    {
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private readonly ILogger<CampaignSearchResponseQueryHandler> _logger;

        public CampaignSearchResponseQueryHandler(ICommonRefDBRepository commonRefDBRepository, ILogger<CampaignSearchResponseQueryHandler> logger)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CampaignSearchResponse>>> Handle(CampaignSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _commonRefDBRepository.SearchCampaigns(request.CampaignId, request.CampaignName, request.StartDateF, request.EndDateF, request.StartDateT, request.EndDateT, request.CampaignCode, request.Status, request.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Campaign Search Response Query Handler with CorrelationId:{AsyncContext.Current.CorrelationId}");
                return Result.Failure<Response<CampaignSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
