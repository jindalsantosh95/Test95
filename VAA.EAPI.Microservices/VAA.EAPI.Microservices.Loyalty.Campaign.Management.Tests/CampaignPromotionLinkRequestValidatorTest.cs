using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignPromotionLinkRequestValidatorTest
    {
        private readonly CampaignPromotionLinkRequestValidator _campaignPromotionLinkRequestValidator;
        public CampaignPromotionLinkRequestValidatorTest()
        {
            _campaignPromotionLinkRequestValidator = new CampaignPromotionLinkRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidCampaignPromotionLinkRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignPromotionLinkRequestData>("CampaignManagementMS\\CampaignPromotionLinkingValid");
            Assert.True(_campaignPromotionLinkRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidCampaignPromotionLinkRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignPromotionLinkRequestData>("CampaignManagementMS\\CampaignPromotionLinkingInvalid");
            Assert.False(_campaignPromotionLinkRequestValidator.Validate(request).IsValid);
        }
    }
}