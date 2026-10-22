using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.EventData;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.PartnerReferenceData;
public sealed class PartnerReferenceDataCommandHandler : IPartnerReferenceDataCommandHandler
{
    private readonly IPartnerProfileQueryHandler _partnerProfileQueryHandler;
    private readonly ILogger<PartnerReferenceDataCommandHandler> _logger;
    private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
    private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
    private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;

    public PartnerReferenceDataCommandHandler(IPartnerProfileQueryHandler partnerProfileQueryHandler, ILogger<PartnerReferenceDataCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler)
    {
        _partnerProfileQueryHandler = partnerProfileQueryHandler ?? throw new ArgumentNullException(nameof(partnerProfileQueryHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
    }
    public async Task<Result<string>> Handle(PartnerReferenceDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<PartnerProfileDetails> partnerDetails;
            var partnersResponse = await _partnerProfileQueryHandler.Handle(new PartnerProfileQuery(), cancellationToken);

            if (partnersResponse.IsFailure)
                return Result.Failure<string>("Failed to retrieve Partner Data from sql DB");

            partnerDetails = partnersResponse.Value?.Value;

            if (partnerDetails is null)
                return Result.Failure<string>("Partner response does not contain partner details");

            CloudEvent cloudEvent = EventMapper.MapPartnersToCloudEvent(new PartnersEventData { Partners = partnerDetails.Select(x => (Partner)x) });

            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = partnerDetails.Select(x => x.RowId), Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);
            }
            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = partnerDetails.Select(x => x.RowId), Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }

            return Result.Success<string>(Constants.Application.EventSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred in PartnerReferenceDataCommandHandler: {ex.Message}");
            return Result.Failure<string>(ex.Message.ToString());
        }
    }
}