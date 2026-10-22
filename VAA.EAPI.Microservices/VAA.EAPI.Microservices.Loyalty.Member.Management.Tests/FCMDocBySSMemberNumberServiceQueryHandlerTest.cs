using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class FCMDocBySSMemberNumberServiceQueryHandlerTest
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;
        private readonly Mock<ILogger<FCMDocBySSMemberNumberServiceQueryHandler>> _mockLogger;
        private readonly FCMDocBySSMemberNumberServiceQueryHandler _handler;

        public FCMDocBySSMemberNumberServiceQueryHandlerTest()
        {
            _mockApiConnector = new Mock<IHttpConnector>();
            _mockLogger = new Mock<ILogger<FCMDocBySSMemberNumberServiceQueryHandler>>();
            _handler = new FCMDocBySSMemberNumberServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldSucceed()
        {
            Assert.NotNull(new FCMDocBySSMemberNumberServiceQueryHandler(_mockApiConnector.Object, _mockLogger.Object));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task HandlerShouldThrowException()
        {
            var request = TestDataProvider.GetTestData<FCMDocBySSMemberNumberServiceQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            var flyingclubResponse = TestDataProvider.GetTestData<FlyingClubMemberResponse>("MemberManagement\\FlyingclubMemberResponse");
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
