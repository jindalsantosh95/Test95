using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Trip.Tests
{
    public class MessageCentreServiceTests
    {
        private readonly Mock<IDeltaMessageDetailsService> _deltaMessageDetailsService;
        private readonly Mock<IDeltaMessageDetailsPrivateService> _deltaMessageDetailsPrivateService;
        private readonly ILogger<MessageCentreService> _loggerService;

        public MessageCentreServiceTests()
        {
            _deltaMessageDetailsService = new Mock<IDeltaMessageDetailsService>();
            _deltaMessageDetailsPrivateService = new Mock<IDeltaMessageDetailsPrivateService>();
            _loggerService = Microsoft.Extensions.Logging.Abstractions.NullLogger<MessageCentreService>.Instance;
            TripMSEnvironmentSetup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void GetMessageDetails_ValidateMessagesDataTest()
        {
            // Arrange  
            var dlMessageCentreResponse = TestDataProvider.GetTestData<MessageResponse>("TripsMS\\DeltaMessageCentreFlyReadyResponse");
            var tripBookingID = Guid.NewGuid().ToString();


            _deltaMessageDetailsService.Setup(x => x.GetMessageDetailsAsync(tripBookingID)).Returns(Task
                .FromResult(dlMessageCentreResponse));
            _deltaMessageDetailsPrivateService.Setup(x => x.GetMessageDetailsAsync(tripBookingID)).Returns(Task
                .FromResult(dlMessageCentreResponse));
            var service = new MessageCentreService(_deltaMessageDetailsService.Object, _deltaMessageDetailsPrivateService.Object, _loggerService);

            // Act  
            var result = service.GetMessageDetails(tripBookingID).Result;

            // Assert  
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(dlMessageCentreResponse.Passengers.Count(), result.Value.Value.Passengers.Count());
            Assert.Equal(dlMessageCentreResponse.Trips.Count(), result.Value.Value.Trips.Count());
            Assert.Equal(dlMessageCentreResponse.Messages.Count(), result.Value.Value.Messages.Count());
        }
    }
}