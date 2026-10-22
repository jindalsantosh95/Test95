using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class RetrieveCountryDetailsRepoQueryHandlerTests
    {
        private readonly Mock<ILogger<RetrievePaymentDetailsRepoQueryHandler>> _mockLogger1;
        private readonly Mock<IHttpConnector> _mockAPIConnector;
        private readonly RetrieveCountryDetailsRepoQueryHandler _mockHandler;


        public RetrieveCountryDetailsRepoQueryHandlerTests()
        {
            _mockLogger1 = new Mock<ILogger<RetrievePaymentDetailsRepoQueryHandler>>();
            _mockAPIConnector = new Mock<IHttpConnector>();
            _mockHandler = new RetrieveCountryDetailsRepoQueryHandler(_mockAPIConnector.Object, _mockLogger1.Object);

            CRMTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAPICOnnectorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrieveCountryDetailsRepoQueryHandler(null, _mockLogger1.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrieveCountryDetailsRepoQueryHandler(_mockAPIConnector.Object, null));
        }
    }
}
