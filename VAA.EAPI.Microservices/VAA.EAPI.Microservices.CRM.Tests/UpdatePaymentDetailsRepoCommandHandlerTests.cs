using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class UpdatePaymentDetailsRepoCommandHandlerTests
    {
        private readonly Mock<ILogger<UpdatePaymentDetailsRepoCommandHandler>> _mockLogger1;
        private readonly UpdatePaymentDetailsRepoCommandHandler _mockHandler;
        public Mock<IHttpConnector> _mockAPIConnector;

        public UpdatePaymentDetailsRepoCommandHandlerTests()
        {
            _mockLogger1 = new Mock<ILogger<UpdatePaymentDetailsRepoCommandHandler>>();
            _mockAPIConnector = new Mock<IHttpConnector>();
            _mockHandler = new UpdatePaymentDetailsRepoCommandHandler(_mockLogger1.Object, _mockAPIConnector.Object);

            CRMTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAPIConnectorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentDetailsRepoCommandHandler(_mockLogger1.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentDetailsRepoCommandHandler(null, _mockAPIConnector.Object));
        }
    }
}
