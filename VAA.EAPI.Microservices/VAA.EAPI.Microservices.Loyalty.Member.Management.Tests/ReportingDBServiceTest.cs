using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class ReportingDBServiceTest
    {
        private readonly Mock<ReportingDataContext> _reportingDataContext;
        private readonly Mock<ILogger<ReportingDBServices>> _mocklogger;
        private readonly ReportingDBServices _reportingDBServices;
        public ReportingDBServiceTest()
        {
            _reportingDataContext = new Mock<ReportingDataContext>();
            _mocklogger = new Mock<ILogger<ReportingDBServices>>();
            _reportingDBServices = new ReportingDBServices(_reportingDataContext.Object, _mocklogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenDataContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ReportingDBServices(null, _mocklogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ReportingDBServices(_reportingDataContext.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetMembersBirthdayEmailDetails()
        {
            var memberDetails = TestDataProvider.GetTestData<List<MemberContactData>>("MemberManagement\\MemberContactData");
            var data = memberDetails.AsQueryable();

            var mockSet = new Mock<DbSet<MemberContactData>>();
            mockSet.As<IQueryable<MemberContactData>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<MemberContactData>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<MemberContactData>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<MemberContactData>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            _reportingDataContext.Setup(m => m.MemberEmailDetails).Returns(mockSet.Object);
            await Assert.ThrowsAsync<Exception>(() => _reportingDBServices.GetMembersBirthdayEmailDetails());
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetMembersDetailsForBirthdayBonus()
        {
            var memberDetails = TestDataProvider.GetTestData<List<BirthdayBonusMemberDetails>>("MemberManagement\\MemberContactData");
            var data = memberDetails.AsQueryable();

            var mockSet = new Mock<DbSet<BirthdayBonusMemberDetails>>();
            mockSet.As<IQueryable<BirthdayBonusMemberDetails>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<BirthdayBonusMemberDetails>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<BirthdayBonusMemberDetails>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<BirthdayBonusMemberDetails>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            _reportingDataContext.Setup(m => m.BirthdayBonusMemberDetails).Returns(mockSet.Object);
            await Assert.ThrowsAsync<Exception>(() => _reportingDBServices.GetMembersDetailsForBirthdayBonus(It.IsAny<DateTime>()));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetEnrolledMemberDetails()
        {
            var promoDetails = new List<PromotionEnrolledMembers>() { new PromotionEnrolledMembers() { MemberNumber = "123", Name = "test" } };
            var data = promoDetails.AsQueryable();

            var mockSet = new Mock<DbSet<PromotionEnrolledMembers>>();
            mockSet.As<IQueryable<PromotionEnrolledMembers>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PromotionEnrolledMembers>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PromotionEnrolledMembers>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PromotionEnrolledMembers>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            _reportingDataContext.Setup(m => m.EnrolledMemberDetails).Returns(mockSet.Object);
            await Assert.ThrowsAsync<Exception>(() => _reportingDBServices.GetEnrolledMemberDetails("XXXX"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task DeleteTargetedMembership()
        {
            await Assert.ThrowsAsync<Exception>(() => _reportingDBServices.DeleteTargetedMembership("XXXX", "Test"));
        }
    }
}
