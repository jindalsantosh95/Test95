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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionLinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionLinking
{
    public class CampaignPromotionLinkEventProcessCommandHandler : IRequestHandler<CampaignPromotionLinkEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignPromotionLinkEventProcessCommandHandler> _logger;
        public CampaignPromotionLinkEventProcessCommandHandler(IMediator mediator, ILogger<CampaignPromotionLinkEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(CampaignPromotionLinkEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("CampaignPromotionLinkEventProcessCommandHandler started");
                CampaignPromotionLinkRequestData campaignPromotionLinkRequest = Json.Deserialize<CampaignPromotionLinkRequestData>(Convert.ToString(request.Data));
                var campaignPromotionLinkResponse = await _mediator.Send(new InsertCampaignPromotionLinkCommand { CampaignId = campaignPromotionLinkRequest.CampaignId, PromotionId = campaignPromotionLinkRequest.PromotionId, CreatedBy = campaignPromotionLinkRequest.CreatedBy });
                if (campaignPromotionLinkResponse.Value.Value.Equals(Constants.Application.Success))
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.PromotionLinkSuccessMessage
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.LinkCampaignPromotion.ToString())) });
                    return result;
                }
                else
                {
                    _logger.LogInformation("LinkCampaignPromotion event processing handler failed with error");
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = campaignPromotionLinkResponse.Value.Value, Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.LinkCampaignPromotion.ToString())) });
                    return Result.Failure<Response<string>>(campaignPromotionLinkResponse.Value.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CampaignPromotionLinkEventProcessCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation("CampaignPromotionLinkEventProcessCommandHandler completed");
            }
        }
    }
}
