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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate
{
    public class PromotionUpdateEventProcessCommandHandler : IRequestHandler<PromotionUpdateEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionUpdateEventProcessCommandHandler> _logger;

        public PromotionUpdateEventProcessCommandHandler(IMediator mediator, ILogger<PromotionUpdateEventProcessCommandHandler> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(PromotionUpdateEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("PromotionUpdateEventProcessCommandHandler started");
                PromotionUpdateRequestData promotionUpdateRequest = Json.Deserialize<PromotionUpdateRequestData>(Convert.ToString(request.Data));
                var promotionUpdateResponse = await _mediator.Send(new PromotionUpdateCommand { PromotionId = promotionUpdateRequest.PromotionId, PromotionName = promotionUpdateRequest.PromotionName, PartnerName = promotionUpdateRequest.PartnerName, StartDate = promotionUpdateRequest.StartDate, EndDate = promotionUpdateRequest.EndDate, Active = promotionUpdateRequest.Active, CampaignLinked = promotionUpdateRequest.CampaignLinked, InitiatedBy = promotionUpdateRequest.InitiatedBy });
                if (promotionUpdateResponse.Value.Value.Equals(Constants.Application.Success))
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.PromotionUpdateSuccessMessage
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdatePromotion.ToString())) });
                    return result;
                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = promotionUpdateResponse.Value.Value, Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdatePromotion.ToString())) });
                    return Result.Failure<Response<string>>(promotionUpdateResponse.Value.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PromotionUpdateEventProcessCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation("PromotionUpdateEventProcessCommandHandler completed");
            }
        }
    }
}
