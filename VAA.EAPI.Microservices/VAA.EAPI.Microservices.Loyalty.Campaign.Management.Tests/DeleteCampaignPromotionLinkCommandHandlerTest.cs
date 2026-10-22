using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class DeleteCampaignPromotionLinkCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly DeleteCampaignPromotionLinkCommandHandler _handler;
        public DeleteCampaignPromotionLinkCommandHandlerTest()
        {
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new DeleteCampaignPromotionLinkCommandHandler(_mockCommonRefDBRepository.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new DeleteCampaignPromotionLinkCommandHandler(null));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            DeleteCampaignPromotionLinkCommand requestData = TestDataProvider.GetTestData<DeleteCampaignPromotionLinkCommand>("CampaignManagementMS\\CampaignPromotionDelinkingValid");
            _mockCommonRefDBRepository.Setup(x => x.DeleteCampaignPromtionLink(requestData.CampaignId, requestData.PromotionId)).ReturnsAsync("Success");
            var campaignPromotionDelinkResponse = _handler.Handle(requestData, System.Threading.CancellationToken.None);
            Assert.True(campaignPromotionDelinkResponse.Result.IsSuccess);
        }
    }
}
