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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate
{
    public class PromotionCreateEventProcessCommandHandler : IRequestHandler<PromotionCreateEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionCreateEventProcessCommandHandler> _logger;

        public PromotionCreateEventProcessCommandHandler(IMediator mediator, ILogger<PromotionCreateEventProcessCommandHandler> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(PromotionCreateEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("PromotionCreateEventProcessCommandHandler started");
                PromotionCreateRequestData promotionCreateRequest = Json.Deserialize<PromotionCreateRequestData>(Convert.ToString(request.Data));
                var promotionCreateResponse = await _mediator.Send(new PromotionCreateCommand { PromotionName = promotionCreateRequest.PromotionName, PartnerName = promotionCreateRequest.PartnerName, StartDate = promotionCreateRequest.StartDate, EndDate = promotionCreateRequest.EndDate, Active = promotionCreateRequest.Active, InitiatedBy = promotionCreateRequest.InitiatedBy });
                if (promotionCreateResponse.Value.Value.Equals(Constants.Application.Success))
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.PromotionCreateSuccessMessage
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreatePromotion.ToString())) });
                    return result;
                }
                else
                {
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = promotionCreateResponse.Value.Value, Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreatePromotion.ToString())) });
                    return Result.Failure<Response<string>>(promotionCreateResponse.Value.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PromotionCreateEventProcessCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation("PromotionUpdateEventProcessCommandHandler completed");
            }
        }
    }
}
