using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.AddressHistoryDocument
{
    public class AddressHistoryRepoCreateCommandHandlerTests
    {
        public Mock<ICustomerService<AddressHistoryDTO>> _customerService;
        protected readonly Mock<ILogger<AddressHistoryRepoCreateCommandHandler>> _mockLogger;
        private readonly AddressHistoryRepoCreateCommandHandler _handler;

        public AddressHistoryRepoCreateCommandHandlerTests()
        {
            _customerService = new Mock<ICustomerService<AddressHistoryDTO>>();
            _mockLogger = new Mock<ILogger<AddressHistoryRepoCreateCommandHandler>>();
            _handler = new AddressHistoryRepoCreateCommandHandler(_customerService.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryRepoCreateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryRepoCreateCommandHandler(_customerService.Object, null));
        }

        [Fact]
        public async Task AddressHistoryRepoCreateCommandHandlerSuccess()
        {
            var addressHistoryDocument = TestDataProvider.GetTestData<AddressHistory>("CustomerMS\\AddressHistory\\AddressHistoryDocument");

            _customerService.Setup(m => m.CreateItemAsync(new AddressHistoryDTO(addressHistoryDocument), It.IsAny<string>(), DocumentType.AddressHistory))
                .Equals(true);
            AddressHistoryRepoCreateCommand request = new AddressHistoryRepoCreateCommand(addressHistoryDocument);
            var result = await _handler.Handle(request, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }
    }
}
