using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop_Mapper
{
    public class OrderReshopRequestMapperTest
    {

        [Fact]
        public void GetParties_ShouldGetParties_When1DistributionChainRequest()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopRQ");

            var parties = Mapper.OrderReshopRequestMapper.GetParties(request);

            Assert.NotNull(parties);
            Assert.True(parties.Participants.Count() == 1);
            Assert.Equal("12345678", parties.Participants.First().AgencyID);
            Assert.Equal("ABCTravel", parties.Participants.First().Name);
            Assert.NotNull(parties.Participants.First().PartyType);
            Assert.Equal("1", parties.Participants.First().Ordinal);
        }


        [Fact]
        public void GetParties_ShouldGetParties_When2DistributionChainRequest()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopRQ _2parties");

            var parties = Mapper.OrderReshopRequestMapper.GetParties(request);

            Assert.NotNull(parties);
            Assert.True(parties.Participants.Count() == 2);
            Assert.Equal("12345678", parties.Participants.Last().AgencyID);
            Assert.Equal("ABCTravel", parties.Participants.Last().Name);
            Assert.Equal("91281035", parties.Participants.First().AgencyID);
            Assert.Equal("Skyscanner", parties.Participants.First().Name);

            Assert.NotNull(parties.Participants.First().PartyType);
            Assert.Equal("1", parties.Participants.First().Ordinal);
            Assert.NotNull(parties.Participants.Last().PartyType);
            Assert.Equal("1", parties.Participants.Last().Ordinal);
        }

        [Fact]
        public void MapReshopRqXmlToJson_ValidResponse()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopRQ");

            var orderReshopRQ = OrderReshopRequestMapper.MapOrderReshopRq(request);

            Assert.NotNull(orderReshopRQ);
            Assert.NotNull(orderReshopRQ?.BookingReferences);
            Assert.NotNull(orderReshopRQ?.OrderId);
            Assert.NotNull(orderReshopRQ?.OriginDestinationAssociations);
            Assert.NotNull(orderReshopRQ?.Parties);
            Assert.NotNull(orderReshopRQ.ReshopQueries);
            Assert.NotNull(orderReshopRQ.ReshopOrder);

        }

        [Fact]
        public void MapReshopRqXmlToJson_CorrectMappings()
        {
            var IATA_orderReshop = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopRQ");

            var orderReshopRQ = OrderReshopRequestMapper.MapOrderReshopRq(IATA_orderReshop);

            Assert.NotNull(orderReshopRQ);
            Assert.Equal("C66PHV", orderReshopRQ.BookingReferences.RLOC);
            Assert.Equal("VS932C66PHV23", orderReshopRQ.OrderId);
            Assert.Equal("LHR", orderReshopRQ.OriginDestinationAssociations.First().OriginDestination.OriginAirport);
            Assert.Equal("TLV", orderReshopRQ.OriginDestinationAssociations.First().OriginDestination.DestinationAirport);
            Assert.Equal("12345678", orderReshopRQ.Parties.Participants.First().AgencyID);
            Assert.Equal("4", orderReshopRQ.ReshopQueries.FlightSpecific.SearchCriteria.CabinTypes.First().CabinCode);
        }

    }
}
