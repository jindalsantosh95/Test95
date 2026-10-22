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
    public class GetMemberAccountDetailsQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<GetMemberAccountDetailsQueryHandler>> _mockLogger;
        private readonly GetMemberAccountDetailsQueryHandler _handler;

        public GetMemberAccountDetailsQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<GetMemberAccountDetailsQueryHandler>>();
            _handler = new GetMemberAccountDetailsQueryHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetMemberAccountDetailsQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new GetMemberAccountDetailsQueryHandler(null, _mockLogger.Object));
        }
        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var request = TestDataProvider.GetTestData<GetMemberAccountDetailsQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            var memberAccountResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDocumentData");
            var referenceDataResponse = TestDataProvider.GetTestData<MemberAccountReferenceData>("MemberManagement\\RetrieveMemberAccountReferenceData");
            _mockMediator.Setup(m => m.Send(It.IsAny<MemberAccountRequestQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<MemberAccountResponse> { Status = Status.OK, Value = memberAccountResponse });
            _mockMediator.Setup(m => m.Send(It.IsAny<MemberAccountReferenceDataQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<MemberAccountReferenceData> { Status = Status.OK, Value = referenceDataResponse });
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task HandlerSuccessWithException()
        {
            var request = TestDataProvider.GetTestData<GetMemberAccountDetailsQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            var memberAccountResponse = TestDataProvider.GetTestData<MemberAccountResponse>("MemberManagement\\RetrieveMemberAccountDocumentData");
            var referenceDataResponse = TestDataProvider.GetTestData<MemberAccountReferenceData>("MemberManagement\\RetrieveMemberAccountReferenceData");
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
    }
}
