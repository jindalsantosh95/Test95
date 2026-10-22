using Azure.Messaging.EventGrid.SystemEvents;
using System;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Application.Query.OfferPrice;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Mapper.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class OfferPriceRQMapperTests
    {
        [Fact]
        public void OfferPrice_RequestMapping_For_MultiplePax()
        {
            var offerPriceQueryRequest = TestDataProvider.GetTestData<OfferPriceQuery>("NDCOffer\\OfferPrice\\OfferPriceRequest(MultiPax)");
            var offerPriceRequest = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ(ReturnandMultiPax)");
            var pricedOffer = (SelectedOfferListType)offerPriceRequest.Request.PricedOffer.Item;
            var offerDocResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse(ReturnandMultiPax)");
            IEnumerable<UpsellOffersData> upsellOffersData = Enumerable.Empty<UpsellOffersData>();
            var genericDataMapper = new GenericDataMapper();

            var response = OfferPriceRQMapper.MapOfferPriceRequest(offerPriceQueryRequest, pricedOffer, true, genericDataMapper, offerDocResponse, upsellOffersData, "NDC11");

            Assert.NotNull(response);
            Assert.Equal(5, response.CustomerDetails.Count());
            Assert.Equal("ADT", response.CustomerDetails.First().Ptc);
            Assert.Equal("ADT", response.CustomerDetails.ElementAt(1).Ptc);
            Assert.Equal("GBE", response.CustomerDetails.ElementAt(2).Ptc);
            Assert.Equal("CNN", response.CustomerDetails.ElementAt(3).Ptc);
            Assert.Equal("INF", response.CustomerDetails.ElementAt(4).Ptc);

        }

        [Fact]
        public void OfferPrice_RequestMapping_For_POS()
        {
            var offerPriceQueryRequest = TestDataProvider.GetTestData<OfferPriceQuery>("NDCOffer\\OfferPrice\\OfferPriceRequest(MultiPax)");
            var offerPriceRequest = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ(ReturnandMultiPax)");
            var pricedOffer = (SelectedOfferListType)offerPriceRequest.Request.PricedOffer.Item;
            var offerDocResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse(ReturnandMultiPax)");
            IEnumerable<UpsellOffersData> upsellOffersData = Enumerable.Empty<UpsellOffersData>();
            var genericDataMapper = new GenericDataMapper();

            var response = OfferPriceRQMapper.MapOfferPriceRequest(offerPriceQueryRequest, pricedOffer, true, genericDataMapper, offerDocResponse, upsellOffersData, "NDC11");

            Assert.NotNull(response);
            Assert.Equal("LON", response.Pos.CityCode);
            Assert.Equal("GB", response.Pos.CountryCode);
            Assert.Equal("GBP", response.Pos.CurrencyCode);
        }

        [Fact]
        public void OfferPrice_RequestMapping_For_Parties()
        {
            var offerPriceQueryRequest = TestDataProvider.GetTestData<OfferPriceQuery>("NDCOffer\\OfferPrice\\OfferPriceRequest(MultiPax)");
            var offerPriceRequest = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ(ReturnandMultiPax)");
            var pricedOffer = (SelectedOfferListType)offerPriceRequest.Request.PricedOffer.Item;
            var offerDocResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse(ReturnandMultiPax)");
            IEnumerable<UpsellOffersData> upsellOffersData = Enumerable.Empty<UpsellOffersData>();
            var genericDataMapper = new GenericDataMapper();

            var response = OfferPriceRQMapper.MapOfferPriceRequest(offerPriceQueryRequest, pricedOffer, true, genericDataMapper, offerDocResponse, upsellOffersData, "NDC11");

            Assert.NotNull(response);
            Assert.Equal("91281035", response.Parties.Participants.First().AgencyID);
            Assert.Equal("Skyscanner", response.Parties.Participants.First().Name);
            Assert.Equal("1", response.Parties.Participants.First().Ordinal);
        }

        [Fact]
        public void GetParties_SingleDistributionChain_ReturnsCorrectParticipant()
        {
            // Arrange
            DistributionChainLinkType participant1 = new DistributionChainLinkType { OrgRole = DistributionChainRoleCodeType.Seller ,Ordinal = "1", ParticipatingOrg = new ParticipatingOrgType { OrgID = "91281035", Name = "Skyscanner" } };
            DistributionChainLinkType[] distributionChain = new DistributionChainLinkType[] { participant1 };
            // Act
            var result = OfferPriceRQMapper.GetParties(distributionChain, null, true, "NDC11");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Participants);
            var participant = result.Participants.First();
            Assert.Equal("91281035", participant.AgencyID);
            Assert.Equal("Skyscanner", participant.Name);
            Assert.Equal(Participant.PartyTypeEnum.Seller, participant.PartyType);
            Assert.Equal("1", participant.Ordinal);
        }

        [Fact]
        public void GetParties_MultipleDistributionChains_ReturnsCorrectParticipants()
        {
            // Arrange
            DistributionChainLinkType participant1 = new DistributionChainLinkType { OrgRole = DistributionChainRoleCodeType.Seller,Ordinal = "1", ParticipatingOrg = new ParticipatingOrgType { OrgID = "91281035", Name = "Skyscanner" } };
            DistributionChainLinkType participant2 = new DistributionChainLinkType { OrgRole = DistributionChainRoleCodeType.Distributor,Ordinal = "1", ParticipatingOrg = new ParticipatingOrgType { OrgID = "35200056", Name = "OASIS TRAVEL" } };
            DistributionChainLinkType[] distributionChain = new DistributionChainLinkType[] { participant1, participant2 };

            // Act
            var result = OfferPriceRQMapper.GetParties(distributionChain, null, true, "NDC11");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Participants.Count());

            var firstParticipant = result.Participants.First();
            Assert.Equal("35200056", firstParticipant.AgencyID);
            Assert.Equal("OASIS TRAVEL", firstParticipant.Name);
            Assert.Equal(Participant.PartyTypeEnum.Distributor, firstParticipant.PartyType);
            Assert.Equal("1", firstParticipant.Ordinal);

            var secondParticipant = result.Participants.Last();
            Assert.Equal("91281035", secondParticipant.AgencyID);
            Assert.Equal("Skyscanner", secondParticipant.Name);
            Assert.Equal(Participant.PartyTypeEnum.Seller, secondParticipant.PartyType);
            Assert.Equal("1", secondParticipant.Ordinal);

        }

        [Fact]
        public void GetParties_EmptyDistributionChains_ReturnsNull()
        {
            DistributionChainLinkType[] distributionChain = System.Array.Empty<DistributionChainLinkType>();


            var parties = OfferPriceRQMapper.GetParties(distributionChain, null, true, "NDC11");

            Assert.Null(parties);
        }

        [Fact]
        public void OfferPrice_RequestMapping_For_OfferId()
        {
            var offerPriceQueryRequest = TestDataProvider.GetTestData<OfferPriceQuery>("NDCOffer\\OfferPrice\\OfferPriceRequest(MultiPax)");
            var offerPriceRequest = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ(ReturnandMultiPax)");
            var pricedOffer = (SelectedOfferListType)offerPriceRequest.Request.PricedOffer.Item;
            var offerDocResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse(ReturnandMultiPax)");
            IEnumerable<UpsellOffersData> upsellOffersData = Enumerable.Empty<UpsellOffersData>();
            var genericDataMapper = new GenericDataMapper();

            var response = OfferPriceRQMapper.MapOfferPriceRequest(offerPriceQueryRequest, pricedOffer, true, genericDataMapper, offerDocResponse, upsellOffersData, "NDC11");

            Assert.NotNull(response);
            Assert.Equal(4, response.PricedOffer.SelectedOffer.OfferItems.Count());
            Assert.Equal(2, response.PricedOffer.SelectedOffer.OfferItems.First().ServiceBeneficiaries.Count());
            Assert.Equal("ADULT_1", response.PricedOffer.SelectedOffer.OfferItems.First().ServiceBeneficiaries.ElementAt(0));
            Assert.Equal("ADULT_2", response.PricedOffer.SelectedOffer.OfferItems.First().ServiceBeneficiaries.ElementAt(1));
        }
    }
}
