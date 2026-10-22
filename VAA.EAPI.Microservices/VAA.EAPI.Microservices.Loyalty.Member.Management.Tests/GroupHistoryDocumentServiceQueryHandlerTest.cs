using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class GroupHistoryDocumentServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<GroupHistoryDocumentServiceQueryHandler>> _mockLogger;
        private readonly GroupHistoryDocumentServiceQueryHandler _handler;

        public GroupHistoryDocumentServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<GroupHistoryDocumentServiceQueryHandler>>();
            _handler = new GroupHistoryDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.CustomerTxnMSDetailsEndpoint, "https://testurl.api.com");
            Environment.SetEnvironmentVariable(Constants.Application.FetchGroupDocumentsByHeadCustomerId, "test");
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GroupHistoryDocumentServiceQueryHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new GroupHistoryDocumentServiceQueryHandler(_mockApiConnector.Object, null));
        }
    }
}
