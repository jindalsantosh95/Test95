using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionUpdateRequestValidatorTest
    {
        private readonly PromotionUpdateRequestValidator _promotionUpdateRequestValidator;
        public PromotionUpdateRequestValidatorTest()
        {
            _promotionUpdateRequestValidator = new PromotionUpdateRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidPromotionUpdateRequestData()
        {
            var request = TestDataProvider.GetTestData<PromotionUpdateRequestData>("CampaignManagementMS\\PromotionUpdateValid");
            Assert.True(_promotionUpdateRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidPromotionUpdateRequestData()
        {
            var request = TestDataProvider.GetTestData<PromotionUpdateRequestData>("CampaignManagementMS\\PromotionUpdateInvalid");
            Assert.False(_promotionUpdateRequestValidator.Validate(request).IsValid);
        }
    }
}
