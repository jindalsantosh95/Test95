using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.OrderReshop;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OrderQuote
{
    public class OrderQuoteRegression : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public OrderQuoteRegression(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));

        }


        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void OrderQuoteE2E()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails( orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }


        [Theory]
        [Trait("Category", "OrderQuoteRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.OneWay, LocationType.LHR, LocationType.IAD, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        public void OrderQuoteOneWay(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);

        }

        [Theory]
        [Trait("Category", "OrderQuoteRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(5, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.SFO, LocationType.SFO, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        public void OrderQuoteReturnCompleteJourneyChange(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "OrderQuoteRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "UntokenizedAmex", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.MCO, LocationType.MCO, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.BOM, LocationType.ATL, LocationType.ATL, LocationType.BOM, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        public void OrderQuoteReturnOutBoundJourneyChange(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);

        }

        [Theory]
        [Trait("Category", "OrderQuoteRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(2, 60, 180, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.EWR, LocationType.EWR, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        public void OrderQuoteReturnInBoundJourneyChange(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "OrderQuoteRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.TLV, LocationType.TLV, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 50, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDMaster", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(4, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(6, 100, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        public void OrderQuotePNRWithSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);

        }

        [Theory]
        [Trait("Category", "OrderQuoteRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        public void OrderQuoteMulticity(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderQuoteRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                string orderCreateRequest = result1["orderCreateRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Trait("Category", "NDC-ProdSanity")]
        [Fact]
        public void Quote_OrdinalValueZero()
        {
            XmlDocument xmlDocument_1 = new XmlDocument();

            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = new XmlNamespaceManager(updatedRequest.NameTable);
            responseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedRequest.SelectSingleNode("//def:Ordinal", responseXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderQuote(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid ordinal number - should start from 1"), $"Error code expected is Invalid ordinal number - should start from 1 but actual is {descText}");

        }
    }
}
