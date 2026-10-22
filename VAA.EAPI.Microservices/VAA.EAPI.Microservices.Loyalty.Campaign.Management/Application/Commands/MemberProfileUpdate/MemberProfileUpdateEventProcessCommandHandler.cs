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
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.MemberProfileUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.MemberProfileUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.MemberProfileUpdate
{
    public class MemberProfileUpdateEventProcessCommandHandler : IRequestHandler<MemberProfileUpdateEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private readonly ILogger<MemberProfileUpdateEventProcessCommandHandler> _logger;

        public MemberProfileUpdateEventProcessCommandHandler(IMediator mediator, ICommonRefDBRepository commonRefDBRepository, ILogger<MemberProfileUpdateEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(MemberProfileUpdateEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var campaignDetails = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
                if (campaignDetails.CampaignType == Constants.Application.FastTrackToGold || campaignDetails.CampaignType == Constants.Application.GoldAfterFirstFlight || campaignDetails.CampaignType == Constants.Application.SilverAfterFirstFlightWithFTG
                    || campaignDetails.CampaignType == Constants.Application.SilverAfterFirstFlight || campaignDetails.CampaignType == Constants.Application.StraightToGold || campaignDetails.CampaignType == Constants.Application.StraightToSilver
                    || campaignDetails.CampaignType == Constants.Application.StraightToSilverWithFTG)
                {
                    MemberProfileUpdateEventData memberProfileUpdateData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(campaignDetails);
                    if (campaignDetails.DocumentStatus == Constants.Application.EnrolDocStatus)
                    {
                        var cloudEvent = CampaignRegisterEventMapper.CommandToMemberProfileUpdateEventMapper(memberProfileUpdateData);

                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(cloudEvent);
                            _logger.LogDebug($"Publish event - End");
                            return Result.Success(new Response<string>
                            {
                                Status = Status.OK,
                                Value = Constants.Application.MemberProfileUpdateMessage
                            });
                        }
                        catch
                        {
                            _logger.LogDebug($"Deleting the event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = CampaignEventType.MemberProfileUpdateEventInitiate.ToString(), Data = memberProfileUpdateData, Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateFlyingClubMember.ToString())) });
                            throw;
                        }
                    }
                    else
                    {
                        return Result.Success(new Response<string>
                        {
                            Status = Status.BusinessException,
                            Value = null,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        });
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Status = Status.OK,
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in MemberProfileUpdateEventProcessCommandHandler handler");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}