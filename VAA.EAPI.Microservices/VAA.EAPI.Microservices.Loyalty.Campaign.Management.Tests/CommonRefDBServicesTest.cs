using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CommonRefDBServicesTest
    {
        private readonly Mock<CommonReferenceDataContext> _commonReferenceDataContext;
        private readonly Mock<ILogger<CommonRefDBServices>> _mocklogger;
        private readonly CommonRefDBServices _commonRefDBService;

        public CommonRefDBServicesTest()
        {
            _commonReferenceDataContext = new Mock<CommonReferenceDataContext>();
            _mocklogger = new Mock<ILogger<CommonRefDBServices>>();
            _commonRefDBService = new CommonRefDBServices(_commonReferenceDataContext.Object, _mocklogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenDataContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBServices(null, _mocklogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBServices(_commonReferenceDataContext.Object, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetCampaignSearchList()
        {
            var campDetails = TestDataProvider.GetTestData<List<CampaignSearchData>>("CampaignManagementMS\\CampaignSearchResponseDB");
            var data = campDetails.AsQueryable();

            var mockSet = new Mock<DbSet<CampaignSearchData>>();
            mockSet.As<IQueryable<CampaignSearchData>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CampaignSearchData>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CampaignSearchData>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CampaignSearchData>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _commonReferenceDataContext.Setup(m => m.CampaignSearchData).Returns(mockSet.Object);

            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetCampaignSearchList(null, "VIRGIN007", null, null, null, null, null, null, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetAllCampaignDetailsWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetAllCampaignDetails("1-A976-1"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetPromotionsDetailWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetPromotionsDetail("1-A976-1"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task UpdateCampaignDetailsWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.UpdateCampaignDetails("1-A976-1", "WELS44Test", "07/03/2022", "07/03/2023", "Unit Test", "Test_Desc_one", "Link", true, "Active", "Straight to Gold", "testuser"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task CreateNewCampaignWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.CreateNewCampaign("WELS44Test", "07/03/2022", "07/03/2023", "Unit Test", "Test_Desc_one", "Link", true, "Active", "Straight to Gold", "testuser"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task InsertCampaignPromtionLinkWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.InsertCampaignPromtionLink("1-A976-1", "1-2OHIEB", "Test.mail@fly.virgin.com"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetCampaignTypeListWithError()
        {
            var listofValues = new List<ListOfValues>() { new ListOfValues() { Type = "CAMPAIGN_TYPE", SubType = "Standalone Campaign", Active = "Y", Value = "XYZ" } };
            var data = listofValues.AsQueryable();

            var mockSet = new Mock<DbSet<ListOfValues>>();
            mockSet.As<IQueryable<ListOfValues>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<ListOfValues>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<ListOfValues>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<ListOfValues>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _commonReferenceDataContext.Setup(m => m.ListOfValues).Returns(mockSet.Object);

            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetCampaignTypeList());
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task UpdatePromotionDetailsWithError()
        {
            PromotionUpdateCommand req = TestDataProvider.GetTestData<PromotionUpdateCommand>("CampaignManagementMS\\PromotionUpdateValid");

            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.UpdatePromotionDetails(req.PromotionId, req.PromotionName, req.PartnerName, req.StartDate, req.EndDate, req.Active, req.CampaignLinked, req.InitiatedBy));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetPromotionSearchListWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetPromotionSearchList(null, "XYZ", null, null, null, null, null, null));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task DeleteCampaignPromtionLinkWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.DeleteCampaignPromtionLink("1-ABC", "1-XYZ"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task CreateNewPromotionWithError()
        {
            var req = TestDataProvider.GetTestData<PromotionCreateRequestCommand>("CampaignManagementMS\\PromotionCreateValid");

            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.CreateNewPromotion(req.PromotionName, req.PartnerName, req.StartDate, req.EndDate, req.Active, req.InitiatedBy));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetPromotionCampaignSearchListWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetPromotionCampaignSearchList("1-XYZ"));
        }

        [Fact(Skip = "Test has not been setup correctly")]
        public async Task GetPromotionslistWithError()
        {
            await Assert.ThrowsAsync<Exception>(() => _commonRefDBService.GetPromotionslist());
        }
    }
}
