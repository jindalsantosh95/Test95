using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class RetrievePaymentDetailsRepoQueryHandlerTests
    {
        private readonly Mock<ILogger<RetrievePaymentDetailsRepoQueryHandler>> _mockLogger1;
        private readonly RetrievePaymentDetailsRepoQueryHandler _mockHandler;
        public Mock<IHttpConnector> _mockAPIConnector;

        public RetrievePaymentDetailsRepoQueryHandlerTests()
        {
            _mockLogger1 = new Mock<ILogger<RetrievePaymentDetailsRepoQueryHandler>>();
            _mockAPIConnector = new Mock<IHttpConnector>();
            _mockHandler = new RetrievePaymentDetailsRepoQueryHandler(_mockAPIConnector.Object, _mockLogger1.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAPIConnectorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrievePaymentDetailsRepoQueryHandler(null, _mockLogger1.Object));

        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrievePaymentDetailsRepoQueryHandler(_mockAPIConnector.Object, null));
        }
    }
}
