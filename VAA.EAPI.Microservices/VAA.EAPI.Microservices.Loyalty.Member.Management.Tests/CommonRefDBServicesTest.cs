using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CommonRefDBServicesTest
    {
        private readonly Mock<CommonReferenceDataContext> _commonReferenceDataContext;
        private readonly CommonRefDBServices _commonRefDBService;
        private readonly Mock<ILogger<CommonRefDBServices>> _mocklogger;

        public CommonRefDBServicesTest()
        {
            _commonReferenceDataContext = new Mock<CommonReferenceDataContext>();
            _mocklogger = new Mock<ILogger<CommonRefDBServices>>();
            _commonRefDBService = new CommonRefDBServices(_commonReferenceDataContext.Object, _mocklogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBServices(null, _mocklogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBServices(_commonReferenceDataContext.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetMemberAccountReferenceDataWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetMemberAccountReferenceData());
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetPartnerDataWithError()
        {
            var partnersResponse = TestDataProvider.GetTestData<Infrastructure.Models.PartnerProfile>("MemberManagement\\PartnerProfileData");
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetPartnersReferenceData());
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetProductDataWithError()
        {
            string ProductName = "Birthday Bonus";
            var partnersResponse = TestDataProvider.GetTestData<Infrastructure.Models.Product>("MemberManagement\\ProductData");
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetProductReferenceDataByName(ProductName));
        }

    }
}
