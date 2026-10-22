using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MembershipChangeDocumentServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<MembershipChangeDocumentServiceQueryHandler>> _mockLogger;
        private readonly MembershipChangeDocumentServiceQueryHandler _handler;

        public MembershipChangeDocumentServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<MembershipChangeDocumentServiceQueryHandler>>();
            _handler = new MembershipChangeDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldSucceed()
        {
            Assert.NotNull(new MembershipChangeDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object));
        }
    }
}
