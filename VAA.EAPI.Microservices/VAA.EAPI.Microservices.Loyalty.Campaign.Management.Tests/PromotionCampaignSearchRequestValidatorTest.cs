using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCampaignSearchRequestValidatorTest
    {
        private readonly PromotionCampaignSearchRequestValidator _promoCampSearchRequestValidator;
        public PromotionCampaignSearchRequestValidatorTest()
        {
            _promoCampSearchRequestValidator = new PromotionCampaignSearchRequestValidator();
        }

        [Fact]
        public void ValidateReturnsfalseGivenInvalidPromotionsearchRequestData()
        {
            var request = TestDataProvider.GetTestData<PromotionCampaignSearchRequestData>("CampaignManagementMS\\PromotionSearchRequest");
            Assert.True(_promoCampSearchRequestValidator.Validate(request).IsValid);
        }
    }
}
