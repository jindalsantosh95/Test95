using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberAccountDetailsResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<MemberAccountDetailsQueryHandler>> _mockLogger;
        private readonly MemberAccountDetailsQueryHandler _handler;

        public MemberAccountDetailsResponseQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<MemberAccountDetailsQueryHandler>>();
            _handler = new MemberAccountDetailsQueryHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountDetailsQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountDetailsQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var memberAccountResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDetailsResponse");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMemberAccountDetailsQuery>(), CancellationToken.None)).ReturnsAsync(new Response<MemberAccountResponse> { Status = Status.OK, Value = memberAccountResponse });
            MemberAccountDetailsQuery query = new() { CustomerId = customerID };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task HandlerSuccessWithError()
        {
            var memberAccountResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDetailsResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMemberAccountDetailsQuery>(), CancellationToken.None)).ReturnsAsync(new Response<MemberAccountResponse> { Status = Status.OK, Value = memberAccountResponse });
            MemberAccountDetailsQuery query = new() { CustomerId = null };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var memberAccountResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDetailsResponse");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMemberAccountDetailsQuery>(), CancellationToken.None)).ThrowsAsync(new Exception());
            MemberAccountDetailsQuery query = new() { CustomerId = customerID };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
    }
}
