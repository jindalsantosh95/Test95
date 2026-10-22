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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUpdate
{
    public class CampaignUpdateEventProcessCommandHandler : IRequestHandler<CampaignUpdateEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignUpdateEventProcessCommandHandler> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public CampaignUpdateEventProcessCommandHandler(IMediator mediator, ILogger<CampaignUpdateEventProcessCommandHandler> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public async Task<Result<Response<string>>> Handle(CampaignUpdateEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                CampaignUpdateRequestData campaignUpdateRequest = Json.Deserialize<CampaignUpdateRequestData>(Convert.ToString(request.Data));
                string campaignUpdateResponse = await _commonRefDBRepository.UpdateCampaign(campaignUpdateRequest.CampaignId, campaignUpdateRequest.CampaignCode, campaignUpdateRequest.StartDate, campaignUpdateRequest.EndDate, campaignUpdateRequest.CampaignName, campaignUpdateRequest.Description, campaignUpdateRequest.LinkToTemplate, campaignUpdateRequest.RegistrationRequiredFlag, campaignUpdateRequest.Status, campaignUpdateRequest.Type, campaignUpdateRequest.InitiatedBy);
                if (campaignUpdateResponse.Equals(Constants.Application.Success))
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.CampaignSuccessMessage
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateCampaign.ToString())) });
                    return result;
                }
                else
                {
                    var result = Result.Failure<Response<string>>(campaignUpdateResponse);
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = campaignUpdateResponse, Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateCampaign.ToString())) });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CampaignUpdateEventProcessCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
