using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountEmailTriggerCommandHandler : IRequestHandler<HouseHoldAccountEmailTriggerCommand, bool>
    {
        private readonly EventStoreRequestInsertCommandHandler _eventStoreInsertHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreDeleteHandler;
        private readonly ILogger<HouseHoldAccountEmailTriggerCommandHandler> _logger;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public HouseHoldAccountEmailTriggerCommandHandler(EventStoreRequestInsertCommandHandler eventStoreInsertHandler, EventStoreRequestDeleteCommandHandler eventStoreDeleteHandler, ILogger<HouseHoldAccountEmailTriggerCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _eventStoreInsertHandler = eventStoreInsertHandler ?? throw new ArgumentNullException(nameof(eventStoreInsertHandler));
            _eventStoreDeleteHandler = eventStoreDeleteHandler ?? throw new ArgumentNullException(nameof(eventStoreDeleteHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        }

        public async Task<bool> Handle(HouseHoldAccountEmailTriggerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"HouseHoldAccountEmailTriggerCommandHandler with Id {request.Id}- Start");
            try
            {
                if (request.IsYoungFlyerParentEmail)
                {
                    var youngFlyerEmailEvent = YoungFlyerParentEmailEventMapper.CreateEvent(new YoungFlyerParentEmailCommand
                    {
                        Email = request.Email,
                        EmailTemplate = ConfigReader.Get(request.EmailTemplate),
                        AdobeInviteMemberRequest = new YoungFlyerParentEmailData
                        {
                            FirstName = request.ChildFirstName,
                            LastName = request.ChildLastName,
                            MembershipNumber = request.ChildMemberNumber,
                            FirstNameOfHouseholdLead = request.FirstName,
                            FirstNameOfHouseholdMember = request.ChildFirstName
                        }
                    });

                    // Keep the Young Flyer publication consistent with the other event paths:
                    // persist the event before publishing it and remove it when publication fails.
                    await _eventStoreInsertHandler.Handle(new EventRequestInsertCommand
                    {
                        Id = youngFlyerEmailEvent.Id,
                        Type = youngFlyerEmailEvent.Type,
                        Data = youngFlyerEmailEvent.Data,
                        Subject = youngFlyerEmailEvent.Subject
                    }, cancellationToken);

                    try
                    {
                        await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                        {
                            Message = youngFlyerEmailEvent,
                            ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                            ServiceBusTopicName = Constants.Application.YoungFlyerParentEmailEventTopicName
                        }, cancellationToken);
                    }
                    catch
                    {
                        await _eventStoreDeleteHandler.Handle(new EventStoreRequestDeleteCommand
                        {
                            Id = youngFlyerEmailEvent.Id,
                            Type = youngFlyerEmailEvent.Type,
                            Data = youngFlyerEmailEvent.Data,
                            Subject = youngFlyerEmailEvent.Subject
                        }, cancellationToken);
                        throw;
                    }

                    return true;
                }

                var eventData = HouseHoldAccountEmailTriggeringEventMapper.CreateEventGridSchemaData(request);
                var cloudEvent = HouseHoldAccountEmailTriggeringEventGridMapper.EventDataMapper(eventData);
                await _eventStoreInsertHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                    {
                        Message = cloudEvent,
                        ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                        ServiceBusTopicName = cloudEvent.Source
                    }, cancellationToken);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"HouseHoldAccountEmailTriggerCommandHandler {request.Id} failed due to Exception");
                    await _eventStoreDeleteHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"HouseHoldAccountEmailTriggerCommandHandler {request.Id} failed due to Exception");
                return false;
            }
            finally
            {
                _logger.LogInformation($"HouseHoldAccountEmailTriggerCommandHandler {request.Id} - End");
            }
        }
    }
}
