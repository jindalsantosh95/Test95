using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services
{
    public class SkyTeamAutoAccrualKafkaPublisher : ISkyTeamAutoAccrualKafkaPublisher
    {
        private const string EventType = "SkyTeamAutoAccrualOutbound";
        private readonly IPgpEncryptionService _pgpEncryptionService;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreInsertHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreDeleteHandler;
        public SkyTeamAutoAccrualKafkaPublisher(IPgpEncryptionService pgpEncryptionService, IKafkaProducerService kafkaProducerService, EventStoreRequestInsertCommandHandler eventStoreInsertHandler, EventStoreRequestDeleteCommandHandler eventStoreDeleteHandler)
        {
            _pgpEncryptionService = pgpEncryptionService ?? throw new ArgumentNullException(nameof(pgpEncryptionService));
            _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
            _eventStoreInsertHandler = eventStoreInsertHandler ?? throw new ArgumentNullException(nameof(eventStoreInsertHandler));
            _eventStoreDeleteHandler = eventStoreDeleteHandler ?? throw new ArgumentNullException(nameof(eventStoreDeleteHandler));
        }
        public async Task PublishAsync(DistanceBasedAccrualPayload accrualRequest, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(accrualRequest);
            string messageId = Guid.NewGuid().ToString();

            var publicKey = ConfigReader.Try(Constants.Environment.SkyTeamPgpPublicKey);
            if (publicKey.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("SkyTeam PGP public key is missing.");
            }

            var encryptionKeyId = ConfigReader.Try(Constants.Environment.AccrualPgpEncryptionKeyId);
            if (encryptionKeyId.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("SkyTeam PGP encryption key ID is missing.");
            }

            var innerPayloadJson = SerializeAccrualPayload(accrualRequest);
            var envelope = new DbaAccrualEnvelope
            {
                Producer = accrualRequest.Producer,
                Consumer = accrualRequest.Consumer,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'"),
                MessageId = messageId,
                CorrelationId = Guid.NewGuid().ToString(),
                EncryptionKeyId = encryptionKeyId,
                EncryptedPayload = innerPayloadJson,
                ProducerSignature = null
            };

            await _eventStoreInsertHandler.Handle(
                new EventRequestInsertCommand
                {
                    Id = messageId,
                    Type = EventType,
                    Data = envelope,
                    Subject = Constants.Application.AutoAccrualMessage,
                    Source = ConfigReader.Get(Constants.Environment.AccrualOutboundKafkaTopic)
                },
                cancellationToken);

            try
            {
                envelope.EncryptedPayload = _pgpEncryptionService.Encrypt(innerPayloadJson, publicKey);
                var completeEnvelopeJson = SerializeEnvelope(envelope);

                cancellationToken.ThrowIfCancellationRequested();
                await _kafkaProducerService.PublishAsync(completeEnvelopeJson, cancellationToken);
            }
            catch
            {
                await _eventStoreDeleteHandler.Handle(
                    new EventStoreRequestDeleteCommand { Id = messageId, Type = EventType },
                    CancellationToken.None);
                throw;
            }
        }

        private static string SerializeAccrualPayload(DistanceBasedAccrualPayload accrualRequest)
        {
            var payload = JObject.Parse(Json.Serialize(accrualRequest));

            RemoveIfEmpty(payload, "uniqueIdentification", "airlineCode");
            RemoveIfEmpty(payload, "flightDetails", "operatingFlight", "operatingFlightNumber");
            RemoveIfEmpty(payload, "flightTypeIndicator");
            RemoveIfEmpty(payload, "bookingDetails", "operatingCarrierFareType");
            RemoveIfEmpty(payload, "bookingDetails", "marketingCarrierFareType");
            RemoveIfEmpty(payload, "additionalPassengerInformation");
            RemoveIfEmpty(payload, "additionalInformation");

            // These fields are conditional for RBA only and must not be sent in a DBA payload.
            payload.Remove("creditCode");
            (payload["documentDetails"] as JObject)?.Remove("documentIssueDateTime");

            return payload.ToString(Formatting.None);
        }

        private static string SerializeEnvelope(DbaAccrualEnvelope envelope)
        {
            var serializedEnvelope = JObject.Parse(Json.Serialize(envelope));
            RemoveIfEmpty(serializedEnvelope, "producerSignature");

            return serializedEnvelope.ToString(Formatting.None);
        }

        private static void RemoveIfEmpty(JObject message, params string[] propertyPath)
        {
            var containingObject = message;

            for (var index = 0; index < propertyPath.Length - 1; index++)
            {
                var nestedObject = containingObject[propertyPath[index]] as JObject;
                if (nestedObject is null)
                {
                    return;
                }

                containingObject = nestedObject;
            }

            var propertyName = propertyPath[^1];
            if (containingObject[propertyName]?.Type == JTokenType.Null ||
                (containingObject[propertyName]?.Value<string>()).IsNullOrWhiteSpace())
            {
                containingObject.Remove(propertyName);
            }
        }
    }
}
