using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public sealed class FlownPassengerCommandHandlerTests
    {
        public FlownPassengerCommandHandlerTests()
        {
            ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "test-service-bus-connection");
        }

        [Fact]
        public async Task Handle_PublishesOnlySuccessfullyInsertedFlights()
        {
            var insertedFlight = CreateFlownPassenger("VS", "0209", "ICN", "LHR", new DateTime(2026, 9, 27));
            var skippedFlight = CreateFlownPassenger("VS", "0001", "LHR", "JFK", new DateTime(2026, 9, 28));
            var repository = new FakeFlownDbRepository { SuccessfullyInsertedPassengers = [insertedFlight] };
            var messageBusFactory = new CapturingMessageBusFactory();
            var handler = CreateHandler(repository, messageBusFactory);

            await handler.Handle(CreateCloudEvent([insertedFlight, skippedFlight]), CancellationToken.None);

            var message = Assert.Single(messageBusFactory.Messages);
            var publishedEvent = Assert.IsType<AutoAccrualMessage>(message);
            Assert.Equal(Constants.Application.AutoAccrualMessage, publishedEvent.Type);
            Assert.Equal(Constants.Application.AutoAccrualMessage, publishedEvent.Subject);
            Assert.Equal("0209", publishedEvent.Data?.FlightNumber);
            Assert.Equal("ICN", publishedEvent.Data?.DepartureAirport);
            Assert.Equal("LHR", publishedEvent.Data?.ArrivalAirport);
        }

        [Fact]
        public async Task Handle_PublishesOneMessageForDuplicateFlightPassengers()
        {
            var firstPassenger = CreateFlownPassenger("VS", "0209", "ICN", "LHR", new DateTime(2026, 9, 27));
            var secondPassenger = CreateFlownPassenger("VS", "0209", "ICN", "LHR", new DateTime(2026, 9, 27));
            var repository = new FakeFlownDbRepository { SuccessfullyInsertedPassengers = [firstPassenger, secondPassenger] };
            var messageBusFactory = new CapturingMessageBusFactory();
            var handler = CreateHandler(repository, messageBusFactory);

            await handler.Handle(CreateCloudEvent([firstPassenger, secondPassenger]), CancellationToken.None);

            Assert.Single(messageBusFactory.Messages);
        }

        [Fact]
        public async Task Handle_DoesNotPublish_WhenNoRowsWereInserted()
        {
            var flownPassenger = CreateFlownPassenger("VS", "0209", "ICN", "LHR", new DateTime(2026, 9, 27));
            var repository = new FakeFlownDbRepository();
            var messageBusFactory = new CapturingMessageBusFactory();
            var handler = CreateHandler(repository, messageBusFactory);

            await handler.Handle(CreateCloudEvent([flownPassenger]), CancellationToken.None);

            Assert.Empty(messageBusFactory.Messages);
        }

        [Fact]
        public async Task Handle_Throws_WhenPublishingFails()
        {
            var flownPassenger = CreateFlownPassenger("VS", "0209", "ICN", "LHR", new DateTime(2026, 9, 27));
            var repository = new FakeFlownDbRepository { SuccessfullyInsertedPassengers = [flownPassenger] };
            var messageBusFactory = new CapturingMessageBusFactory { ThrowOnPublish = true };
            var handler = CreateHandler(repository, messageBusFactory);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(CreateCloudEvent([flownPassenger]), CancellationToken.None));
        }

        private static FlownPassengerCommandHandler CreateHandler(FakeFlownDbRepository repository, CapturingMessageBusFactory messageBusFactory) =>
            new(
                repository,
                new ServiceBusTopicMessageEventCommandHandler(messageBusFactory),
                NullLogger<FlownPassengerCommandHandler>.Instance);

        private static CloudEvent CreateCloudEvent(IEnumerable<FlownPassengerRequest> flownPassengers) =>
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Data = Convert.ToBase64String(Json.Serialize(flownPassengers).Compress())
            };

        private static FlownPassengerRequest CreateFlownPassenger(string carrier, string flightNumber, string departureAirport, string arrivalAirport, DateTime departureDate) =>
            new()
            {
                EventFlight = new Flight
                {
                    FlightCarrier = carrier,
                    FlightNumber = flightNumber,
                    DepartureAirport = departureAirport,
                    ArrivalAirport = arrivalAirport,
                    DepartureDate = departureDate
                }
            };

        private sealed class CapturingMessageBusFactory : IMessageBusFactory
        {
            public List<CloudEvent> Messages { get; } = [];
            public bool ThrowOnPublish { get; init; }

            public IMessageBus GetClient(string connectionString, string sender) => new CapturingMessageBus(Messages, ThrowOnPublish);
        }

        private sealed class CapturingMessageBus(List<CloudEvent> messages, bool throwOnPublish) : IMessageBus
        {
            public Task PublishMessageAsync(CloudEvent @event)
            {
                if (throwOnPublish)
                {
                    throw new InvalidOperationException("Publishing failed.");
                }

                messages.Add(@event);
                return Task.CompletedTask;
            }
        }

        private sealed class FakeFlownDbRepository : IFlownDbRepository
        {
            public IReadOnlyCollection<FlownPassengerRequest> SuccessfullyInsertedPassengers { get; init; } = [];

            public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default) =>
                Task.FromResult(SuccessfullyInsertedPassengers);

            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken) =>
                throw new NotSupportedException();

            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken) =>
                throw new NotSupportedException();

            public Task<RetroCreditFlownDetails> GetFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest) =>
                throw new NotSupportedException();

            public Task<bool> ResetPendingFeedbackAutoRetroAsync(string days) => throw new NotSupportedException();

            public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(RetroCreditRequest retroCreditRequest, string uniqueNumber) =>
                throw new NotSupportedException();

            public Task<bool> UpdateFlownDetailsWithRetroFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest) =>
                throw new NotSupportedException();

            public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message) =>
                throw new NotSupportedException();

            public Task<bool> UpdateTicketStatus(string id, string ticketNumber, string ticketStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode) =>
                throw new NotSupportedException();
        }
    }
}
