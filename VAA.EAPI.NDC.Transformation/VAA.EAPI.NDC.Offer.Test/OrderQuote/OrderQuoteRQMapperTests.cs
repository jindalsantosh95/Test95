using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Mapper.OrderQuote;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderQuote
{
    public class OrderQuoteRQMapperTests
    {
        [Theory]
        [InlineData("Economy Delight Light", "ECONOMY-DELIGHT-LIGHT")]
        [InlineData("Premium Light", "COMFORT-PLUS-PREMIUM-LIGHT")]
        [InlineData("Upper Class Light", "BUS-FIRST-LIGHT")]
        public void GetITABrandName_ForLightFareBrands_ShouldReturnExpectedItaBrand(string vaaBrand, string expectedItaBrand)
        {
            var genericDataMapper = new GenericDataMapper();

            var result = genericDataMapper.GetITABrandName(vaaBrand);

            Assert.Equal(expectedItaBrand, result);
        }

        [Fact]
        public void GetParties_ShouldReturnSuccess()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderQuoteRQ>("NDCOffer\\OrderQuote\\OrderQuoteRQ");

            var result = OrderQuoteRQMapper.GetParties(request);

            Assert.True(result.Participants.Any());
            Assert.Equal("Org", result.Participants.First().AgencyID);
            Assert.Equal("Skyscanner", result.Participants.First().Name);
            Assert.Equal("Seller", result.Participants.First().PartyType.ToString());
        }

        [Fact]
        public void MapOrderQuoteRequestToVAJson_ShouldReturnSuccess()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderQuoteRQ>("NDCOffer\\OrderQuote\\OrderQuoteRQ");

            var result = OrderQuoteRQMapper.MapOrderQuoteRequestToVAJson(request, false,null);

            Assert.NotNull(result);
            Assert.True(result.PricedOffer.SelectedOffer.OfferItems.Count() == 1);
            Assert.True(result.PricedOffer.SelectedOffer.OfferRefID == "GpDPenoBW2BMSEnvZ0zzTL00G|b641faaf616c48f48f7ed8b8e25164b1");
            Assert.Equal("ADULT_1", result.CustomerDetails.First().CustId);
            Assert.Equal("ADT", result.CustomerDetails.First().Ptc);
        }

        [Fact]
        public void OrderQuoteRepriceQuery_ShouldReturnSuccess()
        {
            var OrderQuoteRQ = TestDataProvider.GetTestData<OrderQuoteQuery>("NDCOffer\\OrderQuote\\OrderQuoteNewRQ");
            var OrderRepriceValidRequest = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidateRequest");
            var OfferResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferRepriceResponse");
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderQuote\\OrderDocument");
            var genericDataMapper = new GenericDataMapper();
            string accountCode = "NDC02";

            IEnumerable<UpsellOffersData> upsellOffersData = Enumerable.Empty<UpsellOffersData>();

            var result = OrderQuoteRQMapper.MapOrderQuoteRq(OrderQuoteRQ, OrderRepriceValidRequest, OfferResponse, upsellOffersData, true, genericDataMapper, accountCode, null, false, orderDocument);

            Assert.NotNull(result);
            Assert.False(result.IsReshopDisruption);
            Assert.Equal("91281035", result.Parties.Participants.First().AgencyID);
            Assert.Equal("ADT", result.CustomerDetails.First().Ptc);
            Assert.True(result.CustomerDetails.Count() > 0);
            Assert.Equal("NDC02", result.Parties.Participants.First().AccountCode);
        }
    }
}
