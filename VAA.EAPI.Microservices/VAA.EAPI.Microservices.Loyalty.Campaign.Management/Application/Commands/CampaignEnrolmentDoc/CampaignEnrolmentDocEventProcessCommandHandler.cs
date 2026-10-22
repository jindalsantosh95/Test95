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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignEnrolmentDoc
{
    public class CampaignEnrolmentDocEventProcessCommandHandler : IRequestHandler<CampaignEnrolmentDocEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignEnrolmentDocEventProcessCommandHandler> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public CampaignEnrolmentDocEventProcessCommandHandler(IMediator mediator, ILogger<CampaignEnrolmentDocEventProcessCommandHandler> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<Result<Response<string>>> Handle(CampaignEnrolmentDocEventProcessCommand request, CancellationToken cancellationToken)
        {
            CampaignEnrolmentDocDetails campaignEnrolmentDoc = null;
            try
            {
                var camapignDetails = Json.Deserialize<CampaignRegisterEventData>(Convert.ToString(request.Data));
                campaignEnrolmentDoc = CampaignDetailToEnrollDocMapper.MaptoEnrollmentDoc(camapignDetails, await _commonRefDBRepository.GetPromotionId(camapignDetails.CampaignId));
                if (!string.IsNullOrEmpty(campaignEnrolmentDoc?.CampaignEnrolmentDocument?.PromotionId) && camapignDetails.DocumentStatus == Constants.Application.EnrolDocStatus)
                {
                    var cloudEvent = CampaignRegisterEventMapper.CommandToCampaignEnrolmentDocEventMapper(campaignEnrolmentDoc);
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
                            Value = "CampaignEnrolmentDoc event posted successfully"
                        });
                    }
                    catch
                    {
                        _logger.LogDebug($"Deleting the event from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = CampaignEventType.CampaignEnrolmentDocEventInitiate.ToString(), Data = campaignEnrolmentDoc, Subject = Constants.Application.CampaignEnrolmentDocEventSubject });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in CampaignEnrolmentDocEventProcessCommandHandler handler");
                return Result.Failure<Response<string>>(ex.Message);
            }
        }
    }
}
