using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.MemberAccountDetails;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberAccountReferenceDataCommandHandlerTest
    {
        private readonly Mock<ICachedMemberAccountReferenceData> _mockCachedMemberAccountReferenceData;
        private readonly Mock<ILogger<MemberAccountReferenceDataCommandHandler>> _mockLogger;
        private readonly MemberAccountReferenceDataCommandHandler _handler;
        private readonly Mock<FunctionContext> _context;

        public MemberAccountReferenceDataCommandHandlerTest()
        {
            _mockCachedMemberAccountReferenceData = new Mock<ICachedMemberAccountReferenceData>();
            _context = new Mock<FunctionContext>();
            _mockLogger = new Mock<ILogger<MemberAccountReferenceDataCommandHandler>>();
            _handler = new MemberAccountReferenceDataCommandHandler(_mockCachedMemberAccountReferenceData.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountReferenceDataCommandHandler(null, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new MemberAccountReferenceDataCommandHandler(_mockCachedMemberAccountReferenceData.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithNoError()
        {
            var req = new MemberAccountReferenceDataCommand();
            var referenceDataResponse = TestDataProvider.GetTestData<MemberAccountReferenceData>("MemberManagement\\RetrieveMemberAccountReferenceData");
            _mockCachedMemberAccountReferenceData.Setup(x => x.GetCachedMemberAccountReferenceData()).ReturnsAsync(new Response<MemberAccountReferenceData> { Status = Status.OK, Value = referenceDataResponse });
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldSuccessWithException()
        {
            var req = new MemberAccountReferenceDataCommand();
            var referenceDataResponse = TestDataProvider.GetTestData<MemberAccountReferenceData>("MemberManagement\\RetrieveMemberAccountReferenceData");
            _mockCachedMemberAccountReferenceData.Setup(x => x.GetCachedMemberAccountReferenceData()).Throws(new Exception());
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.NotNull(result.Exception);
        }
    }
}
