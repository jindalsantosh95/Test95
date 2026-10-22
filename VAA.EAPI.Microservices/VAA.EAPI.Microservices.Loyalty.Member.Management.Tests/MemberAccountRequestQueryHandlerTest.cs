using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberAccountRequestQueryHandlerTest
    {
        private readonly Mock<IMemberAccountDetailsService> _mockMemberAccountService;
        private readonly Mock<ILogger<MemberAccountRequestQueryHandler>> _mockLogger;
        private readonly MemberAccountRequestQueryHandler _handler;

        public MemberAccountRequestQueryHandlerTest()
        {
            _mockMemberAccountService = new Mock<IMemberAccountDetailsService>();
            _mockLogger = new Mock<ILogger<MemberAccountRequestQueryHandler>>();
            _handler = new MemberAccountRequestQueryHandler(_mockMemberAccountService.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountRequestQueryHandler(_mockMemberAccountService.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountRequestQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerShouldSuccessWithNoError()
        {
            var memberAccoutnResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDetailsResponse");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            var request = new MemberAccountRequestQuery() { CustomerId = customerID };
            _mockMemberAccountService.Setup(m => m.GetMemberAccountDetails(request)).ReturnsAsync(new Response<MemberAccountResponse> { Status = Status.OK, Value = memberAccoutnResponse });
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task HandlerShouldFailWithException()
        {
            var memberAccoutnResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDetailsResponse");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            var request = new MemberAccountRequestQuery() { CustomerId = customerID };
            _mockMemberAccountService.Setup(m => m.GetMemberAccountDetails(request)).Throws(new Exception());
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
