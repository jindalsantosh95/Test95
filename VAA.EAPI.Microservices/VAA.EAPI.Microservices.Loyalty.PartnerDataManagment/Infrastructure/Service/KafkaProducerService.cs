using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducerService(ILogger<KafkaProducerService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var bootstrapServers = ConfigReader.Get(Constants.Environment.SkyTeamKafkaBootstrapServers);

            _topic = ConfigReader.Get(Constants.Environment.AccrualOutboundKafkaTopic);

            if (string.IsNullOrWhiteSpace(bootstrapServers))
            {
                throw new InvalidOperationException("SkyTeam Kafka bootstrap server is missing.");
            }

            if (string.IsNullOrWhiteSpace(_topic))
            {
                throw new InvalidOperationException("SkyTeam Kafka topic is missing.");
            }

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishAsync(string encryptedPayload, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(encryptedPayload))
            {
                throw new ArgumentException("Encrypted payload cannot be empty.", nameof(encryptedPayload));
            }

            var message = new Message<Null, string>
            {
                Value = encryptedPayload
            };

            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);
        }
    }
}