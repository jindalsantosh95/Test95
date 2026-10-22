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
using VAA.EAPI.Automation.NDC.Tests.AirShopping;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class SeatAvailabilityRegression : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public SeatAvailabilityRegression(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Trait("Category", "SeatPrimeSaleV2")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 2, 2, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, LocationType.None, LocationType.None, "Direct", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 0, 1, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.EWR, LocationType.EWR, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 2, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(5, 50, 150, AgentID.ParticipatingOrgIdKayakUsa, 1, 0, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.SFO, LocationType.SFO, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(6, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, "Direct", null)]
        [InlineData(7, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(8, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 2, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(9, 240, 60, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(10, 240, 60, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.AUS, LocationType.AUS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(11, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(12, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.BOM, LocationType.ATL, LocationType.ATL, LocationType.BOM, LocationType.None, LocationType.None, "Direct", null)]
        public void SeatPrimeSale(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: SeatPriceType.Paid, apiResult: ApiResult.SeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["seatAvailabilityResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(AirshoppingValidator.ValidateNullValues(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCSanityV2)]
        [InlineData(1, 120, 80, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.MIA, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(2, 170, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(3, 40, 180, AgentID.ParticipatingOrgIdKayakUsa, 1, 0, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.JFK, LocationType.LHR, "Direct", null)]
        [InlineData(4, 90, 120, AgentID.ParticipatingOrgIdUSAgent, 1, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(5, 70, 65, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        public void SeatAvailabilityPrimeSale(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, FlightType operatorName = FlightType.VS)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: SeatPriceType.Paid, apiResult: ApiResult.SeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, operatorName: operatorName);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["seatAvailabilityResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(AirshoppingValidator.ValidateNullValues(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "SeatPrimeSaleV2")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.NonVirginRequestedOperator, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 2, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", AgentID.ParticipatingOrgIdTravelFusionAggregator, FlightType.DL)]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 2, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.NonVirginRequestedWithOtherOperators, JourneyType.Return, LocationType.LHR, LocationType.CUN, LocationType.CUN, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.NonVirginRequestedOperator, JourneyType.OneWay, LocationType.LHR, LocationType.YYZ, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 2, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.CUN, LocationType.CUN, LocationType.LHR, LocationType.None, LocationType.None, "Direct", AgentID.ParticipatingOrgIdTravelFusionAggregator, FlightType.DL)]
        [InlineData(6, 100, 200, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(7, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.AUS, LocationType.AUS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(8, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.MEX, LocationType.MEX, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        public void SeatPrimeSaleDLTests(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, FlightType operatorName = FlightType.VS)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, serviceCount: 0, seatType: SeatPriceType.Paid, apiResult: ApiResult.SeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, operatorName: operatorName);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["seatAvailabilityResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(AirshoppingValidator.ValidateNullValues(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "SeatPrimeSaleV2")]
        [Trait("Category", Traits.NDCSanityV2)]
        [Trait("Category", "SeatAvailabilityAllRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.OneWay, LocationType.LHR, LocationType.YYZ, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, FlightType.AF)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, FlightType.KL)]
        public async Task SeatPrimeSaleNonVsAndDL(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, FlightType operatorName = FlightType.VS)
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, serviceCount: 0, seatType: SeatPriceType.Paid, apiResult: ApiResult.SeatAvailabilityRQ, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, operatorName: operatorName);

            string seatAvailabilityRQ = result1["seatAvailabilityRQ"];

            string offerPriceResult = result1["offerPriceResult"];

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRQ);

            string seatAvailabilityResult = await seatAvailabilityResponse.Content.ReadAsStringAsync();

            Assert.True(SeatAvailabilityValidator.ValidateNonVSAndDlErrorScenario(seatAvailabilityResult, offerPriceResult, out ValidationResultHelper result), "Validation failed . " + test + " Reasons: " + string.Join(", ", result.Message));

        }

        [Theory]
        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        public void SeatPostSaleOneWay(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: SeatPriceType.Paid, apiResult: ApiResult.PostSaleSeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 3, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        public void SeatPostSaleReturnTrip(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: SeatPriceType.Paid, apiResult: ApiResult.PostSaleSeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.YYZ, LocationType.YYZ, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 1, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.YYZ, LocationType.YYZ, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 1, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster")]
        public void SeatPostSaleCodeShareFlights(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: SeatPriceType.Paid, apiResult: ApiResult.PostSaleSeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["postSaleSeatAvailabilityRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "Post"), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }


        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_SingleSeatMapWithChild()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "00002222",
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 2);

                //Post Air Shopping request
                HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

                //Getting the Air Shopping response and converting to XML Document
                string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
                airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

                //Adding namespace manager
                XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
                airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                //Find offer based on Cabin Type and Flight Scenraio
                Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

                Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

                string offerRefId = offerDetails["OfferRefID"];

                //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
                string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

                //Post Offer Price request
                HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

                //Getting the Offer Price response and converting to XML Document
                string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
                offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

                //Adding namespace manager
                XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
                offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(AirshoppingValidator.ValidateNullValues(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_SeatPurchasePostOrderCreateIncorrectOrderIDTC19()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyLight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: "VS932GY6RHB22");

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            XmlNodeList code = updatedSeatRequest.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("129"), $"Error code expected is 129 but actual is {code}");

            XmlNodeList descText = updatedSeatRequest.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("No Order ID match found"), $"Error code expected is No Order ID match found but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_SeatPurchasePostOrderCreateNonVSFlightTC20()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS,
                cabinTypeCodeOne: "2", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.NonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            XmlNodeList code = updatedSeatRequest.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("219"), $"Error code expected is 129 but actual is {code}");

            XmlNodeList descText = updatedSeatRequest.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Code Share Carrier Flight Restricted"), $"Error code expected is No Order ID match found but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_SeatPurchasePostOrderCreateIncorrectAgentIDTC21()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyLight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityRequest);

            updatedSeatRequest.SelectSingleNode("//def:DistributionChainLink//def:ParticipatingOrg//def:OrgID", updatedSeatRequestXmlnsManager).InnerText = "00003333";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatRequest.OuterXml);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatResponseXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
            updatedSeatResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            XmlNodeList code = updatedSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("368"), $"Error code expected is 368 but actual is {code}");

            XmlNodeList descText = updatedSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("The requested Seller is not authorised to access the service "), $"Error code expected is Seller's subscription key is not valid but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_FreeSeatOneSegmentReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(260).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 50;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.Return,
                   locationOne: LocationType.LHR, locationTwo: LocationType.BOS, LocationType.BOS, LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

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

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode,
                    $"Success status code was not returned from Seat Availability Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse1.ReadRequestAsString()}");

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult1, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                int seatMap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

                Assert.True(seatMap == 1, $"Only SEG2 seat map is expected");

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_1PaxWithFreeSeatOneSegment()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 60;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                  JourneyType.OneWay,
                   locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

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

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode,
                    $"Success status code was not returned from Seat Availability Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse1.ReadRequestAsString()}");

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                Assert.False(eligiblePax.Contains("ADULT_1"), $"ADULT_1 is not expected in eligibility list");

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_FreeSeatReturn()
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
                Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.VirginAndNonVirgin);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

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

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode,
                    $"Success status code was not returned from Seat Availability Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse1.ReadRequestAsString()}");

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

                    Assert.False(paxRefID.Contains("ADULT") && paxSegmentRefID.Equals("SEG1"), $"Seat is already booked for ADULT_1 first segment");

                    Assert.False(paxRefID.Contains("YOUNG") && paxSegmentRefID.Equals("SEG2"), $"Seat is already booked for YOUNG_1 second segment");

                    Assert.False(paxRefID.Contains("CHILD") && paxSegmentRefID.Equals("SEG2"), $"Seat is already booked for CHILD_1 second segment");

                }

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result, sale: "post"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_FreeSeatForAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 200;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.Return,
                   locationOne: LocationType.LHR, locationTwo: LocationType.DEL, LocationType.DEL, LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                XmlNodeList code = updatedSeatResponse.GetElementsByTagName("Code");

                XmlNodeList descText = updatedSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to return seat map - All passengers have already reserved seats"), $"descText is not as expected");

                Assert.True(code[0].InnerText.Equals("197"), $"code is not as expected");

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_1AdultWithFreeSeatAndInfant()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 60;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.OneWay,
                   locationOne: LocationType.LHR, locationTwo: LocationType.BOS, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfinfants: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Free, seatAvailabilityResult);

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                XmlNodeList code = updatedSeatResponse.GetElementsByTagName("Code");

                XmlNodeList descText = updatedSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to return seat map - All passengers have already reserved seats"), $"descText is not as expected");

                Assert.True(code[0].InnerText.Equals("197"), $"code is not as expected");

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_PaidSeatOneSegmentReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 100;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                   locationOne: LocationType.LHR, locationTwo: LocationType.JFK, LocationType.JFK, LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                // Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode,
                    $"Success status code was not returned from Seat Availability Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse1.ReadRequestAsString()}");

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult1, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                int seatMap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

                Assert.True(seatMap == 1, $"Only SEG2 seat map is expected");

                //Seat availability assertions

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_PaidSeatReturn()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 200;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.Return,
                   locationOne: LocationType.LHR, locationTwo: LocationType.BGI, LocationType.BGI, LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Paid, seatAvailabilityResult);

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                string offerID = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[1]//def:OfferItemRefID", orderCreateRequestXmlnsManager).InnerText;

                string offerID1 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[6]//def:OfferItemRefID", orderCreateRequestXmlnsManager).InnerText;

                string totalticketprice = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[1]//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

                XmlNode[] seatOfferItems = seatresponsedocument.SelectNodes("//def:ALaCarteOffer//def:OfferItem", orderCreateRequestXmlnsManager).OfType<XmlNode>().ToArray();

                string actualseattotalprice = "";

                Double actualseattotalprice1 = 0;

                Double totalprice = 0;

                foreach (XmlElement element in seatOfferItems)
                {
                    string seatOfferItem = element["OfferItemID"].InnerText;

                    if (seatOfferItem.Equals(offerID))
                    {
                        string seg1seatprice = element.SelectSingleNode("//def:OfferItem//def:UnitPrice//def:TotalAmount", orderCreateRequestXmlnsManager).InnerText;

                        actualseattotalprice = Convert.ToString((Convert.ToDouble(seg1seatprice)));

                        totalprice = Math.Round(Convert.ToDouble(totalticketprice) + (Convert.ToDouble(actualseattotalprice)), 2);

                        actualseattotalprice1 = actualseattotalprice1 + Convert.ToDouble(seg1seatprice);
                    }

                    if (seatOfferItem.Equals(offerID1))
                    {
                        string seg2seatprice = element["UnitPrice"].FirstChild.InnerText;

                        actualseattotalprice = Convert.ToString((Convert.ToDouble(seg2seatprice) * 2));

                        totalprice = totalprice + Convert.ToDouble(actualseattotalprice);

                        actualseattotalprice1 = Math.Round(actualseattotalprice1 + (Convert.ToDouble(seg2seatprice) * 2), 2);

                    }
                }

                updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText = Convert.ToString(actualseattotalprice1);

                updatedOrderCreateRequest.SelectSingleNode($"//def:PaymentFunctions//def:PaymentProcessingDetails//def:Amount", orderCreateRequestXmlnsManager).InnerText = Convert.ToString(totalprice);

                XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);
                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

                XmlNode nodeToRemove1 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);
                nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

                XmlNode nodeToRemove2 = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]", orderCreateRequestXmlnsManager);
                nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode,
                    $"Success status code was not returned from Seat Availability Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse1.ReadRequestAsString()}");

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult1, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                int seatMap = updatedSeatResponse.SelectNodes("//def:ALaCarteOffer//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).Count;

                XmlNode[] offerItem = updatedSeatResponse.SelectNodes("//def:ALaCarteOffer//def:OfferItem", updatedSeatRequestXmlnsManager).OfType<XmlNode>().ToArray();

                foreach (XmlElement element in offerItem)
                {
                    string paxSegmentRefID = element.SelectSingleNode("//def:OfferItem//def:Eligibility//def:OfferFlightAssociations//def:PaxSegmentReferences//def:PaxSegmentRefID", updatedSeatRequestXmlnsManager).InnerText;

                    string paxRefID = element.SelectSingleNode("//def:OfferItem//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                    Assert.False(paxRefID.Contains("ADULT") && paxSegmentRefID.Equals("SEG1"), $"Seat is already booked for ADULT_1 first segment");

                    Assert.False(paxRefID.Contains("YOUNG") && paxSegmentRefID.Equals("SEG2"), $"Seat is already booked for YOUNG_1 second segment");

                    Assert.False(paxRefID.Contains("CHILD") && paxSegmentRefID.Equals("SEG2"), $"Seat is already booked for CHILD_1 second segment");

                }

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_1PaxWithPaidSeatOneSegment()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(260).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 50;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.OneWay,
                   locationOne: LocationType.LHR, locationTwo: LocationType.TLV, cabinTypeCodeOne: "5", numberOfadults: 2, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                        actualseattotalprice = Convert.ToString((Convert.ToDouble(seg1seatprice)));

                        totalprice = Convert.ToString(Math.Round(Convert.ToDouble(actualseattotalprice) + (Convert.ToDouble(totalticketprice)), 2));

                        break;
                    }
                }

                updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText = actualseattotalprice;

                updatedOrderCreateRequest.SelectSingleNode($"//def:PaymentFunctions//def:PaymentProcessingDetails//def:Amount", orderCreateRequestXmlnsManager).InnerText = totalprice;

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

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                Assert.True(seatAvailabilityResponse1.IsSuccessStatusCode,
                    $"Success status code was not returned from Seat Availability Response, returned: {seatAvailabilityResponse1.StatusCode} with message: {seatAvailabilityResponse1.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse1.ReadRequestAsString()}");

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult1, out string[] seatAvailabilityResultExceptions1), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string eligiblePax = updatedSeatResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem[1]//def:Eligibility", updatedSeatRequestXmlnsManager).InnerText;

                int seatMap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

                Assert.True(seatMap == 1, $"Only SEG2 seat map is expected");

                Assert.False(eligiblePax.Equals("ADULT_1"), $"Only SEG2 seat map is expected");

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_1AdultWithPaidSeatAndInfant()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 60;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                    JourneyType.OneWay,
                   locationOne: LocationType.LHR, locationTwo: LocationType.ATL, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfinfants: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                XmlNodeList code = updatedSeatResponse.GetElementsByTagName("Code");

                XmlNodeList descText = updatedSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to return seat map - All passengers have already reserved seats"), $"descText is not as expected");

                Assert.True(code[0].InnerText.Equals("197"), $"code is not as expected");

                Assert.True(true);
            }, 2);
        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_PaidSeatForAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

                int maxNumberOfDays = 200;

                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                   JourneyType.Return,
                   locationOne: LocationType.LHR, locationTwo: LocationType.JNB, LocationType.JNB, LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

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

                SoftAssertHelper softAssert = new SoftAssertHelper();
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Paid, seatAvailabilityResult);

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

                Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

                Assert.True(warning.Count == 0, $"seats are not booked");

                string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

                string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

                HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

                string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

                XmlDocument updatedSeatResponse = new XmlDocument();

                XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
                updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedSeatResponse.LoadXml(seatAvailabilityResult1);

                XmlNodeList code = updatedSeatResponse.GetElementsByTagName("Code");

                XmlNodeList descText = updatedSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to return seat map - All passengers have already reserved seats"), $"descText is not as expected");

                Assert.True(code[0].InnerText.Equals("197"), $"code is not as expected");

                Assert.True(true);
            }, 2);

        }

        [Trait("Category", "SeatAvailabilityPostSaleRegression")]
        [Fact]
        public void OrderCreate_PSP_PaidSeatForAllPaxVSNonVS()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, LocationType.ATL, LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date, flighttype: "Direct");

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

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

            SoftAssertHelper softAssert = new SoftAssertHelper();
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Seat availability assertions
            softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", RequestHelper.SeatPriceType.Paid, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().Result;

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreate.OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest1 = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse1 = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest1);

            string seatAvailabilityResult1 = seatAvailabilityResponse1.Content.ReadAsStringAsync().Result;

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult1);

            XmlNodeList code = updatedSeatResponse.GetElementsByTagName("Code");

            XmlNodeList descText = updatedSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Unable to return seat map - All passengers have already reserved seats"), $"descText is not as expected");

            Assert.True(code[0].InnerText.Equals("197"), $"code is not as expected");

        }

        public void SeatAvailability_OrdinalErrorValidation_ValueNull()
        {
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "35200056",
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR, cabinTypeCodeOne: "2", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode,
                $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " +
                $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode,
                $"Success status code was not returned from Offer Price Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()} \r\n " +
                $"Offer Price Request sent was {offerPriceResponse.ReadRequestAsString()}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            XmlDocument updatedseatAvailabilityRequest = new XmlDocument();
            updatedseatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager seatAvailabilityRequestXmlnsManager1 = new XmlNamespaceManager(updatedseatAvailabilityRequest.NameTable);
            seatAvailabilityRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNode nodeToRemove = updatedseatAvailabilityRequest.SelectSingleNode("//def:Ordinal", seatAvailabilityRequestXmlnsManager1);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);
            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument seatAvailabilityResponseXmlDocument = new XmlDocument();
            seatAvailabilityResponseXmlDocument.LoadXml(seatAvailabilityResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            Assert.True(AirshoppingValidator.ValidateOrdinalAirshopping(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact]
        public void SeatAvailability_SeatPurchasePostOrderCreate_OrdinalErrorValidation_ValueBlank()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "35200056",
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyLight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityRequest);

            updatedSeatRequest.SelectSingleNode("//def:Ordinal", updatedSeatRequestXmlnsManager).InnerText = "";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatRequest.OuterXml);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument seatAvailabilityResponseXmlDocument = new XmlDocument();
            seatAvailabilityResponseXmlDocument.LoadXml(seatAvailabilityResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));
            Assert.True(AirshoppingValidator.ValidateOrdinalAirshopping(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact]
        public void SeatAvailability_OrdinalErrorValidation_Value0()
        {
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "35200056",
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR, cabinTypeCodeOne: "2", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode,
                $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " +
                $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode,
                $"Success status code was not returned from Offer Price Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()} \r\n " +
                $"Offer Price Request sent was {offerPriceResponse.ReadRequestAsString()}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            XmlDocument updatedseatAvailabilityRequest = new XmlDocument();
            updatedseatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager seatAvailabilityRequestXmlnsManager1 = new XmlNamespaceManager(updatedseatAvailabilityRequest.NameTable);
            seatAvailabilityRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedseatAvailabilityRequest.SelectSingleNode("//def:Ordinal", seatAvailabilityRequestXmlnsManager1).InnerText = "0";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);
            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument seatAvailabilityResponseXmlDocument = new XmlDocument();
            seatAvailabilityResponseXmlDocument.LoadXml(seatAvailabilityResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));
            Assert.True(AirshoppingValidator.ValidateOrdinalAirshopping(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Fact]
        public void SeatAvailability_SeatPurchasePostOrderCreate_OrdinalErrorValidation_ValueABCD123()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "35200056",
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyLight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityRequest);

            updatedSeatRequest.SelectSingleNode("//def:Ordinal", updatedSeatRequestXmlnsManager).InnerText = "ABCD123";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatRequest.OuterXml);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument seatAvailabilityResponseXmlDocument = new XmlDocument();
            seatAvailabilityResponseXmlDocument.LoadXml(seatAvailabilityResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            Assert.True(AirshoppingValidator.ValidateOrdinalAirshopping(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}
