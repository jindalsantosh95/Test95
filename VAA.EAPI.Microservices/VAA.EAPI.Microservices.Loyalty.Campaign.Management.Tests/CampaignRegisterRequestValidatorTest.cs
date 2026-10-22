using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRegisterRequestValidatorTest
    {
        private readonly CampaignRegisterRequestValidator _campainRegisterRequestValidator;
        public CampaignRegisterRequestValidatorTest()
        {
            _campainRegisterRequestValidator = new CampaignRegisterRequestValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidCampaignRegisterequestData()
        {
            var request = TestDataProvider.GetTestData<CamapignRegisterRequestData>("CampaignManagementMS\\CampaignRegisterValid");
            Assert.True(_campainRegisterRequestValidator.Validate(request).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidCampaignRegisterRequestData()
        {
            var request = TestDataProvider.GetTestData<CamapignRegisterRequestData>("CampaignManagementMS\\CampaignRegisterInvalid");
            Assert.False(_campainRegisterRequestValidator.Validate(request).IsValid);
        }
    }
}
