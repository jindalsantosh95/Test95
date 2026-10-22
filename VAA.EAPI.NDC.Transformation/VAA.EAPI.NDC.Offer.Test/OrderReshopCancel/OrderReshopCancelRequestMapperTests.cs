using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshopCancel
{
    public class OrderReshopCancelRequestMapperTests
    {
        [Fact]
        public void GetParties_ShouldGetParties_ValidResponse()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");

            var parties = Mapper.OrderReshopCancelRequestMapper.GetParties(request);

            Assert.NotNull(parties);
            Assert.True(parties.Participants.Count() == 1);
            Assert.Equal("91281035", parties.Participants.First().AgencyID);
            Assert.Equal("Skyscanner", parties.Participants.First().Name);
            Assert.NotNull(parties.Participants.First().PartyType);
            Assert.Equal("1", parties.Participants.First().Ordinal);
        }

        [Fact]
        public void MapReshopRqXmlToJson_ValidResponse()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderCancel\\OrderCancelRQ");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");
            var orderReshopRQ = OrderReshopCancelRequestMapper.MapCancelReshopRq(request);

            Assert.NotNull(orderReshopRQ);
            Assert.NotNull(orderReshopRQ?.BookingReferences);
            Assert.NotNull(orderReshopRQ?.OrderId);
            Assert.NotNull(orderReshopRQ?.Parties);

        }

        [Fact]
        public void MapOrderReshopRq_ValidResponse()
        {
            var rq = TestDataProvider.GetTestData<OrderReshopRequest>("NDCOffer\\OrderCancel\\OrderReshopCancel");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");

            var orderReshopRQ = OrderReshopCancelRequestMapper.MapOrderReshopRq(orderDoc);

            Assert.NotNull(orderReshopRQ);
			Assert.Equal("ADULT_1", orderReshopRQ.CustomerDetails.First().CustId);
            Assert.Equal("9999327707387", orderReshopRQ.ReshopOrder.First().AccountableDocumentRefs.First().CustomerRefId);
            Assert.Equal("NG05E0LA", orderReshopRQ.ReshopOrder.First().AccountableDocumentRefs.First().Coupons.First().FareBasis);
            Assert.Equal("SEG1", orderReshopRQ.ReshopOrder.First().AccountableDocumentRefs.First().Coupons.First().SegmentRefId);
            Assert.Equal("609.89", orderReshopRQ.ReshopOrder.First().PriceDetails.First().Amount.MonetaryAmount);
            Assert.Equal("GBP", orderReshopRQ.ReshopOrder.First().PriceDetails.First().Amount.BaseCurrency);
            Assert.Equal("TLV", orderReshopRQ.SegmentDetails.First().DestinationAirportCode);
            Assert.Equal("OD1", orderReshopRQ.SegmentDetails.First().OriginDestinationRefId);
            Assert.Equal("453", orderReshopRQ.SegmentDetails.First().MarketingFlightNumber);

        }
    }
}
