using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class PromotionEnrolledMemberServiceTest
    {
        private readonly Mock<ILogger<PromotionEnrolledMemberService>> _mockLogger;
        private readonly PromotionEnrolledMemberService _promotionEnrolledMemberService;

        public PromotionEnrolledMemberServiceTest()
        {
            _mockLogger = new Mock<ILogger<PromotionEnrolledMemberService>>();
            _promotionEnrolledMemberService = new PromotionEnrolledMemberService(_mockLogger.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionEnrolledMemberService(null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task CreateXlStatementAndUploadToBlobShouldTrowExceptionc()
        {
            var memberList = new List<PromotionEnrolledMembers>() { new PromotionEnrolledMembers() { MemberNumber = "123", Name = "test" } };
            await Assert.ThrowsAsync<Exception>(() => _promotionEnrolledMemberService.CreateXlStatementAndUploadToBlob(memberList));
        }
    }
}
