using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class CreateTechnicalErrorTests
    {
        [Fact]
        public void Validate_TechnicalError_Response()
        {
            var validationErrors = ValidateOfferPrice.CreateTechnicalErrorResponse();

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("Service_TechError", validationErrors.First().ErrorCode);
        }
    }
}
