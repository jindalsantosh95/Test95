using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services
{
    public class SkyTeamAutoAccrualFeedbackKafkaPublisher : ISkyTeamAutoAccrualFeedbackKafkaPublisher
    {
        private readonly IPgpEncryptionService _pgpEncryptionService;
        private readonly IKafkaProducerService _kafkaProducerService;

        public SkyTeamAutoAccrualFeedbackKafkaPublisher(
            IPgpEncryptionService pgpEncryptionService,
            IKafkaProducerService kafkaProducerService)
        {
            _pgpEncryptionService = pgpEncryptionService ?? throw new ArgumentNullException(nameof(pgpEncryptionService));
            _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
        }

        public async Task<string> PublishAsync(DistanceBasedAccrualFeedbackPayload feedback, string correlationId, string messageId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(feedback);
            if (correlationId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Correlation ID is required.", nameof(correlationId));
            }

            if (messageId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Message ID is required.", nameof(messageId));
            }

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

            var envelope = new DbaFeedbackEnvelope
            {
                Producer = feedback.Producer,
                Consumer = feedback.Consumer,
                Timestamp = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                MessageId = messageId,
                CorrelationId = correlationId,
                EncryptionKeyId = encryptionKeyId,
                EncryptedPayload = _pgpEncryptionService.Encrypt(Json.Serialize(feedback, ignoreNullValues: false), publicKey)
            };

            await _kafkaProducerService.PublishAsync(Json.Serialize(envelope, ignoreNullValues: false), cancellationToken);
            return envelope.MessageId;
        }
    }
}
