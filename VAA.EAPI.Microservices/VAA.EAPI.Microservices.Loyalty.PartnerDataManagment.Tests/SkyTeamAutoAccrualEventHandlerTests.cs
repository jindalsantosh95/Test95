using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
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
    public class SkyTeamAutoAccrualEventHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public SkyTeamAutoAccrualEventHandlerTests()
        {
            ConfigReader.Set(Constants.Environment.SkyTeamPgpPublicKey, "fake-public-key");
            ConfigReader.Set(Constants.Environment.AccrualPgpEncryptionKeyId, "test-key-id");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ISkyTeamAutoAccrualEventHandler, SkyTeamAutoAccrualEventHandler>()
                .AddSingleton<FakeFlownDbServices>()
                .AddSingleton<IFlownDbServices>(provider => provider.GetRequiredService<FakeFlownDbServices>())
                .AddSingleton<IPartnerDataService>(new FakePartnerDataService())
                .AddSingleton<IPgpEncryptionService, FakePgpEncryptionService>()
                .AddSingleton<IKafkaProducerService, FakeKafkaProducerService>()
                .AddSingleton<ISkyTeamAutoAccrualKafkaPublisher, FakeSkyTeamAutoAccrualKafkaPublisher>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ValidMessage_ReturnsTrue()
        {
            var handler = _serviceProvider
                .GetRequiredService<ISkyTeamAutoAccrualEventHandler>();

            var payload = new AutoAccrualPayload
            {
                FlightCarrier = "VS",
                FlightNumber = "310",
                DepartureAirport = "JFK",
                ArrivalAirport = "DEL",
                DepartureDate = "2026-08-10"
            };

            var result = await handler.Handle(payload, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task Handle_ReturnsFalse_WhenFlownDbLookupFails()
        {
            var flownDbServices = _serviceProvider.GetRequiredService<FakeFlownDbServices>();
            flownDbServices.ExceptionToThrow = new InvalidOperationException("Flown DB is unavailable.");

            var handler = _serviceProvider.GetRequiredService<ISkyTeamAutoAccrualEventHandler>();

            var result = await handler.Handle(new AutoAccrualPayload(), CancellationToken.None);

            Assert.False(result);
        }

        private sealed class FakeFlownDbServices : IFlownDbServices
        {
            public Exception? ExceptionToThrow { get; set; }

            public Task<RetroCreditFlownDetails> GetUniquePassengerFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest) =>
                throw new NotSupportedException();

            public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(string uniqueIdentificationNumber, RetroCreditRequest retroCreditRequest) =>
                throw new NotSupportedException();

            public Task<bool> UpdateFlownDetailsWithFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest) =>
                throw new NotSupportedException();

            public Task<bool> ResetPendingFeedbackFlownDetailsAsync(string days) =>
                throw new NotSupportedException();

            public Task<bool> UpdateTicketStatus(int id, string ticketNumber, string claimStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode) =>
                throw new NotSupportedException();

            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken)
            {
                if (ExceptionToThrow is not null)
                {
                    throw ExceptionToThrow;
                }

                return Task.FromResult<IEnumerable<AutoAccrualFlownDetails>>(
                [
                    new AutoAccrualFlownDetails
                    {
                        TicketNumber = "123456789",
                        ProcessingStatus = "FREE",
                        FdNoShow = "N",
                        FFPCarrierCode = ffpCarrier,
                        FFPAccountNumber = "123456",
                        FirstName = "John",
                        LastName = "Smith",
                        MarketingAirlineCode = "DL",
                        MarketingFlightNum = "310",
                        OperatingAirlineCode = "VS",
                        DepartureDate = "20260810",
                        OriginPort = "JFK",
                        DestinationPort = "DEL",
                        CouponNumber = "1",
                        FlownCabinClass = "Y",
                        MarketingCarrierBookingClass = "Y",
                        OperatingCarrierBookingClass = "Y",
                        OperatingAirAuthNumber = "000106337233331",
                        SentToPartner = "N"
                    }
                ]);
            }

            public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message)
            {
                return Task.FromResult(true);
            }

        }

        private sealed class FakePartnerDataService : IPartnerDataService
        {
            private static readonly List<Partner> Partners =
            [
                new Partner
                {
                    PartnerCode = "KQ"
                }
            ];

            public Task<IReadOnlyCollection<Partner>> GetSkyTeamPartnersAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Partner>>(Partners);
        }

        private sealed class FakePgpEncryptionService : IPgpEncryptionService
        {
            public string Encrypt(string payload, string publicKey)
            {
                return payload;
            }
        }

        private sealed class FakeKafkaProducerService : IKafkaProducerService
        {
            public Task PublishAsync(string encryptedPayload, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class FakeSkyTeamAutoAccrualKafkaPublisher : ISkyTeamAutoAccrualKafkaPublisher
        {
            public Task PublishAsync(DistanceBasedAccrualPayload payload, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
