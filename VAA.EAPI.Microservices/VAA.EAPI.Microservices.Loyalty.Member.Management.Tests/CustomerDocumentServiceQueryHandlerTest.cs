using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using Xunit;
using Xunit.Sdk;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CustomerDocumentServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<CustomerDocumentServiceQueryHandler>> _mockLogger;
        private readonly CustomerDocumentServiceQueryHandler _handler;

        public CustomerDocumentServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<CustomerDocumentServiceQueryHandler>>();
            _handler = new CustomerDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.CustomerMSUrl, "https://testurl.api.com/customer/{customerid}");
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerDocumentServiceQueryHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CustomerDocumentServiceQueryHandler(_mockApiConnector.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerShouldThrowException()
        {
            string customerID = "CustomerId";
            var request = new CustomerDocumentServiceQuery() { CustomerId = customerID };
            var response = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            _mockApiConnector.Setup(m => m.GetAsync<Customer>(It.IsAny<string>(),
                                                                    It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).ReturnsAsync(response);
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerShouldThrowNullException()
        {
            var request = new CustomerDocumentServiceQuery() { CustomerId = null };
            await Assert.ThrowsAsync<NullException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
