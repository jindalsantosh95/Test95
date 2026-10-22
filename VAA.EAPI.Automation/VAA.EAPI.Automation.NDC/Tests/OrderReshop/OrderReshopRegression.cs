using System.Globalization;
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
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OrderReshop
{
    public class OrderReshopRegression : IClassFixture<MemoryStoreFixture>
    {


        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderReshopRegression(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSwedishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSwedishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCarrebianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCarrebianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", "OrderReshopRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.OneWay, LocationType.LHR, LocationType.IAD, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        public void OrderReshopOneWay(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "OrderReshopRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.TLV, LocationType.TLV, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(5, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.SFO, LocationType.SFO, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        public void OrderReshopReturnCompleteJourneyChange(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.validateExistingOrderItems(orderCreateResult,orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "OrderReshopRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "UntokenizedAmex", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.MCO, LocationType.MCO, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.BOM, LocationType.ATL, LocationType.ATL, LocationType.BOM, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        public void OrderReshopReturnOutBoundJourneyChange(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.validateExistingOrderItems(orderCreateResult, orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "OrderReshopRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDMaster", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(2, 60, 180, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.EWR, LocationType.EWR, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        public void OrderReshopReturnInBoundJourneyChange(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "OrderReshopRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.TLV, LocationType.TLV, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 50, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDMaster", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(4, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOM, LocationType.BOM, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "CPDMaster", ChangeOrder.OutBoundJourney, "Direct", null)]
        [InlineData(6, 100, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "CPDVisa", ChangeOrder.InBoundJourney, "Direct", null)]
        public void OrderReshopPNRWithSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "OrderReshopRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        [InlineData(3, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, SeatPriceType.None, null, "CPDMaster", ChangeOrder.CompleteJourney, "Direct", null)]
        public void OrderReshopMulticity(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string paymentType, ChangeOrder changeOrder, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, changeOrder: changeOrder, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void OrderReshopE2E()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(100, 200, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.ATL,locationThree:LocationType.ATL,locationFour:LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderReshopRS);

                string orderCreateResult = result1["orderCreateResult"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string orderReshopResult = result1["orderReshopRS"];

                string orderReshopRequest = result1["orderReshopRQ"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.validateExistingOrderItems(orderCreateResult, orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 2);

        }

        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void ReshopRegression_CabinUpgradeEconomyToPremiumTC21()
        {
            RetryHelper.RunWithRetry(() =>
            {
                SoftAssertHelper softAssert = new SoftAssertHelper();

                string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 100;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                    JourneyType.Return,
                    locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, deptDate: journey1Date,
                    cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

                XmlDocument updatedAirShoppingRequest = new XmlDocument();

                XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
                airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedAirShoppingRequest.LoadXml(airShoppingRequest);

                string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

                string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

                //Post Air Shopping request
                HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

                Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

                //Getting the Air Shopping response and converting to XML Document
                string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
                airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

                //Adding namespace manager
                XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
                airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                //Find offer based on Cabin Type and Flight Scenraio
                Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

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
                string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
                offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

                //Adding namespace manager
                XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
                offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");


                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa");

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

                //Order Retrieve 
                dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

                //Getting the Order Retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                //Adding namespace manager
                XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
                orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

                int countOrderItemID = offerPriceResponseXmlDocument.SelectNodes("//def:Pax", offerPriceResponseXmlnsManager).Count;

                int offerCount = 0;
                int counter = 0;
                string orderReshopRequest = "";
                string orderReshopResult = "";
                IDictionary<string, string> reshopOfferDetails = new Dictionary<string, string>();

                XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
                do
                {

                    DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter);

                    string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");

                    DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate1 = date1.AddDays(counter);

                    string reshopJourney2Date = newDate1.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request

                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                        JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, newCabinTypeCodeOne: "4", newCabinTypeCodeTwo: "4", deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);

                    //Post Order Reshop request
                    HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                    //Getting the Order Reshop response and converting to XML Document
                    orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                    counter++;
                    offerCount = orderReshopResponseXmlDocument.SelectNodes("//def:ReshopResults//def:ReshopOffers//def:Offer", orderRetrieveResponseXmlnsManager).Count;

                    if (offerCount > 0)
                    {
                        break;
                    }
                } while (counter < 6);

                string reshopOfferRefId = "";
                string reshopTotalAmount = "";

                Random rnd = new Random();

                if (offerCount > 0)
                {
                    reshopOfferRefId = orderReshopResponseXmlDocument.SelectSingleNode($"//def:ReshopResults//def:ReshopOffers//def:Offer[{rnd.Next(1, offerCount)}]//def:OfferID", orderRetrieveResponseXmlnsManager).InnerText;
                    reshopTotalAmount = orderReshopResponseXmlDocument.SelectSingleNode($"//def:ReshopResults//def:ReshopOffers//def:Offer[{rnd.Next(1, offerCount)}]//def:TotalPrice//def:TotalAmount", orderRetrieveResponseXmlnsManager).InnerText;
                }

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void ReshopRegression_CabinUpgradePremiumToUpperTC22()
        {
            RetryHelper.RunWithRetry(() =>
            {
                SoftAssertHelper softAssert = new SoftAssertHelper();

                string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 60;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                    JourneyType.Return,
                    locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR, deptDate: journey1Date,
                    cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

                XmlDocument updatedAirShoppingRequest = new XmlDocument();

                XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
                airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedAirShoppingRequest.LoadXml(airShoppingRequest);

                string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

                string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

                //Post Air Shopping request
                HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

                Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

                //Getting the Air Shopping response and converting to XML Document
                string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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
                string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
                offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

                //Adding namespace manager
                XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
                offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");


                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa");

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

                //Order Retrieve 
                dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

                //Getting the Order Retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                //Adding namespace manager
                XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
                orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;
                int countOrderItemID = offerPriceResponseXmlDocument.SelectNodes("//def:Pax", offerPriceResponseXmlnsManager).Count;

                int offerCount = 0;

                int counter = 0;
                string orderReshopRequest = "";
                string orderReshopResult = "";
                IDictionary<string, string> reshopOfferDetails = new Dictionary<string, string>();

                XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

                do
                {

                    DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter);

                    string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");

                    DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    DateTime newDate1 = date1.AddDays(counter);

                    string reshopJourney2Date = newDate1.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request

                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                        JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR, newCabinTypeCodeOne: "2", newCabinTypeCodeTwo: "2", deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);

                    //Post Order Reshop request
                    HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                    //Getting the Order Reshop response and converting to XML Document
                    orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                    counter++;

                    offerCount = orderReshopResponseXmlDocument.SelectNodes("//def:ReshopResults//def:ReshopOffers//def:Offer", orderRetrieveResponseXmlnsManager).Count;

                    if (offerCount > 0)
                    {
                        break;
                    }

                } while (counter < 6);

                string reshopOfferRefId = "";
                string reshopTotalAmount = "";

                Random rnd = new Random();

                if (offerCount > 0)
                {
                    reshopOfferRefId = orderReshopResponseXmlDocument.SelectSingleNode($"//def:ReshopResults//def:ReshopOffers//def:Offer[{rnd.Next(1, offerCount)}]//def:OfferID", orderRetrieveResponseXmlnsManager).InnerText;
                    reshopTotalAmount = orderReshopResponseXmlDocument.SelectSingleNode($"//def:ReshopResults//def:ReshopOffers//def:Offer[{rnd.Next(1, offerCount)}]//def:TotalPrice//def:TotalAmount", orderRetrieveResponseXmlnsManager).InnerText;
                }

                softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }
        
        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void Reshop_OrdinalValueZero()
        {
            XmlDocument xmlDocument_1 = new XmlDocument();

            string request = string.Format(System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml"));

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = new XmlNamespaceManager(updatedRequest.NameTable);
            responseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedRequest.SelectSingleNode("//def:Ordinal", responseXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

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
