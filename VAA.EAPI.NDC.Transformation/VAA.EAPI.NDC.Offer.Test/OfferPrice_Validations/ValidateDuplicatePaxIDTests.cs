using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidateDuplicatePaxIDTests
    {
        [Fact]
        public void ValidateDuplicatePaxID_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var validationErrors = ValidateOfferPrice.ValidateDuplicatePaxID(request);

            Assert.True(validationErrors.Count == 0);

        }

        [Fact]
        public void ValidateDuplicatePaxID_Error_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            foreach (var x in request.Request.DataLists.PaxList)
            {
                if (x.PaxID.Contains("ADULT_3"))
                {
                    x.PaxID = "ADULT_1";
                }
            }
            var validationErrors = ValidateOfferPrice.ValidateDuplicatePaxID(request);

            Assert.True(validationErrors.Any());
            Assert.Equal("143_3", validationErrors.First().ErrorCode);

        }
    }
}
