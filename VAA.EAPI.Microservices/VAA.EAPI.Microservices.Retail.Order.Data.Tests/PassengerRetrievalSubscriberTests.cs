using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.PassengerRetrieval;
using VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class PassengerRetrievalSubscriberTests
    {
        private readonly Mock<IPassengerRetrieval> _mockPassengerRetrieval;
        private readonly PassengerRetrievalSubscriber _subscriber;

        public PassengerRetrievalSubscriberTests()
        {
            _mockPassengerRetrieval = new Mock<IPassengerRetrieval>();
            _subscriber = new PassengerRetrievalSubscriber(_mockPassengerRetrieval.Object);
        }

        [Fact]
        public async Task Run_Should_Call_GetPassengerFlightDetailAsync_With_Valid_CloudEvent()
        {
            // Arrange
            var eventPayload = "{\"Specversion\":\"1.0\",\"Type\":\"VAA.NDC.Order.InitiateOrderChange\",\"Source\":\"source\",\"Id\":\"id\",\"Time\":\"2023-10-01T12:00:00Z\"}";
            var cloudEvent = Json.Deserialize<CloudEvent>(eventPayload);
            var cancellationToken = CancellationToken.None;

            _mockPassengerRetrieval
                .Setup(x => x.GetPassengerFlightDetailAsync(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _subscriber.Run(eventPayload, cancellationToken);

            // Assert
            _mockPassengerRetrieval.Verify(x => x.GetPassengerFlightDetailAsync(It.Is<CloudEvent>(e => e.Id == cloudEvent.Id), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Run_Should_Throw_Exception_When_EventPayload_Is_Invalid()
        {
            // Arrange
            var invalidEventPayload = "InvalidPayload";
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(() => _subscriber.Run(invalidEventPayload, cancellationToken));
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_PassengerRetrieval_Is_Null()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PassengerRetrievalSubscriber(null));
        }
    }
}

