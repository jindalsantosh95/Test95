using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CustomerDetailsServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<CustomerDetailsServiceQueryHandler>> _mockLogger;
        private readonly CustomerDetailsServiceQueryHandler _handler;

        public CustomerDetailsServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<CustomerDetailsServiceQueryHandler>>();
            _handler = new CustomerDetailsServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerDetailsServiceQueryHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CustomerDetailsServiceQueryHandler(_mockApiConnector.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerShouldThrowException()
        {
            string customerID = "CustomerId";
            var request = new CustomerDetailsServiceQuery() { CustomerId = customerID };
            var response = TestDataProvider.GetTestData<CustomerDTO>("MemberManagement\\CustomerDtoResponse");
            _mockApiConnector.Setup(m => m.GetAsync<CustomerDTO>(It.IsAny<string>(),
                                                                    It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).ReturnsAsync(response);
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task HandlerShouldThrowExceptionWhenInputIsNull()
        {
            var request = new CustomerDetailsServiceQuery() { CustomerId = null };
            var response = TestDataProvider.GetTestData<CustomerDTO>("MemberManagement\\CustomerDtoResponse");
            _mockApiConnector.Setup(m => m.GetAsync<CustomerDTO>(It.IsAny<string>(),
                                                                    It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).ReturnsAsync(response);
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
