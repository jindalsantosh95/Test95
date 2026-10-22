using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class FlyingClubMemberDocumentServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<FlyingClubMemberDocumentServiceQueryHandler>> _mockLogger;
        private readonly FlyingClubMemberDocumentServiceQueryHandler _handler;

        public FlyingClubMemberDocumentServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<FlyingClubMemberDocumentServiceQueryHandler>>();
            _handler = new FlyingClubMemberDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldSucceed()
        {
            Assert.NotNull(new FlyingClubMemberDocumentServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object));
        }
    }
}
