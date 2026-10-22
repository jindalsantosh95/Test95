using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Services
{
    public class AutoAccrualFeedbackPublisher : IAutoAccrualFeedbackPublisher
    {
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusPublisher;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreInsertHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreDeleteHandler;

        public AutoAccrualFeedbackPublisher(
            ServiceBusTopicMessageEventCommandHandler serviceBusPublisher,
            EventStoreRequestInsertCommandHandler eventStoreInsertHandler,
            EventStoreRequestDeleteCommandHandler eventStoreDeleteHandler)
        {
            _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
            _eventStoreInsertHandler = eventStoreInsertHandler ?? throw new ArgumentNullException(nameof(eventStoreInsertHandler));
            _eventStoreDeleteHandler = eventStoreDeleteHandler ?? throw new ArgumentNullException(nameof(eventStoreDeleteHandler));
        }

        public Task PublishSuccessAsync(string correlationId, string messageId, TransactionDocument transaction, CancellationToken cancellationToken) =>
            PublishAsync(new AutoAccrualFeedbackPayload
            {
                CorrelationId = correlationId,
                MessageId = messageId,
                ProcessedStatus = Constants.Application.AutoAccrualFeedbackSuccessStatus,
                TransactionId = transaction.TransactionId,
                MileDetails = GetMileDetails(transaction)
            }, cancellationToken);

        public Task PublishErrorAsync(string correlationId, string messageId, string errorCode, string errorDescription, CancellationToken cancellationToken) =>
            PublishAsync(new AutoAccrualFeedbackPayload
            {
                CorrelationId = correlationId,
                MessageId = messageId,
                ProcessedStatus = Constants.Application.AutoAccrualFeedbackErrorStatus,
                TransactionId = string.Empty,
                MileDetails = PlaceholderMileDetails(),
                ErrorDetails = new AutoAccrualFeedbackErrorDetails
                {
                    ErrorCode = errorCode,
                    ErrorDescription = errorDescription
                }
            }, cancellationToken);

        private async Task PublishAsync(AutoAccrualFeedbackPayload payload, CancellationToken cancellationToken)
        {
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = Guid.TryParse(payload.CorrelationId, out var correlationId) ? correlationId : Guid.Empty,
                Specversion = Constants.Application.EventSpecversion,
                Type = Constants.Application.AutoAccrualFeedbackEventName,
                Subject = Constants.Application.AutoAccrualFeedbackEventName,
                Source = Constants.Application.PdmEventTopicName,
                DataContentType = Constants.Application.AutoAccrualFeedbackDataContentType,
                Data = payload,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat)
            };

            try
            {
                await _eventStoreInsertHandler.Handle(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent.Data,
                    Subject = cloudEvent.Subject
                }, cancellationToken);

                await _serviceBusPublisher.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = cloudEvent,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = Constants.Application.PdmEventTopicName
                }, cancellationToken);
            }
            catch
            {
                await _eventStoreDeleteHandler.Handle(new EventStoreRequestDeleteCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent.Data,
                    Subject = cloudEvent.Subject
                }, cancellationToken);

                throw;
            }
        }

        private static AutoAccrualFeedbackMileDetails PlaceholderMileDetails() => new()
        {
            BaseMiles = Constants.Application.AutoAccrualFeedbackPlaceholderMiles,
            ClassOfServiceBonusMiles = Constants.Application.AutoAccrualFeedbackPlaceholderMiles,
            EliteTierBonusMiles = Constants.Application.AutoAccrualFeedbackPlaceholderMiles,
            PromotionalBonusMiles = Constants.Application.AutoAccrualFeedbackPlaceholderMiles
        };

        private static AutoAccrualFeedbackMileDetails GetMileDetails(TransactionDocument transaction) => new()
        {
            BaseMiles = GetCreditValue(transaction, Constants.Application.BaseMilesPointSubType),
            ClassOfServiceBonusMiles = GetCreditValue(transaction, Constants.Application.CabinBonusPointSubType),
            EliteTierBonusMiles = GetCreditValue(transaction, Constants.Application.TierBonusPointSubType),
            PromotionalBonusMiles = GetCreditValue(transaction, Constants.Application.PromotionBonusPointSubType)
        };

        private static int GetCreditValue(TransactionDocument transaction, string pointSubType) => int.TryParse(transaction.CreditDetails?.FirstOrDefault(credit => credit.PointSubType.Is(pointSubType))?.CreditValue, out var value) ? value : Constants.Application.AutoAccrualFeedbackPlaceholderMiles;
    }
}
