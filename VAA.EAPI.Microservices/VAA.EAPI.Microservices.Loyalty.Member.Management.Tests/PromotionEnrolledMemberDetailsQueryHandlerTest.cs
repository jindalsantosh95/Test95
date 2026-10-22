using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class PromotionEnrolledMemberDetailsQueryHandlerTest
    {
        private readonly Mock<IReportingDBServices> _reportingDBService;
        private readonly Mock<IPromotionEnrolledMemberService> _promotionEnrolledMemberService;
        private readonly Mock<ILogger<PromotionEnrolledMemberDetailsQueryHandler>> _mockLogger;
        private readonly PromotionEnrolledMemberDetailsQueryHandler _handler;

        public PromotionEnrolledMemberDetailsQueryHandlerTest()
        {

            _mockLogger = new Mock<ILogger<PromotionEnrolledMemberDetailsQueryHandler>>();
            _reportingDBService = new Mock<IReportingDBServices>();
            _promotionEnrolledMemberService = new Mock<IPromotionEnrolledMemberService>();
            _handler = new PromotionEnrolledMemberDetailsQueryHandler(_reportingDBService.Object, _promotionEnrolledMemberService.Object, _mockLogger.Object);
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionEnrolledMemberDetailsQueryHandler(_reportingDBService.Object, _promotionEnrolledMemberService.Object, null));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenReportingDBServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionEnrolledMemberDetailsQueryHandler(null, _promotionEnrolledMemberService.Object, _mockLogger.Object));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenPromotionEnrolledMemberServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionEnrolledMemberDetailsQueryHandler(_reportingDBService.Object, null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var response = new PromotionEnrolledMemberDetailsResponse() { EnrolledMembersListFileURL = "dummyurl", Message = "Success", PromotionId = "XXXX" };
            var promoMemberDetails = new List<PromotionEnrolledMembers>() { new PromotionEnrolledMembers() { MemberNumber = "123", Name = "test" } };
            _reportingDBService.Setup(x => x.GetEnrolledMemberDetails("XXXX")).ReturnsAsync(promoMemberDetails);
            _promotionEnrolledMemberService.Setup(m => m.CreateXlStatementAndUploadToBlob(promoMemberDetails)).ReturnsAsync(new Response<string> { });
            PromotionEnrolledMemberDetailsQuery query = new() { PromotionId = "XXXX" };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldSuccessWhenNoenrolledMember()
        {
            var response = new PromotionEnrolledMemberDetailsResponse() { EnrolledMembersListFileURL = "dummyurl", Message = "Success", PromotionId = "XXXX" };
            var promoMemberDetails = new List<PromotionEnrolledMembers>() { };
            _reportingDBService.Setup(x => x.GetEnrolledMemberDetails("XXXX")).ReturnsAsync(promoMemberDetails);
            _promotionEnrolledMemberService.Setup(m => m.CreateXlStatementAndUploadToBlob(promoMemberDetails)).ReturnsAsync(new Response<string> { });
            PromotionEnrolledMemberDetailsQuery query = new() { PromotionId = "XXXX" };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal("No Associated Members with given Promotion Id", result.Value.Value.Message);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationfailure()
        {
            var response = new PromotionEnrolledMemberDetailsResponse() { EnrolledMembersListFileURL = "dummyurl", Message = "Success", PromotionId = "XXXX" };
            var promoMemberDetails = new List<PromotionEnrolledMembers>() { new PromotionEnrolledMembers() { MemberNumber = "123", Name = "test" } };
            _reportingDBService.Setup(x => x.GetEnrolledMemberDetails("XXXX")).ReturnsAsync(promoMemberDetails);
            _promotionEnrolledMemberService.Setup(m => m.CreateXlStatementAndUploadToBlob(promoMemberDetails)).ReturnsAsync(new Response<string> { });
            PromotionEnrolledMemberDetailsQuery query = new() { PromotionId = null };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
