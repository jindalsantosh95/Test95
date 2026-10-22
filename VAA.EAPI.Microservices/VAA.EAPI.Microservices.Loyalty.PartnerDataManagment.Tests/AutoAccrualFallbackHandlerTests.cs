using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class AutoAccrualFallbackHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public AutoAccrualFallbackHandlerTests()
        {
            ConfigReader.Set(Constants.Environment.AutoAccrualFallbackDepartureThresholdDays, "2");
            ConfigReader.Set(Constants.Environment.SkyTeamPgpPublicKey, "test-public-key");
            ConfigReader.Set(Constants.Environment.AccrualPgpEncryptionKeyId, "test-key-id");

            _serviceProvider = new ServiceCollection()
                .AddSingleton<FakeFlownDbRepository>()
                .AddSingleton<IFlownDbRepository>(provider => provider.GetRequiredService<FakeFlownDbRepository>())
                .AddSingleton<IPartnerDataService, FakePartnerDataService>()
                .AddSingleton<IPgpEncryptionService, FakePgpEncryptionService>()
                .AddSingleton<FakeKafkaProducerService>()
                .AddSingleton<IKafkaProducerService>(provider => provider.GetRequiredService<FakeKafkaProducerService>())
                .AddSingleton<ISkyTeamAutoAccrualKafkaPublisher, FakeSkyTeamAutoAccrualKafkaPublisher>()
                .AddSingleton<IAutoAccrualFallbackHandler, AutoAccrualFallbackHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_PublishesOneBatchPerEligibleFfpCarrier()
        {
            ConfigReader.Set(Constants.Environment.AutoAccrualFallbackDepartureThresholdDays, "3");
            var flownDbRepository = _serviceProvider.GetRequiredService<FakeFlownDbRepository>();
            flownDbRepository.Records =
            [
                CreateFlownRecord("KQ", "1234567890"),
                CreateFlownRecord("KQ", "1234567891"),
                CreateFlownRecord("AF", "1234567892")
            ];
            var kafkaProducer = _serviceProvider.GetRequiredService<FakeKafkaProducerService>();
            var handler = _serviceProvider.GetRequiredService<IAutoAccrualFallbackHandler>();

            await handler.HandleAsync(CancellationToken.None);

            Assert.Equal(Constants.Application.ClaimStatus, flownDbRepository.Criteria?.ProcessingStatus);
            Assert.Equal(Constants.Application.NoShowNo, flownDbRepository.Criteria?.NoShowIndicator);
            Assert.Equal(Constants.Application.SentToPartnerNo, flownDbRepository.Criteria?.SentToPartnerIndicator);
            Assert.Equal(DateTime.UtcNow.Date.AddDays(-3), flownDbRepository.Criteria?.DepartureDate);
            Assert.Contains("KQ", flownDbRepository.Criteria?.FfpCarrierCodes);
            Assert.Contains("AF", flownDbRepository.Criteria?.FfpCarrierCodes);
            Assert.DoesNotContain("BA", flownDbRepository.Criteria?.FfpCarrierCodes);
            Assert.Equal(3, kafkaProducer.Payloads.Count);

            var publishedPayloads = kafkaProducer.Payloads .Select(payload => Json.Deserialize<DistanceBasedAccrualPayload>(payload)).ToList();

            Assert.Equal(3, publishedPayloads.Count);
            Assert.Equal(2, publishedPayloads.Count(request => request?.Consumer == "KQ"));
            Assert.Equal(1, publishedPayloads.Count(request => request?.Consumer == "AF"));
        }

        [Fact]
        public async Task HandleAsync_DoesNotPublish_WhenNoEligibleRecordsAreReturned()
        {
            var flownDbRepository = _serviceProvider.GetRequiredService<FakeFlownDbRepository>();
            flownDbRepository.Records = [CreateFlownRecord(null, "1234567890")];
            var kafkaProducer = _serviceProvider.GetRequiredService<FakeKafkaProducerService>();
            var handler = _serviceProvider.GetRequiredService<IAutoAccrualFallbackHandler>();

            await handler.HandleAsync(CancellationToken.None);

            Assert.Empty(kafkaProducer.Payloads);
        }

        private static AutoAccrualFlownDetails CreateFlownRecord(string? ffpCarrierCode, string ticketNumber)
        {
            return new AutoAccrualFlownDetails
            {
                FFPCarrierCode = ffpCarrierCode,
                FFPAccountNumber = "123456",
                FirstName = "Test",
                LastName = "Passenger",
                MarketingAirlineCode = "VS",
                MarketingFlightNum = "100",
                OperatingAirlineCode = "VS",
                OperatingFlightNum = "100",
                DepartureDate = "2026-09-01",
                OriginPort = "LHR",
                DestinationPort = "JFK",
                FlownCabinClass = "Y",
                MarketingCarrierBookingClass = "Y",
                OperatingCarrierBookingClass = "Y",
                CouponNumber = "1",
                TicketNumber = ticketNumber,
                OperatingAirAuthNumber = "000106337233331",
                ProcessingStatus = Constants.Application.ClaimStatus,
                FdNoShow = Constants.Application.NoShowNo,
                SentToPartner = Constants.Application.SentToPartnerNo
            };
        }

        private class FakeFlownDbRepository : IFlownDbRepository
        {
            public IEnumerable<AutoAccrualFlownDetails> Records { get; set; } = [];

            public AutoAccrualFallbackCriteria? Criteria { get; private set; }

            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken)
            {
                Criteria = criteria;
                return Task.FromResult(Records);
            }

            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken) =>
                throw new NotSupportedException();

            public Task<RetroCreditFlownDetails> GetFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest) =>
                throw new NotSupportedException();

            public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<bool> ResetPendingFeedbackAutoRetroAsync(string days) => throw new NotSupportedException();

            public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(RetroCreditRequest retroCreditRequest, string uniqueNumber) =>
                throw new NotSupportedException();

            public Task<bool> UpdateFlownDetailsWithRetroFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest) =>
                throw new NotSupportedException();

            public Task<bool> UpdateTicketStatus(string id, string ticketNumber, string ticketStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode) =>
                throw new NotSupportedException();

            public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message) =>
                throw new NotSupportedException();

        }

        private class FakePgpEncryptionService : IPgpEncryptionService
        {
            public string Encrypt(string payload, string publicKeyContent) => payload;
        }

        private class FakePartnerDataService : IPartnerDataService
        {
            private static readonly List<Partner> Partners =
            [
                new Partner
                {
                    PartnerCode = "KQ"
                },
                new Partner
                {
                    PartnerCode = "AF"
                }
            ];

            public Task<IReadOnlyCollection<Partner>> GetSkyTeamPartnersAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Partner>>(Partners);
        }

        private class FakeKafkaProducerService : IKafkaProducerService
        {
            public List<string> Payloads { get; } = [];

            public Task PublishAsync(string encryptedPayload, CancellationToken cancellationToken)
            {
                Payloads.Add(encryptedPayload);
                return Task.CompletedTask;
            }
        }
        private sealed class FakeSkyTeamAutoAccrualKafkaPublisher : ISkyTeamAutoAccrualKafkaPublisher
        {
            private readonly FakeKafkaProducerService _kafkaProducerService;
            public FakeSkyTeamAutoAccrualKafkaPublisher(FakeKafkaProducerService kafkaProducerService)
            {
                _kafkaProducerService = kafkaProducerService;
            }
            public async Task PublishAsync(DistanceBasedAccrualPayload payload, CancellationToken cancellationToken)
            {
                var serializedPayload = Json.Serialize(payload);
                await _kafkaProducerService.PublishAsync(serializedPayload, cancellationToken);
            }
        }
    }
}
