using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionDelinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking
{
    public class CampaignPromotionDelinkEventProcessCommandHandler : IRequestHandler<CampaignPromotionDelinkEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignPromotionDelinkEventProcessCommandHandler> _logger;
        public CampaignPromotionDelinkEventProcessCommandHandler(IMediator mediator, ILogger<CampaignPromotionDelinkEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(CampaignPromotionDelinkEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                CampaignPromotionDelinkRequestData campaignPromotionDelinkRequest = Json.Deserialize<CampaignPromotionDelinkRequestData>(Convert.ToString(request.Data));
                var campaignPromotionDelinkResponse = await _mediator.Send(new DeleteCampaignPromotionLinkCommand { CampaignId = campaignPromotionDelinkRequest.CampaignId, PromotionId = campaignPromotionDelinkRequest.PromotionId });
                if (campaignPromotionDelinkResponse.Value.Value.Equals(Constants.Application.Success))
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.PromotionDelinkSuccessMessage
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), Convert.ToString(CampaignCommandType.DelinkCampaignPromotion))) });
                    return result;
                }
                else
                {
                    _logger.LogInformation("DelinkCampaignPromotion event processing handler failed with error");
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = campaignPromotionDelinkResponse.Value.Value, Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), Convert.ToString(CampaignCommandType.DelinkCampaignPromotion))) });
                    return Result.Failure<Response<string>>(campaignPromotionDelinkResponse.Value.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CampaignPromotionDelinkEventProcessCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
