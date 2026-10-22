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
    public class ITTTL : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public ITTTL(MemoryStoreFixture memoryStoreFixture)
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

        [Theory]
        [Trait("Category", Traits.NDCITTLL)]

        [InlineData( 1, 1, 1, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "2", CabinType.UpperClass, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, null, "70J", FareSubTypeCode.None, "CPDVisa", SeatPriceType.Paid, "CPDVisa", 16)]
        [InlineData( 2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.SEA, null, "Direct", FareSubTypeCode.None, "CPDMaster", SeatPriceType.None, "CPDVisa", 16)]
         
        public void IT_TTL_E2E( int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, string corporate, string fareTypeCode, FareSubTypeCode fareSubType, string payment, SeatPriceType seatPriceType, string orderChangePayment, int timeRange)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = TTLShoppingHelper.TTLShoppingRequest(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationTwo, locationFour: locationOne,
                    locationFive: LocationType.None, locationSix: LocationType.None, offerRequestType: "PrivateFare", taxonomy: "taxonomy", corporate: corporate, fareTypeCode: fareTypeCode, fareSubType: fareSubType, paymentType: payment, changeOrder: ChangeOrder.CompleteJourney, exchangeType: "AddCol", seatType: seatPriceType, orderChangePayment: orderChangePayment, timerange: timeRange);

                string orderChangeResult = result1["OrderChangeResult"];

                string orderChangeRequest = result1["orderChangeRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

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

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);

            }, 2);
        }
    }
}
