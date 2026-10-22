using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidateOfferFromDbTests
    {
        [Fact]
        public void ValidateOfferFromDb_Error_ForNullOffer_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var airlineOffers = new OfferResponse() { Offers = null };
            
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            
            var validationErrors = ValidateOfferPrice.ValidateOfferFromDb(airlineOffers, pricedOffer.SelectedOffer.First().OfferRefID);

            Assert.True(validationErrors.Any());
            Assert.Equal("OFFERPRICE_OfferRefID_InValid", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateOfferFromDb_Error_ForNull_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
           
            OfferResponse airlineOffers = null;
            
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            
            var validationErrors = ValidateOfferPrice.ValidateOfferFromDb(airlineOffers, pricedOffer.SelectedOffer.First().OfferRefID);

            Assert.True(validationErrors.Any());
            Assert.Equal("OFFERPRICE_OfferRefID_InValid", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateOfferFromDb_Error_ForOfferCountZero_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var airlineOffers = new OfferResponse() { Offers = new List<OfferDetails> { new() } };
            
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            
            var validationErrors = ValidateOfferPrice.ValidateOfferFromDb(airlineOffers, pricedOffer.SelectedOffer.First().OfferRefID);

            Assert.True(validationErrors.Any());
            Assert.Equal("OFFERPRICE_OfferRefID_InValid", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateOfferFromDb_Error_ForOfferNotMatch_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            OfferDetails offers = new() { OfferId = "abc" };
            var airlineOffers = new OfferResponse() { Offers = new List<OfferDetails> { offers } };
           
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            
            var validationErrors = ValidateOfferPrice.ValidateOfferFromDb(airlineOffers, pricedOffer.SelectedOffer.First().OfferRefID);

            Assert.True(validationErrors.Any());
            Assert.Equal("OFFERPRICE_OfferRefID_InValid", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateOfferFromDb_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");

            OfferDetails offers = new() { OfferId = "78e3f8d1-faad-4f04-b0ca-89b14d0f0560|0o6TkD1Dht7RU7oXMdZbTJ001" };
            var airlineOffers = new OfferResponse() { Offers = new List<OfferDetails> { offers } };
           
            var pricedOffer = (SelectedOfferListType)request.Request.PricedOffer.Item;
            
            var validationErrors = ValidateOfferPrice.ValidateOfferFromDb(airlineOffers, pricedOffer.SelectedOffer.First().OfferRefID);

            Assert.True(validationErrors.Count == 0);

        }
    }
}
