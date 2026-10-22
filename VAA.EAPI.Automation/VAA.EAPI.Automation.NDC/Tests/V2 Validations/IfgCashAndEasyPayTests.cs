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
using VAA.EAPI.Automation.NDC.Tests.OrderReshop;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.V2Validations
{
    public class IfgCashAndEasyPayTests : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public IfgCashAndEasyPayTests(MemoryStoreFixture memoryStoreFixture)
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
        public void IfgCashOrderChangeCancelRFCReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "2", cabinName: CabinType.UpperClass, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.LOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "Cash", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                Dictionary<string, string> cancelPnrResult = UpdatePNRDetails.CancelPNR(airShoppingRequest, orderCreateResult, apiResult: ApiResult.OrderChangeRS);

                string orderChangeCancelRQ = cancelPnrResult["OrderChangeCancelRQ"];

                string orderChangeCancelRS = cancelPnrResult["OrderChangeCancelRS"];

                string orderReshopCancelRQ = cancelPnrResult["orderReshopCancelRQ"];

                string orderReshopCancelRS = cancelPnrResult["orderReshopCancelRS"];

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
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgCashOrderCreateHoldBookingReturnAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateResponses = OrderCreateHelper.CreatePnr(120, 158,participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.SFO, locationThree: LocationType.SFO, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "HoldBooking", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

                dynamic orderChangeRequest = UpdatePNRDetails.OrderChangeHeldBooking(airShoppingRequest, orderCreateResult,orderChangePayment:"Cash");

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
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgCashOrderCreateInstantPurchaseFreeSeat()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.PremiumFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "Cash", seatType: SeatPriceType.Free);

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderCreateResult, out result, paymentType: "Cash"), "Order Create Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                HttpResponseMessage orderRetrieveResponse = null;

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                string orderRetrieveResult = "";

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                    orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.True(OrderCreateAssertions.ValidateSeatRetrieve(orderCreateResult, orderRetrieveResult, out ValidationResultHelper error), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                }, 3, 1000);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderRetrieveResult, out result, paymentType: "Cash"), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateSeatRetrieve(orderCreateResult, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                NDCTransformationEndpoints.OrderCancel(OrderPaymentRequestBuilderHelper.PnrCancel(FindAndReturnDataHelper.ReturnBookingId(orderCreateResult)));

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgCashOrderChangeAddCol()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassic, flight: FlightScenarioType.Virgin, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "Cash", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "Cash", agentEmailId: "Bhaskar.Singh@fly.virgin.com");

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

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgCashOrderChangeRefund()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "Refund", orderChangePayment: "Cash", agentEmailId: "Bhaskar.Singh@fly.virgin.com");

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

                softAssert.True(OrderChangeValidators.ValidateRefundResidualWarning(orderQuoteResult, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgCashOrderChangeEvenexchange()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(200, 100, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "4", cabinName: CabinType.PremiumFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOM, bookingType: "InstantPurchase", paymentType: "Cash", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "Evenexchange", orderChangePayment: "Cash", agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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
        public void IfgEasyPayOrderCreateHoldBookingReturnAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateResponses = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdEasyPayGBPAgent, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "HoldBooking", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

                dynamic orderChangeRequest = UpdatePNRDetails.OrderChangeHeldBooking(airShoppingRequest, orderCreateResult, orderChangePayment: "EasyPay",accountNumber: "2235291539831600");

                dynamic orderChangeResponses = orderChangeRequest["OrderChangeConifrmHeldBookingRS"];

                dynamic orderChangeRequests = orderChangeRequest["OrderChangeConifrmHeldBookingRQ"];

                dynamic orderQuoteResponse = orderChangeRequest["orderQuoteHoldBookingRS"];

                dynamic orderQuoteRequest = orderChangeRequest["orderQuoteHoldBookingRQ"];

                XmlDocument updatedorderChangeResponses = new XmlDocument();

                updatedorderChangeResponses.LoadXml(orderChangeResponses);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResponses, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderChangeRequests, orderChangeResponses, out result, paymentType: "EasyPay"), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

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
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgEasyPayOrderChangeCancelRFCReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdEasyPayGBPAgent, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "EasyPay",accountNumber: "2235291539831600", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                Dictionary<string, string> cancelPnrResult = UpdatePNRDetails.CancelPNR(airShoppingRequest, orderCreateResult, apiResult: ApiResult.OrderChangeRS);

                string orderChangeCancelRQ = cancelPnrResult["OrderChangeCancelRQ"];

                string orderChangeCancelRS = cancelPnrResult["OrderChangeCancelRS"];

                string orderReshopCancelRQ = cancelPnrResult["orderReshopCancelRQ"];

                string orderReshopCancelRS = cancelPnrResult["orderReshopCancelRS"];

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
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void IfgEasyPayOrderCreateInstantPurchaseFreeSeat()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdEasyPayGBPAgent, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.Premium, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BGI, locationThree: LocationType.BGI, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "EasyPay", seatType: SeatPriceType.Free, accountNumber: "2235291539831600", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderCreateResult, out result, paymentType: "EasyPay"), "Order Create Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                HttpResponseMessage orderRetrieveResponse = null;

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                string orderRetrieveResult = "";

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                    orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.True(OrderCreateAssertions.ValidateSeatRetrieve(orderCreateResult, orderRetrieveResult, out ValidationResultHelper error), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                }, 3, 1000);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderRetrieveResult, out result, paymentType: "EasyPay"), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateSeatRetrieve(orderCreateResult, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                NDCTransformationEndpoints.OrderCancel(OrderPaymentRequestBuilderHelper.PnrCancel(FindAndReturnDataHelper.ReturnBookingId(orderCreateResult)));

                Assert.True(true);
            }, 1);
        }


        [Fact]
        [Trait("Category", "Local-IfgCash")]
        public void IfgCashOrderCreateInstantPurchasePaidSeat()
        {
            RetryHelper.RunWithRetry(() =>
            {

                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(120, 158, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.MIA, locationThree: LocationType.MIA, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "Cash", seatType: SeatPriceType.Paid, agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com", seatPaymentCardType: "CPDVisa");

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string airShoppingResult = result1["airShoppingResult"];

                string offerPriceResult = result1["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateTicketDocumentSeat(orderCreateRequest, orderCreateResult, out ValidationResultHelper result2), "Validation failed. Reasons: " + string.Join(", ", result2.Message));

                softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result2), "Validation failed. Reasons: " + string.Join(", ", result2.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "InstantPurchase"), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocumentSeat(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateSeatRetrieve(orderCreateResult, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                NDCTransformationEndpoints.OrderCancel(OrderPaymentRequestBuilderHelper.PnrCancel(FindAndReturnDataHelper.ReturnBookingId(orderCreateResult)));

                Assert.True(true);
            }, 1);
        }

    }
}
