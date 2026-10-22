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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.TargetMembershipDoc;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.TargetedMembership
{
    public class TargetedMembershipDocCreateCommandHandler : IRequestHandler<TargetedMembershipDocCreateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private readonly ILogger<TargetedMembershipDocCreateCommandHandler> _logger;

        public TargetedMembershipDocCreateCommandHandler(IMediator mediator, ICommonRefDBRepository commonRefDBRepository, ILogger<TargetedMembershipDocCreateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(TargetedMembershipDocCreateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var assignEventData = Json.Deserialize<CampaignAssignEventData>(Convert.ToString(request.Data));
                var campaignData = await _commonRefDBRepository.GetCampaignDetails(assignEventData.CampaignId);
                if (!string.IsNullOrEmpty(campaignData?.CampaignId))
                {
                    var eventData = TargetedMembershipDocCreateEventDataMapper.TargetMembershipDocCreateEventDataMapper(assignEventData, campaignData);
                    var cloudEvent = TargetedMembershipDocCreateEventDataMapper.CommandToTragetedMembershipEventMapper(eventData);
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                    _logger.LogDebug($"Store event in event store - End");
                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent);
                        _logger.LogDebug($"Publish event - End");
                    }
                    catch
                    {
                        _logger.LogDebug($"Deleting the event from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }

                    return Result.Success(new Response<string>
                    {
                        Status = Status.OK,
                        Value = Constants.Application.TargetedDocSuccessMessage
                    });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in CampaignRegisterEventProcessCommandHandler handler");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}