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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignCreate
{
    public class CampaignCreateEventProcessCommandHandler : IRequestHandler<CampaignCreateEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignCreateEventProcessCommandHandler> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public CampaignCreateEventProcessCommandHandler(IMediator mediator, ILogger<CampaignCreateEventProcessCommandHandler> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<Result<Response<string>>> Handle(CampaignCreateEventProcessCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Campaign Create Event Handler ({request.Id}) - Start");

            try
            {
                CampaignCreateRequestData campaignCreateRequest = Json.Deserialize<CampaignCreateRequestData>(Convert.ToString(request.Data));
                string campaignCreateResponse = await _commonRefDBRepository.CreateCampaign(campaignCreateRequest.CampaignCode, campaignCreateRequest.StartDate, campaignCreateRequest.EndDate, campaignCreateRequest.CampaignName, campaignCreateRequest.Description, campaignCreateRequest.LinkToTemplate, campaignCreateRequest.RegistrationRequiredFlag, campaignCreateRequest.Status, campaignCreateRequest.Type, campaignCreateRequest.InitiatedBy);
                if (campaignCreateResponse.Equals(Constants.Application.Success))
                {
                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.CampaignCreateSuccessMessage
                    };
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreateCampaign.ToString())) });
                    return result;
                }
                else
                {
                    var result = Result.Failure<Response<string>>(campaignCreateResponse);
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = campaignCreateResponse, Status = ServiceResponseProcessStatus.Error, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreateCampaign.ToString())) });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CampaignCreateEventProcessCommandHandler failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation($"Campaign Create Event Handler ({request.Id}) - End");
            }
        }
    }
}
