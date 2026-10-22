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
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CustomerSearchDocumentServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<CustomerSearchDocumentServiceQueryHandler>> _mockLogger;
        private readonly CustomerSearchDocumentServiceQueryHandler _handler;

        public CustomerSearchDocumentServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<CustomerSearchDocumentServiceQueryHandler>>();
            _handler = new CustomerSearchDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerSearchDocumentServiceQueryHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CustomerSearchDocumentServiceQueryHandler(_mockApiConnector.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerShouldThrowException()
        {
            var request = new CustomerSearchDocumentServiceQuery() { MemberNumber = "1074670470" };
            var response = TestDataProvider.GetTestData<CustomerSearch>("MemberManagement\\CustomerSearchDocument");

            _mockApiConnector.Setup(m => m.GetAsync<CustomerSearch>(It.IsAny<string>(),
                                                                    It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).ReturnsAsync(response);

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task HandlerShouldThrowNullException()
        {
            var request = new CustomerSearchDocumentServiceQuery() { MemberNumber = null };
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
