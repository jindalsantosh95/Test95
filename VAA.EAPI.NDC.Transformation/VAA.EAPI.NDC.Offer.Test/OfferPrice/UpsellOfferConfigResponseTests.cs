using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class UpsellOfferConfigResponseTests
    {
        [Fact]
        public void ValidateUpsellOfferConfigResponse_NoError()
        {
            List<UpsellOffersData> upsellOffersData = new List<UpsellOffersData>();

            var validationErrors = ValidateOfferPrice.ValidateUpsellConfigResponse(upsellOffersData);
            
            Assert.True(validationErrors.Count == 0);
        }
    }
}
