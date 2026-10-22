using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUpdateRequestValidatorTest
    {
        private readonly CampaignUpdateRequestValidator _campainUpdateRequestValidator;
        public CampaignUpdateRequestValidatorTest()
        {
            _campainUpdateRequestValidator = new CampaignUpdateRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidCampaignUpdateRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignUpdateRequestData>("CampaignManagementMS\\CampaignUpdateValid");
            Assert.True(_campainUpdateRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidCampaignUpdateRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignUpdateRequestData>("CampaignManagementMS\\CampaignUpdateInvalid");
            Assert.False(_campainUpdateRequestValidator.Validate(request).IsValid);
        }
    }
}
