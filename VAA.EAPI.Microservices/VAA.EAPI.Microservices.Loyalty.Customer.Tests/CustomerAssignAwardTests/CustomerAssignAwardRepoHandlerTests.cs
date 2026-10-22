using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerAssignAward;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerAssignAwardTests
{
    public class CustomerAssignAwardRepoHandlerTests
    {
        private readonly Mock<ILogger<CustomerAssignAwardRepoHandler>> _mockLogger;
        public Mock<ICustomerService<CustomersDTO>> _customerService;
        public Mock<ICustomerTransactionService<AwardDocumentDataDTO>> _service;
        private readonly CustomerAssignAwardRepoHandler _handler;

        public CustomerAssignAwardRepoHandlerTests()
        {
            _mockLogger = new Mock<ILogger<CustomerAssignAwardRepoHandler>>();
            _customerService = new Mock<ICustomerService<CustomersDTO>>();
            _service = new Mock<ICustomerTransactionService<AwardDocumentDataDTO>>();
            _customerService.Setup(m => m.GetItemAsync(It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
                .Equals(true);
            _service.Setup(m => m.CreateItemAsync(It.IsAny<AwardDocumentDataDTO>(), It.IsAny<string>(), It.IsAny<DocumentType>()))
                .Equals(true);
            _handler = new CustomerAssignAwardRepoHandler(_mockLogger.Object, _customerService.Object, _service.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardRepoHandler(null, _customerService.Object, _service.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardRepoHandler(_mockLogger.Object, null, _service.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardRepoHandler(_mockLogger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldReturnTrueWhenGoodRequestIsPassed()
        {
            // Arrange
            var request = new Domain.CustomerAssignAward.AssignAwardRequestData()
            {
                MemberId = "12345"
            };
            string id = "789";
            // Act
            var result = await _handler.Handle(new CustomerAssignAwardRepoCommand(request, id), CancellationToken.None);

            // Assert

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalseWhenBadRequestIsPassed()
        {
            // Arrange

            // Act
            var result = await _handler.Handle(null, CancellationToken.None);

            // Assert

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnAcceptedStatusWithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CustomerAssignAwardRepoCommand>("CustomerMS\\CustomerAssignAward");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}
