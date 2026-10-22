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
using VAA.EAPI.Automation.NDC.Tests.OrderChange;
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using VAA.EAPI.Automation.NDC.Tests.OrderQuote;
using VAA.EAPI.Automation.NDC.Tests.OrderReshop;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    public class OrderChangeUnEvenExchange : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderChangeUnEvenExchange(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdDanishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdDanishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSwedishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSwedishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSaudiAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSaudiAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCanadianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCanadianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCarrebianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCarrebianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCaribjetAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCaribjetAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdEuropeanAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdEuropeanAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdNorwegiansAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdNorwegiansAgent}-OcpSubID"));

        }


        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void OrderChangeRefund()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.DEL,locationThree:LocationType.DEL,locationFour:LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "Refund", orderChangePayment: "CPDVisa", agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string offerPriceResult = result1["offerPriceRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void OrderChangeAddCol()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, cabinCode: "5", cabinName: CabinType.EconomyClassic, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.SEA, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "CPDVisa",agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeExchangeFlowE2E)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol", "CPDMaster")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.SEA, LocationType.SEA, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol", "CPDMaster")]
        [InlineData(3, 210, 90, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.MIA, LocationType.MIA, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null, "AddCol", "CPDVisa")]
        [InlineData(4, 200, 50, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "AddCol", "CPDVisa")]
        [InlineData(5, 150, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "AddCol", "CPDVisa")]
        [InlineData(6, 145, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol", "CPDVisa")]
        public void AddColOrderChangeE2E(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType, string orderChangePayment)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType, orderChangePayment: orderChangePayment);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                #region order reshop validations
                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order quote validaton

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change even exchange assertions

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeExchangeFlowE2E)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol", "CPDMaster")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol", "CPDMaster")]
        public void AddColCompleteJourneyOrderChangePNRWithSeatE2E(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType, string orderChangePayment)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType, orderChangePayment: orderChangePayment);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                #region order reshop validations
                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateSeatBooking(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order quote validaton

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change even exchange assertions

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeExchangeFlowE2E)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null, "AddCol", "CPDMaster")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "AddCol", "CPDMaster")]
        public void AddColPartialJourneyOrderChangePNRWithSeatE2E(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType, string orderChangePayment)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType, orderChangePayment: orderChangePayment);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                #region order reshop validations
                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateSeatBooking(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order quote validaton

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change even exchange assertions

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeExchangeFlowE2E)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund", "CPDMaster")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.SEA, LocationType.SEA, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund", "CPDMaster")]
        [InlineData(3, 210, 90, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.MIA, LocationType.MIA, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null, "Refund", "CPDVisa")]
        [InlineData(4, 200, 50, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "Refund", "CPDVisa")]
        [InlineData(5, 150, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "Refund", "CPDVisa")]
        [InlineData(6, 145, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund", "CPDVisa")]
        public void RefundOrderChangeE2E(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType, string orderChangePayment)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType, orderChangePayment: orderChangePayment);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                #region order reshop validations
                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order quote validaton

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change even exchange assertions

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeExchangeFlowE2E)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund", "CPDMaster")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund", "CPDMaster")]
        public void RefundCompleteJourneyOrderChangePNRWithSeatE2E(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType, string orderChangePayment)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType, orderChangePayment: orderChangePayment);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                #region order reshop validations
                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateSeatBooking(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order quote validaton

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change even exchange assertions

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeExchangeFlowE2E)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null, "AddCol", "CPDMaster")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "Refund", "CPDMaster")]
        public void RefundPartialJourneyOrderChangePNRWithSeatE2E(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType, string orderChangePayment)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType, orderChangePayment: orderChangePayment);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                string orderQuoteRequest = result1["orderQuoteRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                #region order reshop validations
                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateSeatBooking(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order quote validaton

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change even exchange assertions

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        //AddCol
        [Theory]
        [Trait("Category", "NDC - OrderChangeUnEvenExchangeV2")]
        [InlineData(1, 100, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol")]
        [InlineData(2, 180, 60, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null, "AddCol")]
        public void OrderChangeOneWay(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, string orderChangePayment, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, orderChangePayment: orderChangePayment, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();
                XmlDocument updatedOrderChangeResponse = new XmlDocument();
                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdCanadianAgent, 1, 2, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDMaster", ChangeOrder.InBoundJourney, "Direct", null, "AddCol")]
        [InlineData(2, 60, 180, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDVisa", ChangeOrder.InBoundJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator, "AddCol")]
        public void OrderChangeInbound(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, string orderChangePayment, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, orderChangePayment: orderChangePayment, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 2, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "AddCol")]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdCaribjetAgent, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null, "AddCol")]
        public void OrderChangeOutbound(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, string orderChangePayment, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, orderChangePayment: orderChangePayment, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.BOM, LocationType.ATL, LocationType.ATL, LocationType.BOM, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null, "AddCol")]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.DEL, LocationType.SEA, LocationType.SEA, LocationType.DEL, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "AddCol")]
       public void OrderChangeConjunctionTkt(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, string orderChangePayment, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, orderChangePayment: orderChangePayment, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderQuoteResult = result1["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null, "AddCol")]
        public void OrderChangeTicketReissuemultipletimes(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, string orderChangePayment, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, orderChangePayment: orderChangePayment, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        //Refund
        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        public void OrderChange_OneWay(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out  result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.TLV, LocationType.TLV, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
       public void OrderChange_Return(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdCanadianAgent, 1, 2, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null, "Refund")]
        [InlineData(2, 60, 180, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator, "Refund")]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null, "Refund")]
        public void OrderChange_Inbound(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 2, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "Refund")]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdCaribjetAgent, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.OutBoundJourney, "Direct", null, "Refund")]
      public void OrderChange_Outbound(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.BOM, LocationType.ATL, LocationType.ATL, LocationType.BOM, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.DEL, LocationType.SEA, LocationType.SEA, LocationType.DEL, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.DEL, LocationType.MBJ, LocationType.MBJ, LocationType.DEL, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        public void OrderChange_ConjunctionTkt(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        public void OrderChange_OpenJaw(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        public void OrderChange_FreeSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdKayakUsa, 1, 2, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        public void OrderChange_PaidSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeUnEvenExchangeV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null, "Refund")]
        public void OrderChange_TicketReissuemultipletimes(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2, string exchangeType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderChangeRS, exchangeType: exchangeType);

            string orderChangeResult = result1["OrderChangeResult"];

            string orderChangeRequest = result1["orderChangeRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderQuoteResult = result1["orderQuoteRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //will be uncommented once the code is deployed
            //softAssert.True(OrderCreateAssertions.validateStatusCode(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateUnEvenPriceDifferential(orderCreateResult, orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }
    }
}
