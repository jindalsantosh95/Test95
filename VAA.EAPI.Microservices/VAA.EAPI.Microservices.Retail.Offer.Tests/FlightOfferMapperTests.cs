using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlightOfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using CommonPrice = VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class FlightOfferMapperTests
    {
        private readonly FlightOfferRequest _request;
        private readonly OffersSetList _offerResponse;
        private readonly ILogger _logger;

        public FlightOfferMapperTests()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set(Constants.Environment.IgnoreFlexFares, "false");

            _request = TestDataProvider.GetTestData<FlightOfferRequest>("OfferMS/Reward_Saver_OfferSearchRQ");

            var gqlResponse = TestDataProvider.GetTestData<GqlSearchOffersResponse>("OfferMS/Reward_Saver_DeltaGqlQueryRS");
            _offerResponse = gqlResponse.Data.SearchOffers;

            _logger = NullLogger.Instance;
        }

        [Fact]
        public void FlightOfferResponseMapper_AwardSearch_MapsOffersSuccessfully()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "false");

            var sliceZeroPrice = new List<CommonPrice>
            {
                new() { Amount = new() { MonetaryAmount = "201.99", BaseCurrency = "GBP" }, PriceComponentType = CommonPrice.PriceComponentTypes.BasePriceEnum },
                new() { PointsAmount = new() { PointsCount = 60000 }, PriceComponentType = CommonPrice.PriceComponentTypes.PointsEnum }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", sliceZeroPrice, _logger);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Offers);
            Assert.NotNull(result.DataList);
            Assert.NotEmpty(result.DataList.FlightSegments);
            Assert.NotEmpty(result.DataList.Journey);
            Assert.NotEmpty(result.DataList.CustomerDetails);
        }

        [Fact]
        public void FlightOfferResponseMapper_AwardSearch_OfferItemPricing_ContainsPointsComponent()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "false");

            var sliceZeroPrice = new List<CommonPrice>
            {
                new() { Amount = new() { MonetaryAmount = "201.99", BaseCurrency = "GBP" }, PriceComponentType = CommonPrice.PriceComponentTypes.BasePriceEnum },
                new() { PointsAmount = new() { PointsCount = 60000 }, PriceComponentType = CommonPrice.PriceComponentTypes.PointsEnum }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", sliceZeroPrice, _logger);

            var firstOfferItemPricing = result.Offers.First().OfferItems.First().OfferitemPricing.First();
            Assert.Contains(firstOfferItemPricing.SalePrice, p => p.PriceComponentType == CommonPrice.PriceComponentTypes.PointsEnum);
        }

        [Fact]
        public void FlightOfferResponseMapper_MapsTicketDesignatorFromFareEndorsement()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "false");

            var expectedTicketDesignator = _offerResponse.OffersSets.First().Offers.First().OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First().TicketDesignator;

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", null, _logger);
            var fareSegment = result.Offers.First().OfferItems.First().Services.First().FareDetails.FareSegments.First();

            Assert.Equal(expectedTicketDesignator, fareSegment.TicketDesignator);
        }

        [Fact]
        public void FlightOfferResponseMapper_BaselineFareDifferences_Enabled_SalePriceDifferencePopulated()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "true");

            // VSCL is the economy baseline brand in the test data (tripId=1), so offers with fewer points than the baseline should have a negative difference
            var sliceZeroPrice = new List<CommonPrice>
            {
                new() { Amount = new() { MonetaryAmount = "201.99", BaseCurrency = "GBP" }, PriceComponentType = CommonPrice.PriceComponentTypes.BasePriceEnum },
                new() { PointsAmount = new() { PointsCount = 60000 }, PriceComponentType = CommonPrice.PriceComponentTypes.PointsEnum }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", sliceZeroPrice, _logger);

            var vspeOffer = result.Offers.FirstOrDefault(o =>
                o.OfferItems.Any(oi => oi.OfferitemPricing.Any(p =>
                    p.SalePrice.Any(sp => sp.PriceComponentType == CommonPrice.PriceComponentTypes.PointsEnum && sp.PointsAmount.PointsCount == 31000))));

            Assert.NotNull(vspeOffer);
            var vspeItemPricing = vspeOffer.OfferItems.First().OfferitemPricing.First();
            Assert.NotNull(vspeItemPricing.SalePriceDifference);
            Assert.NotEmpty(vspeItemPricing.SalePriceDifference);

            var pointsDiff = vspeItemPricing.SalePriceDifference.First(p => p.PriceComponentType == CommonPrice.PriceComponentTypes.PointsEnum);
            Assert.Equal("-", pointsDiff.DifferenceDetails.PointsDifferenceSign);
            Assert.Equal(29000, pointsDiff.DifferenceDetails.PointsDifference);
        }

        [Fact]
        public void FlightOfferResponseMapper_BaselineFareDifferences_Disabled_BaselineOfferItems_NotUsed()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "false");

            var sliceZeroPrice = new List<CommonPrice>
            {
                new() { Amount = new() { MonetaryAmount = "201.99", BaseCurrency = "GBP" }, PriceComponentType = CommonPrice.PriceComponentTypes.BasePriceEnum },
                new() { PointsAmount = new() { PointsCount = 60000 }, PriceComponentType = CommonPrice.PriceComponentTypes.PointsEnum }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", sliceZeroPrice, _logger);

            Assert.All(
                result.Offers.SelectMany(o => o.OfferItems).SelectMany(oi => oi.OfferitemPricing),
                p => Assert.Null(p.SalePriceDifference));
        }

        [Fact]
        public void FlightOfferResponseMapper_AwardSearch_MapsSalesAdjustedPointsWhenBaselineFareDifferencesEnabled()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "true");

            const int baselineCabinPoints = 45000;
            const int salesAdjustedPoints = 86000;
            var offerSet = _offerResponse.OffersSets.First(offersSet =>
                offersSet.Offers.Any(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSCL") &&
                offersSet.Offers.Any(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSPE"));
            var economyOffer = offerSet.Offers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSCL");
            var premiumOffer = offerSet.Offers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSPE");
            var economyEndorsement = economyOffer.OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First();
            var premiumEndorsement = premiumOffer.OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First();
            economyEndorsement.SalesAdjustedPrice = new Infrastructure.Proxy.DeltaOffer.Price
            {
                MilesEquivalentPrice = new AwardPrice { MileCnt = baselineCabinPoints }
            };
            premiumEndorsement.SalesAdjustedPrice = new Infrastructure.Proxy.DeltaOffer.Price
            {
                MilesEquivalentPrice = new AwardPrice { MileCnt = salesAdjustedPoints }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", null, _logger);

            var fareSegment = result.Offers
                .SelectMany(offer => offer.OfferItems)
                .SelectMany(offerItem => offerItem.Services)
                .First(service => service.FareDetails.DominantFareProduct == "VSPE")
                .FareDetails.FareSegments.First();

            Assert.Equal(CommonPrice.PriceComponentTypes.PointsEnum, fareSegment.BaselineCabinSalePrice.PriceComponentType);
            Assert.Equal(baselineCabinPoints, fareSegment.BaselineCabinSalePrice.PointsAmount.PointsCount);
            Assert.Equal(CommonPrice.PriceComponentTypes.PointsEnum, fareSegment.CurrentCabinSalePrice.PriceComponentType);
            Assert.Equal(salesAdjustedPoints, fareSegment.CurrentCabinSalePrice.PointsAmount.PointsCount);
        }

        [Fact]
        public void FlightOfferResponseMapper_BaselineFareDifferences_UsesPremiumWhenEconomyIsSoldOut()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "true");

            var offerSet = _offerResponse.OffersSets.First(offersSet =>
                offersSet.Offers.Any(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSCL") &&
                offersSet.Offers.Any(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSPE") &&
                offersSet.Offers.Any(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSUP"));
            var economyOffer = offerSet.Offers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSCL");
            var premiumOffer = offerSet.Offers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSPE");
            var upperOffer = offerSet.Offers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSUP");
            var premiumPoints = GetAwardPoints(premiumOffer);
            var upperPoints = GetAwardPoints(upperOffer);
            const int premiumSalesAdjustedPoints = 40000;
            const int upperSalesAdjustedPoints = 71000;
            premiumOffer.OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First().SalesAdjustedPrice = new Infrastructure.Proxy.DeltaOffer.Price
            {
                MilesEquivalentPrice = new AwardPrice { MileCnt = premiumSalesAdjustedPoints }
            };
            upperOffer.OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First().SalesAdjustedPrice = new Infrastructure.Proxy.DeltaOffer.Price
            {
                MilesEquivalentPrice = new AwardPrice { MileCnt = upperSalesAdjustedPoints }
            };

            economyOffer.AdditionalOfferProperties.SoldOut = true;

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", null, _logger);

            var upperPricing = result.Offers
                .Single(offer => offer.OfferItems.First().Services.First().FareDetails.DominantFareProduct == "VSUP")
                .OfferItems.First().OfferitemPricing.First();
            var pointsDifference = upperPricing.SalePriceDifference.Single(price => price.PriceComponentType == CommonPrice.PriceComponentTypes.PointsEnum);

            Assert.Equal(upperPoints - premiumPoints, pointsDifference.DifferenceDetails.PointsDifference);
            Assert.Equal("+", pointsDifference.DifferenceDetails.PointsDifferenceSign);

            var upperFareSegment = result.Offers
                .SelectMany(offer => offer.OfferItems)
                .SelectMany(offerItem => offerItem.Services)
                .First(service => service.FareDetails.DominantFareProduct == "VSUP")
                .FareDetails.FareSegments.First();

            Assert.Equal(premiumSalesAdjustedPoints, upperFareSegment.BaselineCabinSalePrice.PointsAmount.PointsCount);
            Assert.Equal(upperSalesAdjustedPoints, upperFareSegment.CurrentCabinSalePrice.PointsAmount.PointsCount);
        }

        [Fact]
        public void FlightOfferResponseMapper_CabinSalePrices_AreNotMappedWhenBaselineFareDifferencesDisabled()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "false");

            var fareEndorsement = _offerResponse.OffersSets.First().Offers.First().OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First();
            fareEndorsement.SalesAdjustedPrice = new Infrastructure.Proxy.DeltaOffer.Price
            {
                MilesEquivalentPrice = new AwardPrice { MileCnt = 45000 }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", null, _logger);
            var fareSegment = result.Offers.First().OfferItems.First().Services.First().FareDetails.FareSegments.First();

            Assert.Null(fareSegment.BaselineCabinSalePrice);
            Assert.Null(fareSegment.CurrentCabinSalePrice);
        }

        [Fact]
        public void FlightOfferResponseMapper_CabinSalePrices_AreNotMappedForNonAwardSearch()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "true");
            _request.FlightsearchRequest.AwardSearch = false;

            var fareEndorsement = _offerResponse.OffersSets.First().Offers.First().OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().Endorsement.First();
            fareEndorsement.SalesAdjustedPrice = new Infrastructure.Proxy.DeltaOffer.Price
            {
                MilesEquivalentPrice = new AwardPrice { MileCnt = 45000 }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", null, _logger);
            var fareSegment = result.Offers.First().OfferItems.First().Services.First().FareDetails.FareSegments.First();

            Assert.Null(fareSegment.BaselineCabinSalePrice);
            Assert.Null(fareSegment.CurrentCabinSalePrice);
        }

        private static int GetAwardPoints(Infrastructure.Proxy.DeltaOffer.Offer offer)
        {
            return offer.OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().FarePrice.First().TotalFarePrice.MilesEquivalentPrice.MileCnt;
        }

        [Fact]
        public void FlightOfferResponseMapper_SalePrice_PriceDifference_Populated_WhenSliceZeroProvided()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "false");

            var sliceZeroPrice = new List<CommonPrice>
            {
                new() { Amount = new() { MonetaryAmount = "100.00", BaseCurrency = "GBP" }, PriceComponentType = CommonPrice.PriceComponentTypes.BasePriceEnum },
                new() { PointsAmount = new() { PointsCount = 60000 }, PriceComponentType = CommonPrice.PriceComponentTypes.PointsEnum }
            };

            var result = FlightOfferMapper.FlightOfferResponseMapper(_request, _offerResponse, "DOC123", sliceZeroPrice, _logger);

            var vsclOffer = result.Offers.First(o => o.OfferId.Contains("FL-1"));
            var basePrice = vsclOffer.SalePrice.First(p => p.PriceComponentType == CommonPrice.PriceComponentTypes.BasePriceEnum);
            Assert.Equal("+", basePrice.Amount.PriceDifferenceSign);
            Assert.True(basePrice.Amount.PriceDifference > 0);
        }

        [Fact]
        public void OfferPriceResponseMapper_BaselineFareDifferences_UsesEconomyOfferAcrossOfferSets()
        {
            ConfigReader.Set(Constants.Environment.EnableOfferBaselineFareDifferences, "true");

            var allOffers = (_offerResponse.SelectedOfferSets ?? Enumerable.Empty<OffersSet>())
                .Concat(_offerResponse.RecommendedOfferSets ?? Enumerable.Empty<OffersSet>())
                .Concat(_offerResponse.OffersSets ?? Enumerable.Empty<OffersSet>())
                .SelectMany(offerSet => offerSet.Offers);
            var economyOffer = allOffers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSCL");
            var premiumOffer = allOffers.First(offer => offer.AdditionalOfferProperties?.DominantSegmentBrandId == "VSPE");
            _offerResponse.OfferDataList.Penalties = Enumerable.Empty<Penalty>();
            foreach (var offer in new[] { economyOffer, premiumOffer })
            {
                foreach (var offerPricing in offer.OfferPricing.Concat(offer.OfferItems.SelectMany(offerItem => offerItem.RetailItems).SelectMany(retailItem => retailItem.RetailItemPricing)))
                {
                    offerPricing.TaxSummary.Taxes = Enumerable.Empty<TaxType>();
                }
            }
            var offerPriceResponse = new OffersSetList
            {
                OfferDataList = _offerResponse.OfferDataList,
                SelectedOfferSets = [new OffersSet { Offers = [premiumOffer] }],
                RecommendedOfferSets = [new OffersSet { Offers = [economyOffer] }]
            };

            var result = OfferPriceRsMapper.OfferPriceResMapper(_request.Pos, offerPriceResponse, "DOC123", true, null);

            var pricing = result.Offers.Single(offer => offer.Selected).OfferItems.First().OfferitemPricing.First();
            Assert.NotNull(pricing.SalePriceDifference);
            Assert.Contains(pricing.SalePriceDifference, price => price.PriceComponentType == CommonPrice.PriceComponentTypes.PointsEnum);
        }
    }
}
