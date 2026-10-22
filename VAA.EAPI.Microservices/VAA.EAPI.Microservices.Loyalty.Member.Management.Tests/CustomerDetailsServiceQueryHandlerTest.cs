using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
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
            Environment.SetEnvironmentVariable(Constants.Environment.CustomerMSUrlGetbyCustID, "https://testurl.api.com/customer/{customerid}");
            Environment.SetEnvironmentVariable(Constants.Environment.CustomerMSUrlGetByMemNum, "https://testurl.api.com/customer/loyalty/{memberid}");
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
    }
}
