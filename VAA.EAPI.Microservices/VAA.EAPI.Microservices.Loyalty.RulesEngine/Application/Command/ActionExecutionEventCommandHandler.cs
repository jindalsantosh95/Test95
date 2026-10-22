using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Query;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Transaction;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper;
using Action = VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine.Action;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command
{
    public class ActionExecutionEventCommandHandler : IRequestHandler<ActionExecutionEventCommand, Result<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ActionExecutionEventCommandHandler> _logger;
        public ActionExecutionEventCommandHandler(IMediator mediator, ILogger<ActionExecutionEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<bool>> Handle(ActionExecutionEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Action Execution Event CommandHandler started");
                var actionExecutionRequest = Json.Deserialize<RuleActionsResponse>(Convert.ToString(request.Data));

                foreach (Action action in actionExecutionRequest?.RuleResponse?.Actions)
                {
                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.SetTier.ToString())))
                    {
                        //Set Tier - FCM Doc Update Event
                        FlyingClubMemberUpdateEventData flyingClubMemberEventData = ActionExecutionEventDataMapper.MapSetTierFCMDocEventData(action, actionExecutionRequest.CustomerId);
                        var fcmCloudEvent = ActionExecutionEventMapper.MapFCMDocEvent(flyingClubMemberEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = fcmCloudEvent.Id, Type = fcmCloudEvent.Type, Data = fcmCloudEvent.Data, Subject = fcmCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(fcmCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch (Exception exception)
                        {
                            _logger.LogDebug($"Deleting the FCM Doc Update event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = fcmCloudEvent.Id, Type = fcmCloudEvent.Type, Data = fcmCloudEvent.Data, Subject = fcmCloudEvent.Subject }, cancellationToken);
                            _logger.LogError(exception, $"FCM Doc Update Event publish failed - {exception}");
                        }

                        //Set Tier - MembershipChange Doc Update Event
                        MembershipChangeDocumentEventData MembershipChangeEventData = ActionExecutionEventDataMapper.MapMembershipChangeUpdateEventData(action, actionExecutionRequest.CustomerId);
                        var updateCloudEvent = ActionExecutionEventMapper.MapMembershipDocUpdateEvent(MembershipChangeEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = updateCloudEvent.Id, Type = updateCloudEvent.Type, Data = updateCloudEvent.Data, Subject = updateCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(updateCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch (Exception exception)
                        {
                            _logger.LogDebug($"Deleting the  MembershipChange Doc Update event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = updateCloudEvent.Id, Type = updateCloudEvent.Type, Data = updateCloudEvent.Data, Subject = updateCloudEvent.Subject }, cancellationToken);
                            _logger.LogError(exception, $"MembershipChange Doc Update Event publish failed - {exception}");
                        }

                        //Set Tier - MembershipChange Doc Create Event
                        MembershipChangeDocumentEventData MembershipChangeCreateEventData = ActionExecutionEventDataMapper.MapMembershipChangeCreateEventData(action, actionExecutionRequest);
                        var createCloudEvent = ActionExecutionEventMapper.MapMembershipDocCreateEvent(MembershipChangeCreateEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = createCloudEvent.Id, Type = createCloudEvent.Type, Data = createCloudEvent.Data, Subject = createCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(createCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch (Exception exception)
                        {
                            _logger.LogDebug($"Deleting the MembershipChange Doc Create from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = createCloudEvent.Id, Type = createCloudEvent.Type, Data = createCloudEvent.Data, Subject = createCloudEvent.Subject }, cancellationToken);
                            _logger.LogError(exception, $"MembershipChange Doc Create Event publish failed - {exception}");
                        }
                    }

                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.SetDowngradeProhibitFlag.ToString())))
                    {
                        //SetDowngradeProhibitFlag - FCM Doc update Event
                        FlyingClubMemberUpdateEventData fcmDPFUpdateEventData = ActionExecutionEventDataMapper.MapDowngradeProhibitFlagFCMDocEventData(action, actionExecutionRequest.CustomerId);
                        var fcmDPFCloudEvent = ActionExecutionEventMapper.MapFCMDocEvent(fcmDPFUpdateEventData);      
                        
                        await _mediator.Send(new EventRequestInsertCommand { Id = fcmDPFCloudEvent.Id, Type = fcmDPFCloudEvent.Type, Data = fcmDPFCloudEvent.Data, Subject = fcmDPFCloudEvent.Subject });
                       
                        try
                        {                            
                            await _mediator.Publish(fcmDPFCloudEvent);                          
                        }
                        catch
                        {                          
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = fcmDPFCloudEvent.Id, Type = fcmDPFCloudEvent.Type, Data = fcmDPFCloudEvent.Data, Subject = fcmDPFCloudEvent.Subject }, cancellationToken);
                            throw;
                        }

                    }

                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.SetS2GFlag.ToString())))
                    {
                        //SetS2GFlag - FCM Doc update Event
                        FlyingClubMemberUpdateEventData fcmS2GUpdateEventData = ActionExecutionEventDataMapper.MapFlagsFCMDocEventData(action, actionExecutionRequest.CustomerId);
                        var fcmS2GCloudEvent = ActionExecutionEventMapper.MapFCMDocEventFlags(fcmS2GUpdateEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = fcmS2GCloudEvent.Id, Type = fcmS2GCloudEvent.Type, Data = fcmS2GCloudEvent.Data, Subject = fcmS2GCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(fcmS2GCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch
                        {
                            _logger.LogDebug($"Deleting the FCM Doc update event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = fcmS2GCloudEvent.Id, Type = fcmS2GCloudEvent.Type, Data = fcmS2GCloudEvent.Data, Subject = fcmS2GCloudEvent.Subject }, cancellationToken);
                            throw;
                        }

                    }
                    
                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.SetS2SFlag.ToString())))
                    {
                        //SetS2SFlag - FCM Doc update Event
                        FlyingClubMemberUpdateEventData fcmS2SUpdateEventData = ActionExecutionEventDataMapper.MapFlagsFCMDocEventData(action, actionExecutionRequest.CustomerId);
                        var fcmS2SCloudEvent = ActionExecutionEventMapper.MapFCMDocEventFlags(fcmS2SUpdateEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = fcmS2SCloudEvent.Id, Type = fcmS2SCloudEvent.Type, Data = fcmS2SCloudEvent.Data, Subject = fcmS2SCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(fcmS2SCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch
                        {
                            _logger.LogDebug($"Deleting the FCM Doc update event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = fcmS2SCloudEvent.Id, Type = fcmS2SCloudEvent.Type, Data = fcmS2SCloudEvent.Data, Subject = fcmS2SCloudEvent.Subject }, cancellationToken);
                            throw;
                        }

                    }
                   
                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.SetS2SFTGFlag.ToString())))
                    {
                        //SetS2SFTGFlag - FCM Doc update Event
                        FlyingClubMemberUpdateEventData fcmS2SFTGUpdateEventData = ActionExecutionEventDataMapper.MapFlagsFCMDocEventData(action, actionExecutionRequest.CustomerId);
                        var fcmS2SFTGCloudEvent = ActionExecutionEventMapper.MapFCMDocEventFlags(fcmS2SFTGUpdateEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = fcmS2SFTGCloudEvent.Id, Type = fcmS2SFTGCloudEvent.Type, Data = fcmS2SFTGCloudEvent.Data, Subject = fcmS2SFTGCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(fcmS2SFTGCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch
                        {
                            _logger.LogDebug($"Deleting the FCM Doc update event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = fcmS2SFTGCloudEvent.Id, Type = fcmS2SFTGCloudEvent.Type, Data = fcmS2SFTGCloudEvent.Data, Subject = fcmS2SFTGCloudEvent.Subject }, cancellationToken);
                            throw;
                        }

                    }
                    
                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.SetEarnActivityFlag.ToString())))
                    {
                        FlyingClubMemberUpdateEventData fcmEarnAcitivityUpdateEventData = ActionExecutionEventDataMapper.MapFlagsFCMDocEventData(action, actionExecutionRequest.CustomerId);
                        var fcmEarnActivityCloudEvent = ActionExecutionEventMapper.MapFCMDocEventFlags(fcmEarnAcitivityUpdateEventData);
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = fcmEarnActivityCloudEvent.Id, Type = fcmEarnActivityCloudEvent.Type, Data = fcmEarnActivityCloudEvent.Data, Subject = fcmEarnActivityCloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        try
                        {
                            _logger.LogDebug($"Publish event - Start");
                            await _mediator.Publish(fcmEarnActivityCloudEvent);
                            _logger.LogDebug($"Publish event - End");
                        }
                        catch
                        {
                            _logger.LogDebug($"Deleting the FCM Doc update event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = fcmEarnActivityCloudEvent.Id, Type = fcmEarnActivityCloudEvent.Type, Data = fcmEarnActivityCloudEvent.Data, Subject = fcmEarnActivityCloudEvent.Subject }, cancellationToken);
                            throw;
                        }
                    }
                   
                    if (action.Name.ToUpper() == EnumOperations.GetEnumDescription((ActionNameEnum)Enum.Parse(typeof(ActionNameEnum), ActionNameEnum.AssignPoints.ToString())))
                    {
                        string statementText = TransactionStatementMapper.MapStatement(actionExecutionRequest);
                        if (actionExecutionRequest.ProductName == Constants.Application.BirthdayBonusProductName)
                        {
                            string partnerId = await _mediator.Send(new GetPartnerReferenceDetailsQuery() { PartnerCode = Constants.Application.VSPartnerCode });
                            TransactionCreateEventData transactionCreateEventData = ActionExecutionEventDataMapper.MapBirthdayBonusTransactionCreateEventData(action, actionExecutionRequest, partnerId, statementText);
                            var txnCreateCloudEvent = ActionExecutionEventMapper.MapTxnDocCreateEvent(transactionCreateEventData);
                            _logger.LogDebug($"Store event in event store - Start");
                            await _mediator.Send(new EventRequestInsertCommand { Id = txnCreateCloudEvent.Id, Type = txnCreateCloudEvent.Type, Data = txnCreateCloudEvent.Data, Subject = txnCreateCloudEvent.Subject });
                            _logger.LogDebug($"Store event in event store - End");
                            try
                            {
                                _logger.LogDebug($"Publish event - Start");
                                await _mediator.Publish(txnCreateCloudEvent);
                                _logger.LogDebug($"Publish event - End");
                            }
                            catch
                            {
                                _logger.LogDebug($"Deleting the assign points event from event store");
                                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = txnCreateCloudEvent.Id, Type = txnCreateCloudEvent.Type, Data = txnCreateCloudEvent.Data, Subject = txnCreateCloudEvent.Subject }, cancellationToken);
                                throw;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Action executtion failed - {exception}");
                return false;
            }
            finally
            {
                _logger.LogInformation("Action Execution Event CommandHandler completed");
            }
        }
    }
}
