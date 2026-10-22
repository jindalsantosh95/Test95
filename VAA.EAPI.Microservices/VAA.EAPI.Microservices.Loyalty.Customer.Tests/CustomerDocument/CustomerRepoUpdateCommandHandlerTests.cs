using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerDocument
{
    public class CustomerRepoUpdateCommandHandlerTests
    {
        public Mock<ICustomerService<CustomersDTO>> _customerService;
        protected readonly Mock<ILogger<CustomerRepoUpdateCommandHandler>> _mockLogger;
        private readonly CustomerRepoUpdateCommandHandler _handler;

        public CustomerRepoUpdateCommandHandlerTests()
        {
            _customerService = new Mock<ICustomerService<CustomersDTO>>();
            _mockLogger = new Mock<ILogger<CustomerRepoUpdateCommandHandler>>();
            _handler = new CustomerRepoUpdateCommandHandler(_customerService.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerRepoUpdateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerRepoUpdateCommandHandler(_customerService.Object, null));
        }

        [Fact]
        public async Task CustomerRepoUpdateCommandHandlerSuccess()
        {
            var customerDocument = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc");

            _customerService.Setup(m => m.ReplaceItemAsync(new CustomersDTO(customerDocument), DocumentType.Customer))
                .Equals(true);
            CustomerRepoUpdateCommand request = new CustomerRepoUpdateCommand(customerDocument);
            var result = await _handler.Handle(request, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CustomerRepoUpdateCommandHandlerFailure()
        {
            var customerDocument = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc");

            _customerService.Setup(m => m.ReplaceItemAsync(new CustomersDTO(customerDocument),  DocumentType.Customer))
                .Throws(new InvalidOperationException());
            CustomerRepoUpdateCommand request = new CustomerRepoUpdateCommand(customerDocument);
            var result = await _handler.Handle(request, new CancellationTokenSource().Token);

            Assert.True(result.IsSuccess);
        }

    }
}
