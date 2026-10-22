using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionDelinkRequestValidatorTest
    {
        private readonly CampaignPromotionDelinkRequestValidator _campaignPromotionDelinkRequestValidator;
        public CampaignPromotionDelinkRequestValidatorTest()
        {
            _campaignPromotionDelinkRequestValidator = new CampaignPromotionDelinkRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidCampaignPromotionDelinkRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignPromotionDelinkRequestData>("CampaignManagementMS\\CampaignPromotionLinkingValid");
            Assert.True(_campaignPromotionDelinkRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidCampaignPromotionDelinkRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignPromotionDelinkRequestData>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            Assert.False(_campaignPromotionDelinkRequestValidator.Validate(request).IsValid);
        }
    }
}