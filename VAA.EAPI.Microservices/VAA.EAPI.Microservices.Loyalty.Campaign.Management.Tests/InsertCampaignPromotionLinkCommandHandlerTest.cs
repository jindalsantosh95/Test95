using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class InsertCampaignPromotionLinkCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly InsertCampaignPromotionLinkCommandHandler _handler;
        public InsertCampaignPromotionLinkCommandHandlerTest()
        {
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new InsertCampaignPromotionLinkCommandHandler(_mockCommonRefDBRepository.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new InsertCampaignPromotionLinkCommandHandler(null));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            InsertCampaignPromotionLinkCommand requestData = TestDataProvider.GetTestData<InsertCampaignPromotionLinkCommand>("CampaignManagementMS\\CampaignPromotionLinkingValid");
            _mockCommonRefDBRepository.Setup(x => x.InsertCampaignPromtionLink("1-A976-1", "1-2OHIEB", "Test.mail@fly.virgin.com")).ReturnsAsync("Success");
            var campaignPromotionLinkResponse = _handler.Handle(requestData, System.Threading.CancellationToken.None);
            Assert.True(campaignPromotionLinkResponse.Result.IsSuccess);
        }
    }
}
