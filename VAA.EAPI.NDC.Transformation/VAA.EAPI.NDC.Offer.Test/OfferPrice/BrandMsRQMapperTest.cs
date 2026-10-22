using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.BrandMs;
using VAA.EAPI.NDC.Offer.Constants;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class BrandMsRQMapperTest
    {

        [Fact]
        public void BrandMsRQMapper_ShouldReturnSuccess()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");
           
            var offer = offerDoc.Offers.ToList().FirstOrDefault();
            var result = BrandMsRqMapper.MapBrandMsRequest(offer, offerDoc);

            Assert.NotNull(result);
            Assert.Equal(1, result.Itinerary.Count());
            Assert.Equal(3, result.Itinerary.First().Segments.Count());
            Assert.Equal("LHR", result.Itinerary.First().Segments.First().Origin.ToString());
            Assert.Equal("7048", result.Itinerary.First().Segments.First().FlightNumber.ToString());
            Assert.Equal("AMS", result.Itinerary.First().Segments.First().Destination.ToString());
            Assert.Equal("NDC99", result.AccountCode);
            Assert.Equal("1P", result.Itinerary.FirstOrDefault()?.Source);
        }

        [Fact]
        public void BrandMsRQMapper_IncludesTicketDesignator_ForPrivateFare()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");
            var offer = offerDoc.Offers.First();
            foreach (var priceClass in offerDoc.DataList.PriceClass)
            {
                priceClass.TicketDesigCode = "UWF1";
            }

            var result = BrandMsRqMapper.MapBrandMsRequest(offer, offerDoc);

            Assert.Equal("UWF1", result.Itinerary.First().Segments.First().TicketDesignator);
        }

        [Fact]
        public void BrandMsRQMapper_MapsNewFareFareBasisAndTicketDesignator()
        {
            var newFareResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");
            newFareResponse.FareTypeRequest.FareTypeCode = Constants.Application.FareTypeCode;
            var newFareOffer = newFareResponse.Offers.First();
            var newFareService = newFareOffer.OfferItems.First().Services.First();
            var priceClass = newFareResponse.DataList.PriceClass.First(priceClass => priceClass.Id == newFareService.PriceClassRefIds.First());
            priceClass.FareBasisCode = "NGCEB8WA";
            priceClass.TicketDesigCode = "UWF1";

            var result = BrandMsRqMapper.MapBrandMsRequest(newFareOffer, newFareResponse);
            var segment = result.Itinerary.First().Segments.First();

            Assert.Equal("NGCEB8WA", segment.FareBasis);
            Assert.Equal("UWF1", segment.TicketDesignator);
        }

        [Fact]
        public void BrandMsRQMapper_DoesNotIncludeTicketDesignator_ForCorporateFare()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");
            var offer = offerDoc.Offers.First();
            offerDoc.FareTypeRequest.FareTypeCode = Constants.Application.CorporateFareTypeCode;
            foreach (var priceClass in offerDoc.DataList.PriceClass)
            {
                priceClass.TicketDesigCode = "UWF1";
            }

            var result = BrandMsRqMapper.MapBrandMsRequest(offer, offerDoc);

            Assert.Null(result.Itinerary.First().Segments.First().TicketDesignator);
        }

        [Theory]
        [InlineData("758", true)]
        [InlineData("70J", true)]
        [InlineData("70G", false)]
        public void BrandMsRQMapper_MapsTicketDesignator_ForInfantPtc(string fareTypeCode, bool shouldIncludeTicketDesignator)
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");
            offerDoc.FareTypeRequest.FareTypeCode = fareTypeCode;
            offerDoc.DataList.CustomerDetails.First().Ptc = Constants.Application.InfantPTCCode;
            foreach (var priceClass in offerDoc.DataList.PriceClass)
            {
                priceClass.TicketDesigCode = "UWF1";
            }

            var result = BrandMsRqMapper.MapBrandMsPTCRequest(offerDoc.Offers.First(), offerDoc);

            Assert.Equal(shouldIncludeTicketDesignator ? "UWF1" : null, result.Itinerary.First().Segments.First().TicketDesignator);
        }
    }
}
