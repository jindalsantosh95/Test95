using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class PromotionCampaignSearchQueryHandler : IRequestHandler<PromotionCampaignSearchQuery, Result<Response<PromotionCampaignSearchResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionCampaignSearchQueryHandler> _logger;
        private readonly IValidator<PromotionCampaignSearchRequestData> _validator;
        public PromotionCampaignSearchQueryHandler(IMediator mediator, ILogger<PromotionCampaignSearchQueryHandler> logger, IValidator<PromotionCampaignSearchRequestData> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }
        public async Task<Result<Response<PromotionCampaignSearchResponse>>> Handle(PromotionCampaignSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("PromotionCampaignSearchResponseQueryHandler started");
                if (_validator.Validate(request).IsValid)
                {
                    var promotionCampaignSearchResponse = await _mediator.Send(new GetPromotionCampaignCommand { PromotionId = request.PromotionId });
                    return promotionCampaignSearchResponse;
                }
                else
                {
                    return Result.Failure<Response<PromotionCampaignSearchResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Promotion Search Response Query Handler with CorrelationId:{request.PromotionId}");
                return Result.Failure<Response<PromotionCampaignSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
