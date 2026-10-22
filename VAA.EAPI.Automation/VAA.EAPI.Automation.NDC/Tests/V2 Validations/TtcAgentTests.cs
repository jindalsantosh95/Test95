using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.OrderChange;
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using VAA.EAPI.Automation.NDC.Tests.OrderQuote;
using VAA.EAPI.Automation.NDC.Tests.OrderReshop;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.V2Validations
{
    public class TtcAgentTests : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public TtcAgentTests(MemoryStoreFixture memoryStoreFixture)
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
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void OrderCreateHoldBookingReturnAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateResponses = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.LOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "HoldBooking", paymentType: "CPDVisa", agentMailId: "Bhaskar.Singh@fly.virgin.com", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator, agentType: "InDirect");

                string orderCreateResult = orderCreateResponses["orderCreateResult"];

                string orderCreateRequest = orderCreateResponses["orderCreateRQ"];

                string airShoppingResult = orderCreateResponses["airShoppingResult"];

                string airShoppingRequest = orderCreateResponses["airShoppingRQ"];

                string offerPriceResult = orderCreateResponses["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderCreateResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Reshop update Pax Name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderRetrieveResult, offerPriceResult, out result), "Order Retrieve    Validation failed. Reasons: " + string.Join(", ", result.Message));

                dynamic orderChangeRequest = UpdatePNRDetails.OrderChangeHeldBooking(airShoppingRequest, orderCreateResult);

                dynamic orderChangeResponses = orderChangeRequest["OrderChangeConifrmHeldBookingRS"];

                dynamic orderChangeRequests = orderChangeRequest["OrderChangeConifrmHeldBookingRQ"];

                dynamic orderQuoteResponse = orderChangeRequest["orderQuoteHoldBookingRS"];

                dynamic orderQuoteRequest = orderChangeRequest["orderQuoteHoldBookingRQ"];

                XmlDocument updatedorderChangeResponses = new XmlDocument();

                updatedorderChangeResponses.LoadXml(orderChangeResponses);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResponses, out result, flow: "InstantPurchase"), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderQuoteValidator.ValidateRequotePaymentFunctions(offerPriceResult, orderQuoteResponse, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void OrderChange_DateChange()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderChangeHelperResult = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassic, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.SEA, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "InDirect", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "CPDVisa", agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com", reshopFlightType: "Direct", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator);

                string orderChangeResult = orderChangeHelperResult["OrderChangeResult"];

                string orderChangeRequest = orderChangeHelperResult["orderChangeRQ"];

                string orderCreateResult = orderChangeHelperResult["orderCreateResult"];

                string orderCreateRequest = orderChangeHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderChangeHelperResult["airShoppingRQ"];

                string offerPriceResult = orderChangeHelperResult["offerPriceRS"];

                string orderQuoteResult = orderChangeHelperResult["orderQuoteRS"];

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
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void IfgCashOrderCreateInstantPurchase()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyLight, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.LOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "Cash", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator, agentType: "InDirect");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderCreateResult, out result, paymentType: "Cash"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderCreateResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["ADULT_1"]);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "InstantPurchase"), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderRetrieveResult, out result, paymentType: "Cash"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderRetrieveResult, offerPriceResult, out result), "Order Retrieve    Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderRetrieveResult, out result, passport: "passport", visa: "visa"), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Order Reshop update pax name Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                NDCTransformationEndpoints.OrderCancel(OrderPaymentRequestBuilderHelper.PnrCancel(FindAndReturnDataHelper.ReturnBookingId(orderCreateResult)));

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void OrderChangeNameCorrectionSinglePax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(200, 100, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 1, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.PremiumFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator, agentType: "InDirect");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                //update adult pax name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                //Getting the Order retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                XmlDocument orderChangeUpdatedPaxNameXmlDocument = new XmlDocument();

                //validate Adult_1 name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

                XmlNamespaceManager xmlNamespaceManager = orderChangeUpdatedPaxNameXmlDocument.CreateXmlNamespaceManager();

                string adult1UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
                {
                    result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void OrderChangeCancelRFCReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.MIA, locationThree: LocationType.MIA, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator, agentType: "InDirect");

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                Dictionary<string, string> result4 = UpdatePNRDetails.CancelPNR(airShoppingRequest, orderCreateResult, apiResult: ApiResult.OrderChangeRS);

                string orderChangeCancelRQ = result4["OrderChangeCancelRQ"];

                string orderChangeCancelRS = result4["OrderChangeCancelRS"];

                string orderReshopCancelRQ = result4["orderReshopCancelRQ"];

                string orderReshopCancelRS = result4["orderReshopCancelRS"];

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateRfcOrderViewRS(orderCreateResult, orderChangeCancelRS, out ValidationResultHelper result, flow: "Rfc"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateReshopCancelRS(airShoppingRequest, orderReshopCancelRS, out result, flow: "Rfc"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderCreateResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "Cancel"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void IfgCashOrderChangeAddCol()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 1, numberOfinfants: 1, cabinCode: "5", cabinName: CabinType.EconomyDelightFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "Cash", changeOrder: ChangeOrder.InBoundJourney, agentType: "InDirect", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "Cash", agentEmailId: "Bhaskar.Singh@fly.virgin.com", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator);

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
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void IfgCashOrderCreateHoldBookingReturnAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateResponses = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 1, numberOfinfants: 1, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "HoldBooking", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator, agentType: "InDirect");

                string orderCreateResult = orderCreateResponses["orderCreateResult"];

                string orderCreateRequest = orderCreateResponses["orderCreateRQ"];

                string airShoppingResult = orderCreateResponses["airShoppingResult"];

                string airShoppingRequest = orderCreateResponses["airShoppingRQ"];

                string offerPriceResult = orderCreateResponses["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderCreateResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Reshop update Pax Name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderRetrieveResult, offerPriceResult, out result), "Order Retrieve    Validation failed. Reasons: " + string.Join(", ", result.Message));

                dynamic orderChangeRequest = UpdatePNRDetails.OrderChangeHeldBooking(airShoppingRequest, orderCreateResult, orderChangePayment: "Cash");

                dynamic orderChangeResponses = orderChangeRequest["OrderChangeConifrmHeldBookingRS"];

                dynamic orderChangeRequests = orderChangeRequest["OrderChangeConifrmHeldBookingRQ"];

                dynamic orderQuoteResponse = orderChangeRequest["orderQuoteHoldBookingRS"];

                dynamic orderQuoteRequest = orderChangeRequest["orderQuoteHoldBookingRQ"];

                XmlDocument updatedorderChangeResponses = new XmlDocument();

                updatedorderChangeResponses.LoadXml(orderChangeResponses);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderChangeRequests, orderChangeResponses, out result, paymentType: "Cash"), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResponses, out result, flow: "InstantPurchase"), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCTtcAgentTests)]
        public void OrderChangeSplit()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgPreviewAgent, numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 1, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com",participatingOrgOrgID2: AgentID.ParticipatingOrgIdTTCAggregator, agentType: "InDirect");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                string[] splitPax = { "ADULT_1", "CHILD_1" };

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedXmlRQ = new XmlDocument();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();
                XmlDocument airShoppingDoc = new XmlDocument();

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                airShoppingDoc.LoadXml(airShoppingResult);

                XmlNodeList paxNodes = airShoppingDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                List<string> parentPnrPax = new List<string>();

                List<string> childPnrPax = splitPax.Select(x => x.ToString()).ToList();

                XmlDocument xmlRsDoc = new XmlDocument();

                string orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, childPnrPax);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                HttpResponseMessage orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml).EnsureIsSuccessStatusCode();

                string orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                XmlDocument orderChangeParentPnrRSDoc = new XmlDocument();

                XmlDocument orderChangeChildPnrRSDoc = new XmlDocument();

                orderChangeChildPnrRSDoc.LoadXml(orderChangeSplitPnrResult);

                orderChangeParentPnrRSDoc.LoadXml(orderChangeSplitPnrResult);

                //get parent pnr pax
                foreach (XmlNode paxNode in paxNodes)
                {
                    XmlNode paxIdNode = paxNode.SelectSingleNode("ct:PaxID", xmlNamespaceManager);
                    if (paxIdNode != null)
                    {
                        string paxId = paxIdNode.InnerText;
                        if (!childPnrPax.Contains(paxId))
                        {
                            orderChangeParentPnrRSDoc.SelectSingleNode($"//ct:DataLists//ct:PaxList//ct:Pax[ct:paxID='{paxId}']", xmlNamespaceManager);
                            parentPnrPax.Add(paxId);
                        }
                    }
                }

                //remove child pnr pax from parent pnr
                XmlNodeList removeChildPnrPaxNodes = orderChangeParentPnrRSDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                XmlNode removeParentOrderNode = orderChangeChildPnrRSDoc.SelectSingleNode("//ct:Order[1]", xmlNamespaceManager);

                removeParentOrderNode.ParentNode.RemoveChild(removeParentOrderNode);

                foreach (XmlNode paxNode in removeChildPnrPaxNodes)
                {
                    XmlNode paxIdNode = paxNode.SelectSingleNode("ct:PaxID", xmlNamespaceManager);
                    if (paxIdNode != null)
                    {
                        string paxId = paxIdNode.InnerText;
                        if (parentPnrPax.Contains(paxId))
                        {
                            paxNode.ParentNode.RemoveChild(paxNode);
                        }
                    }
                }

                //remove parent pnr pax from child pnr
                XmlNodeList removeParentPnrPaxNodes = orderChangeChildPnrRSDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                XmlNode removeChildOrderNode = orderChangeParentPnrRSDoc.SelectSingleNode("//ct:Order[2]", xmlNamespaceManager);

                removeChildOrderNode.ParentNode.RemoveChild(removeChildOrderNode);

                foreach (XmlNode paxNode in removeParentPnrPaxNodes)
                {
                    XmlNode paxIdNode = paxNode.SelectSingleNode("ct:PaxID", xmlNamespaceManager);
                    if (paxIdNode != null)
                    {
                        string paxId = paxIdNode.InnerText;
                        if (childPnrPax.Contains(paxId))
                        {
                            paxNode.ParentNode.RemoveChild(paxNode);
                        }
                    }
                }

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                //retrieve parent pnr
                string orderRetrieveParentPnrRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderChangeParentPnrRSDoc.OuterXml);

                HttpResponseMessage orderRetrieveParentPnrResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveParentPnrRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveParentPnrResult = orderRetrieveParentPnrResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //retrieve child pnr
                string orderRetrieveChildPnrRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderChangeChildPnrRSDoc.OuterXml);

                HttpResponseMessage orderRetrieveChildPnrResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveChildPnrRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveChildPnrResult = orderRetrieveChildPnrResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderChangeParentPnrRSDoc.OuterXml, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveChildPnrResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveChildPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveChildPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveChildPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveChildPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveChildPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveChildPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderRetrieveChildPnrResult, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveParentPnrResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveParentPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveParentPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveParentPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveParentPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveParentPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveParentPnrResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderRetrieveParentPnrResult, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

    }
}
