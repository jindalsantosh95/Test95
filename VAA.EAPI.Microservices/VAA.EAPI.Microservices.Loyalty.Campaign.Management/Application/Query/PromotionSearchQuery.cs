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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class PromotionSearchQuery : PromotionSearchRequestData, IRequest<Result<Response<PromotionSearchResponse>>>
    {
    }
    public class PromotionSearchResponseQueryHandler : IRequestHandler<PromotionSearchQuery, Result<Response<PromotionSearchResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionSearchResponseQueryHandler> _logger;
        public PromotionSearchResponseQueryHandler(IMediator mediator, ILogger<PromotionSearchResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<PromotionSearchResponse>>> Handle(PromotionSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("PromotionSearchResponseQueryHandler started");

                return await _mediator.Send(new GetPromotionCommand { PromotionId = request.PromotionId, PromotionName = request.PromotionName, StartDateF = request.StartDateF, EndDateF = request.EndDateF, StartDateT = request.StartDateT, EndDateT = request.EndDateT, PartnerName = request.PartnerName, Active = request.Active });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Promotion Search Response Query Handler with CorrelationId:{AsyncContext.Current.CorrelationId}");
                return Result.Failure<Response<PromotionSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
