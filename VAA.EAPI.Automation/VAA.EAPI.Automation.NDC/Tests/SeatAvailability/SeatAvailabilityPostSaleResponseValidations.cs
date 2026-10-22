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
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class SeatAvailabilityPostSaleResponseValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public SeatAvailabilityPostSaleResponseValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", Traits.NDCSanityV2)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Paid, "Direct", null, "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, "Direct", null, "CPDMaster")]
        [InlineData(3, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDMaster")]
        public void OrderChange_PostSaleSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, apiResult: ApiResult.OrderChangeSeatRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderChangeSeatRequest = result1["orderChangeSeatRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string orderChangeSeatResult = result1["orderChangeSeatRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //post sale seat response validations

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "post sale seat Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                //order change seat response validations

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                // order retrieve
                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = null;

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();
                    Assert.True(true);
                }, 5);

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

                if (seatType.Equals(SeatPriceType.Paid))
                {
                    softAssert.True(OrderCreateAssertions.ValidateOrderChangeSeatPrice(orderChangeSeatRequest, orderCreateResult, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                    softAssert.True(OrderChangeValidators.ValidateOrderChangeSeatTicketDocument(orderChangeSeatRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                    softAssert.True(OrderCreateAssertions.ValidateOrderChangeSeatPrice(orderChangeSeatRequest, orderCreateResult, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                    softAssert.True(OrderChangeValidators.ValidateOrderChangeSeatTicketDocument(orderChangeSeatRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                }

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();
                Assert.True(true);
            }, 1);
        }


        [Theory]
        [Trait("Category", "OrderChangeSeat")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Paid, "Direct", null, "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Paid, "Direct", null, "CPDMaster")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.None, SeatPriceType.Paid, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, "Direct", null, "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, "Direct", null, "CPDMaster")]
        public void OrderChange_PaidSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, apiResult: ApiResult.OrderChangeSeatRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderChangeSeatRequest = result1["orderChangeSeatRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string orderChangeSeatResult = result1["orderChangeSeatRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                #region post sale seat response validations

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "post sale seat Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change seat response validations

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderChangeSeatPrice(orderChangeSeatRequest, orderCreateResult, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderChangeSeatTicketDocument(orderChangeSeatRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

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

                softAssert.True(OrderCreateAssertions.ValidateOrderChangeSeatPrice(orderChangeSeatRequest, orderCreateResult, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderChangeSeatTicketDocument(orderChangeSeatRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "OrderChangeSeat")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDMaster")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.None, SeatPriceType.Free, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDMaster")]
        public void OrderChange_FreeSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, apiResult: ApiResult.OrderChangeSeatRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderChangeSeatRequest = result1["orderChangeSeatRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string orderChangeSeatResult = result1["orderChangeSeatRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                #region post sale seat response validations

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "post sale seat Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change seat response validations

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

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

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "OrderChangeSeat")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.None, SeatPriceType.Free, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, "Direct", null, "CPDMaster")]
        public void OrderChange_DLFreeSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, apiResult: ApiResult.OrderChangeSeatRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType, operatorName: FlightType.DL);

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderChangeSeatRequest = result1["orderChangeSeatRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string orderChangeSeatResult = result1["orderChangeSeatRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                #region post sale seat response validations

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "post sale seat Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "post sale seat Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order change seat response validations

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region order retrieve

                Thread.Sleep(2000);

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

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

                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_FreeSeatOneSegmentReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(260).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 50;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.Return,
                   locationOne: LocationType.LHR, locationTwo: LocationType.TLV, LocationType.TLV, LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

                //Post Air Shopping request
                HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

                Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

                //Getting the Air Shopping response and converting to XML Document
                string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().Result;
                XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
                airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

                //Adding namespace manager
                XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
                airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                //Find offer based on Cabin Type and Flight Scenraio
                Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

                Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

                string offerRefId = offerDetails["OfferRefID"];
                string totalAmount = offerDetails["TotalAmount"];
                string shoppingRefId = offerDetails["ShoppingResponseRefID"];
                string currencyCode = offerDetails["CurrencyCode"];

                //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
                string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

                //Post Offer Price request
                HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

                Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

                //Getting the Offer Price response and converting to XML Document
                string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().Result;
                XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
                offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

                //Adding namespace manager
                XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
                offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().Result;

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "UnsecureUntokenized", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[6]", orderCreateRequestXmlnsManager);
                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

                XmlNode nodeToRemove1 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[5]", orderCreateRequestXmlnsManager);
                nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

                XmlNode nodeToRemove2 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[4]", orderCreateRequestXmlnsManager);
                nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode, $"Success status code was not returned from Seat Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {seatAvailabilityRequest1}");

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult1, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                int seatMap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

                Assert.True(seatMap == 1, $"Only SEG2 seat map is expected");

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult1, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "OfflineAgentUntokenized");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                XmlNode nodeToRemove6 = updatedOrderChangeSeatRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
                nodeToRemove6.ParentNode.RemoveChild(nodeToRemove6);

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                Assert.True(orderChangeSeatResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeSeatResponse.StatusCode} with message: {orderChangeSeatResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeSeatRequest}");

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll()
;
                Assert.True(true);
            }, 3);
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_1PaxWithFreeSeatOneSegment()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
              JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().Result;
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().Result;
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().Result;

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "UnsecureUntokenized", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

            Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode, $"Success status code was not returned from Seat Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {seatAvailabilityRequest1}");

            string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult1);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

            Assert.False(eligiblePax.Has("ADULT_1"), $"ADULT_1 is not expected in eligibility list");

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult1, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNode nodeToRemove3 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]", orderCreateRequestXmlnsManager);
            nodeToRemove3.ParentNode.RemoveChild(nodeToRemove3);

            XmlNode seatprice = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            Assert.True(orderChangeSeatResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeSeatResponse.StatusCode} with message: {orderChangeSeatResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeSeatRequest}");

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_FreeSeatReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(260).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, LocationType.TLV, LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().Result;
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().Result;
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().Result;

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "UnsecureUntokenized", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove1 = updatedOrderCreateRequest.SelectSingleNode("//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);
            //Removing FlightRelatedCriteria node as flight type is not requested
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            XmlNode nodeToRemove2 = updatedOrderCreateRequest.SelectSingleNode("//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);
            //Removing FlightRelatedCriteria node as flight type is not requested
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);

            XmlNode nodeToRemove3 = updatedOrderCreateRequest.SelectSingleNode("//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);
            //Removing FlightRelatedCriteria node as flight type is not requested
            nodeToRemove3.ParentNode.RemoveChild(nodeToRemove3);

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

            Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode, $"Success status code was not returned from Seat Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {seatAvailabilityRequest1}");

            string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult1);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

            int seatMap = updatedSeatResponse.SelectNodes("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).Count;

            XmlNode[] offerItem = updatedSeatResponse.SelectNodes("//def:ALaCarteOffer//def:OfferItem", updatedSeatRequestXmlnsManager).OfType<XmlNode>().ToArray();

            foreach (XmlElement element in offerItem)
            {
                string paxSegmentRefID = element.SelectSingleNode("//def:OfferItem//def:Eligibility//def:OfferFlightAssociations//def:PaxSegmentReferences//def:PaxSegmentRefID", updatedSeatRequestXmlnsManager).InnerText;

                string paxRefID = element.SelectSingleNode("//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                Assert.False(paxRefID.Has("ADULT") && paxSegmentRefID.Equals("SEG1"), $"Seat is already booked for ADULT_1 first segment");

                Assert.False(paxRefID.Has("YOUNG") && paxSegmentRefID.Equals("SEG2"), $"Seat is already booked for YOUNG_1 second segment");

                Assert.False(paxRefID.Has("CHILD") && paxSegmentRefID.Equals("SEG2"), $"Seat is already booked for CHILD_1 second segment");

            }

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult1, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNode nodeToRemove4 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]", orderCreateRequestXmlnsManager);
            nodeToRemove4.ParentNode.RemoveChild(nodeToRemove4);

            XmlNode nodeToRemove5 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[4]", orderCreateRequestXmlnsManager);
            nodeToRemove5.ParentNode.RemoveChild(nodeToRemove5);

            XmlNode nodeToRemove6 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[4]", orderCreateRequestXmlnsManager);
            nodeToRemove6.ParentNode.RemoveChild(nodeToRemove6);


            XmlNode nodeToRemove7 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove7.ParentNode.RemoveChild(nodeToRemove7);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            Assert.True(orderChangeSeatResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeSeatResponse.StatusCode} with message: {orderChangeSeatResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeSeatRequest}");

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            Assert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void PrimeAndPostSalePartialSeat()
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(120, 158, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BGI, locationThree: LocationType.BGI, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", seatType: SeatPriceType.Paid, apiResult: ApiResult.OrderCreateRQ);

            string airShoppingRequest = result1["airShoppingRQ"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string seatAvailabilityResult = result1["seatAvailabilityResult"];

            XmlDocument seatresponsedocument = new XmlDocument();

            seatresponsedocument.LoadXml(seatAvailabilityResult);

            XmlDocument orderCreateRequestDocument = new XmlDocument();

            orderCreateRequestDocument.LoadXml(orderCreateRequest);

            XmlNamespaceManager xmlNamespaceManager = orderCreateRequestDocument.CreateXmlNamespaceManager();

            string offerID = orderCreateRequestDocument.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[1]//ct:OfferItemRefID", xmlNamespaceManager).InnerText;

            string totalticketprice = orderCreateRequestDocument.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[1]//ct:TotalOfferPriceAmount", xmlNamespaceManager).InnerText;

            XmlNodeList seatOfferItems = orderCreateRequestDocument.SelectNodes("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem", xmlNamespaceManager);

            //removing seg2 seat offeritems
            XmlNode removeAdultSeatNode = orderCreateRequestDocument.SelectSingleNode($"//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[6]", xmlNamespaceManager);
            removeAdultSeatNode.ParentNode.RemoveChild(removeAdultSeatNode);

            XmlNode removeYouthSeatNode = orderCreateRequestDocument.SelectSingleNode($"//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[5]", xmlNamespaceManager);
            removeYouthSeatNode.ParentNode.RemoveChild(removeYouthSeatNode);

            XmlNode removeChildSeatNode = orderCreateRequestDocument.SelectSingleNode($"//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[4]", xmlNamespaceManager);
            removeChildSeatNode.ParentNode.RemoveChild(removeChildSeatNode);

            XmlDocument seatResponseDcoument = new XmlDocument();
            seatResponseDcoument.LoadXml(seatAvailabilityResult);

            string adultSeatPrice = seatResponseDcoument.SelectSingleNode($"//ct:OfferItem[ct:OfferItemID='{orderCreateRequestDocument.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[1]/ct:OfferItemRefID", xmlNamespaceManager).InnerText}']//ct:UnitPrice//ct:TotalAmount", xmlNamespaceManager).InnerText;

            string youthSeatPrice = seatResponseDcoument.SelectSingleNode($"//ct:OfferItem[ct:OfferItemID='{orderCreateRequestDocument.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[2]/ct:OfferItemRefID", xmlNamespaceManager).InnerText}']//ct:UnitPrice//ct:TotalAmount", xmlNamespaceManager).InnerText;

            string infantSeatPrice = seatResponseDcoument.SelectSingleNode($"//ct:OfferItem[ct:OfferItemID='{orderCreateRequestDocument.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:SelectedOfferItem[3]/ct:OfferItemRefID", xmlNamespaceManager).InnerText}']//ct:UnitPrice//ct:TotalAmount", xmlNamespaceManager).InnerText;

            double totalseatPrice = Math.Round(Convert.ToDouble(adultSeatPrice) + Convert.ToDouble(youthSeatPrice) + Convert.ToDouble(infantSeatPrice), 2);

            totalticketprice = Convert.ToString(Math.Round(Convert.ToDouble(totalticketprice), 2) + totalseatPrice);

            orderCreateRequestDocument.SelectSingleNode($"//ct:PaymentFunctions//ct:PaymentProcessingDetails//ct:Amount", xmlNamespaceManager).InnerText = totalticketprice.ToString();

            orderCreateRequestDocument.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:TotalOfferPriceAmount", xmlNamespaceManager).InnerText = totalseatPrice.ToString();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequestDocument.OuterXml).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//ct:Order//ct:OrderID", xmlNamespaceManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", xmlNamespaceManager).InnerText;

            string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

            XmlDocument updatedSeatResponse = new XmlDocument();
            updatedSeatResponse.LoadXml(seatAvailabilityResult1);

            int seatMap = updatedSeatResponse.SelectNodes("//ct:SeatMap", xmlNamespaceManager).Count;

            Assert.True(seatMap == 1, $"Only SEG2 seat map is expected{seatAvailabilityResult1}");

            softAssert.True(SeatAvailabilityValidator.ValidateAssignedSeatWarning(seatAvailabilityResult1, out result), "seat warning Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult1, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "CPDVisa");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml).EnsureIsSuccessStatusCode();

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            HttpResponseMessage orderRetrieveResponse = null;

            dynamic orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            string orderRetrieveResult = "";

            RetryHelper.RunWithRetry(() =>
            {
                orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }, 2);

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

            softAssert.True(OrderChangeValidators.ValidateOrderChangeSeatTicketDocument(orderChangeSeatRequest, orderRetrieveResult, out result, orderCreateRequestDocument.OuterXml), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_PaidSeatPrimeSaleAndFreeSeatPostSale()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 100;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, LocationType.JNB, LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().Result;
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().Result;
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().Result;

            XmlDocument seatresponsedocument = new XmlDocument();

            seatresponsedocument.LoadXml(seatAvailabilityResult);

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Paid, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            string offerID = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[1]//def:OfferItemRefID", orderCreateRequestXmlnsManager).InnerText;

            string totalticketprice = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[1]//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            XmlNode[] seatOfferItems = seatresponsedocument.SelectNodes("//def:ALaCarteOffer//def:OfferItem", orderCreateRequestXmlnsManager).OfType<XmlNode>().ToArray();

            string actualseattotalprice = "";

            string totalprice = "";

            foreach (XmlElement element in seatOfferItems)
            {
                string seatOfferItem = element["OfferItemID"].InnerText;

                if (seatOfferItem.Equals(offerID))
                {
                    string seg1seatprice = element.SelectSingleNode("//def:UnitPrice//def:TotalAmount", orderCreateRequestXmlnsManager).InnerText;

                    actualseattotalprice = Convert.ToString((Convert.ToDouble(seg1seatprice)) * 3);

                    totalprice = Convert.ToString(Math.Round(Convert.ToDouble(actualseattotalprice) + (Convert.ToDouble(totalticketprice)), 2));

                    break;
                }
            }

            updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText = actualseattotalprice;

            updatedOrderCreateRequest.SelectSingleNode($"//def:PaymentFunctions//def:PaymentProcessingDetails//def:Amount", orderCreateRequestXmlnsManager).InnerText = totalprice;

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[6]", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[5]", orderCreateRequestXmlnsManager);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            XmlNode nodeToRemove2 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[4]", orderCreateRequestXmlnsManager);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

            Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode, $"Success status code was not returned from Seat Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {seatAvailabilityRequest1}");

            string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult1);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult1, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

            int seatMap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.True(seatMap == 1, $"Only SEG2 seat map is expected");

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult1, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "CPDVisa");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNode nodeToRemove4 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove4.ParentNode.RemoveChild(nodeToRemove4);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            Assert.True(orderChangeSeatResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeSeatResponse.StatusCode} with message: {orderChangeSeatResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeSeatRequest}");

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();


        }
    }
}
