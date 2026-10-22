using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAssignRequestValidatorTest
    {
        private readonly CampaignAssignRequestValidator _campaignAssignRequestValidator;
        public CampaignAssignRequestValidatorTest()
        {
            _campaignAssignRequestValidator = new CampaignAssignRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidCampaignAssignrequestData()
        {
            var request = TestDataProvider.GetTestData<CamapignAssignRequestData>("CampaignManagementMS\\CampaignAssignValidRequest");
            Assert.True(_campaignAssignRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidCampaignRegisterRequestData()
        {
            var request = TestDataProvider.GetTestData<CamapignAssignRequestData>("CampaignManagementMS\\CampaignAssignInvalidRequest");
            Assert.False(_campaignAssignRequestValidator.Validate(request).IsValid);
        }
    }
}
