using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;

using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUnassignRequestValidatorTest
    {
        private readonly CampaignUnassignRequestValidator _campaignUnassignRequestValidator;
        public CampaignUnassignRequestValidatorTest()
        {
            _campaignUnassignRequestValidator = new CampaignUnassignRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidCampaignUnassignquestData()
        {
            var request = TestDataProvider.GetTestData<CampaignUnassignRequestData>("CampaignManagementMS\\CampaignUnassignValid");
            Assert.True(_campaignUnassignRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidCampaignUnassignRequestData()
        {
            var request = TestDataProvider.GetTestData<CampaignUnassignRequestData>("CampaignManagementMS\\CampaignUnassignInValid");
            Assert.False(_campaignUnassignRequestValidator.Validate(request).IsValid);
        }
    }
}

