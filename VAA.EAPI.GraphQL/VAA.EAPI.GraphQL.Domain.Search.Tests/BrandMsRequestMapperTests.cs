using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    public class BrandMsRequestMapperTests
    {
        [Fact]
        public void BrandMsReqMapper_UsesAppliedPromoCodeAsAccountCode()
        {
            var offerResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var cabinInfo = TestDataProvider.GetTestData<IEnumerable<CabinInfo>>("CabinInfo");
            var offers = offerResponse.Offers.ToList();
            offers.First().Selected = true;
            offers.First().PromoCodeApplied = "uwf01";
            offers.Add(new OfferDetails { Selected = false, UpsellOffer = true, PromoCodeApplied = "upsell" });
            offerResponse.Offers = offers;

            var result = BrandMsRequestMapper.BrandMsReqMapper(
                offerResponse,
                cabinInfo,
                new[] { "ADT" },
                BrandMsRequestMapper.GetAppliedPromoCode(offerResponse));

            Assert.Equal("UWF01", result.AccountCode);
        }

        [Fact]
        public void GetAppliedPromoCode_DoesNotUsePromoCodeFromUpsellWhenSelectedOfferHasNoPromoCode()
        {
            var offerResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var offers = offerResponse.Offers.ToList();
            offers.First().Selected = true;
            offers.First().PromoCodeApplied = null;
            offers.Add(new OfferDetails { Selected = false, UpsellOffer = true, PromoCodeApplied = "upsell" });
            offerResponse.Offers = offers;

            var promoCode = BrandMsRequestMapper.GetAppliedPromoCode(offerResponse);

            Assert.Null(promoCode);
        }

        [Fact]
        public void GetAppliedPromoCode_UsesSingleOfferWhenNoSelectedOfferIsPresent()
        {
            var offerResponse = new FlightOfferResponse
            {
                Offers = new[] { new OfferDetails { PromoCodeApplied = "uwf01" } }
            };

            var promoCode = BrandMsRequestMapper.GetAppliedPromoCode(offerResponse);

            Assert.Equal("uwf01", promoCode);
        }

        [Fact]
        public void BrandMsReqMapper_LeavesAccountCodeUnsetWhenNoPromoCodeIsApplied()
        {
            var offerResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var cabinInfo = TestDataProvider.GetTestData<IEnumerable<CabinInfo>>("CabinInfo");
            offerResponse.Offers.First().PromoCodeApplied = null;

            var result = BrandMsRequestMapper.BrandMsReqMapper(
                offerResponse,
                cabinInfo,
                new[] { "ADT" },
                BrandMsRequestMapper.GetAppliedPromoCode(offerResponse));

            Assert.Null(result.AccountCode);
        }

        [Fact]
        public void BrandMsReqMapper_MapsTicketDesignatorToBrandRequestSegment()
        {
            var offerResponse = TestDataProvider.GetTestData<FlightOfferResponse>("OfferPriceResponse");
            var cabinInfo = TestDataProvider.GetTestData<IEnumerable<CabinInfo>>("CabinInfo");
            var ticketDesignator = "UWF1";
            offerResponse.Offers.First().OfferItems.First().Services.First().FareDetails.FareSegments.First().TicketDesignator = ticketDesignator;

            var result = BrandMsRequestMapper.BrandMsReqMapper(offerResponse, cabinInfo, new[] { "ADT" });

            Assert.Contains(
                result.Itinerary.First().Segments,
                segment => segment.TicketDesignator == ticketDesignator);
        }
    }
}
