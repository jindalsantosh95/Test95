using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerSearchDocument
{
    public class CustomerSearchRepoUpdateCommandHandlerTests
    {
        public Mock<ICustomerService<CustomerSearchDTO>> _customerService;
        protected readonly Mock<ILogger<CustomerSearchRepoUpdateCommandHandler>> _mockLogger;
        private readonly CustomerSearchRepoUpdateCommandHandler _handler;

        public CustomerSearchRepoUpdateCommandHandlerTests()
        {
            _customerService = new Mock<ICustomerService<CustomerSearchDTO>>();
            _mockLogger = new Mock<ILogger<CustomerSearchRepoUpdateCommandHandler>>();
            _handler = new CustomerSearchRepoUpdateCommandHandler(_customerService.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchRepoUpdateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchRepoUpdateCommandHandler(_customerService.Object, null));
        }

        [Fact]
        public async Task CustomerSearchRepoUpdateCommandHandlerSuccess()
        {
            var customerSearchDocument = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument");

            _customerService.Setup(m => m.GetItemAsync(It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
                .Equals(true);
            CustomerSearchRepoUpdateCommand request = new CustomerSearchRepoUpdateCommand(customerSearchDocument);
            var result = await _handler.Handle(request, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CustomerSearchRepoUpdateCommandHandlerFailure()
        {
            var customerSearchDocument = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument");

            _customerService.Setup(m => m.GetItemAsync(It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());
            CustomerSearchRepoUpdateCommand request = new CustomerSearchRepoUpdateCommand(customerSearchDocument);
            var result = await _handler.Handle(request, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }
    }
}
