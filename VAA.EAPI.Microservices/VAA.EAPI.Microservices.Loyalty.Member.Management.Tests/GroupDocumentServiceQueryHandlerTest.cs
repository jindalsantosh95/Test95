using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class GroupDocumentServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<GroupDocumentServiceQueryHandler>> _mockLogger;
        private readonly GroupDocumentServiceQueryHandler _handler;

        public GroupDocumentServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<GroupDocumentServiceQueryHandler>>();
            _handler = new GroupDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.CustomerTxnMSDetailsEndpoint, "https://testurl.api.com");
            Environment.SetEnvironmentVariable(Constants.Application.FetchGroupDocumentsByHeadCustomerId, "test");
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GroupDocumentServiceQueryHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new GroupDocumentServiceQueryHandler(_mockApiConnector.Object, null));
        }
    }
}

