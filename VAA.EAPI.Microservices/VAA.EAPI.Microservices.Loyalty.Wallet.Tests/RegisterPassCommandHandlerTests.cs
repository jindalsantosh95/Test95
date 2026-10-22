using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Tests
{
    public class RegisterPassCommandHandlerTests
    {
        private readonly RegisterPassCommandHandler _handler;
        private readonly Mock<ITableStorageService<PassEntities>> _tableStorageService;
        private readonly Mock<ILogger<RegisterPassCommandHandler>> _logger;

        public RegisterPassCommandHandlerTests()
        {
            _tableStorageService = new Mock<ITableStorageService<PassEntities>>();
            _logger = new Mock<ILogger<RegisterPassCommandHandler>>();

            _handler = new RegisterPassCommandHandler(_tableStorageService.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTableStorageServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RegisterPassCommandHandler(null, _logger.Object));
        }

        [Fact]
        public async Task ShouldReturnSuccessResponse_ForValidRequest()
        {
            //Arrange
            var request = new RegisterPassCommand()
            {
                DeviceId = "123",
                PassSerial = "9878764567",
                PassTypeId = "Test@fly@virgin.com",
                PushToken = "JNIOJK233ED34JIOJ"
            };

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<int>(result.Value);
            Assert.Equal(201, result.Value);
            Assert.Equal(Status.OK, result.Status);
            Assert.Null(result.Errors);

        }
    }
}
