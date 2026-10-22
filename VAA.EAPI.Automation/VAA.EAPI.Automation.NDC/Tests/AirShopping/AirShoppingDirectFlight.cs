using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingDirectFlight : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public static string participatingOrgOrgIdTravelRepublic = "91281035";

        public AirShoppingDirectFlight(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Theory]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5",JourneyType.OneWay, LocationType.LHR, LocationType.TLV, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 1, 1, 1, 1, "5", JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 2, 2, 1, 1, "5", JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdUSAgent, 1, 0, 1, 1, "4", JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(6, 100, 200, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 2, 1, 0, "2",JourneyType.Return, LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.LHR, LocationType.None, LocationType.None, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        [InlineData(7, 50, 150, AgentID.ParticipatingOrgIdKayakUsa, 1, 0, 1, 1, "2", JourneyType.Return, LocationType.LHR, LocationType.SFO, LocationType.SFO, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(8, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 1, 0, "5", JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, "Direct", null)]
        [InlineData(9, 240, 60, AgentID.ParticipatingOrgIdSkyscannerInDirect, 1, 0, 1, 0, "4", JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, "InDirect", AgentID.ParticipatingOrgIdTravelFusionAggregator)]
        public async Task AirShopping_DirectFlightSearch(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode,  JourneyType journeyType, Core.Helpers.Enums.LocationType locationOne, Core.Helpers.Enums.LocationType locationTwo, Core.Helpers.Enums.LocationType locationThree, Core.Helpers.Enums.LocationType locationFour, Core.Helpers.Enums.LocationType locationFive, Core.Helpers.Enums.LocationType locationSix, string agentType, string participatingOrgOrgID2)
        {
            string deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maximumDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                journeyType,
                locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
                cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date, flighttype: "Direct", agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgID)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateDirectFlightSearch(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " +test+ string.Join(", ", result.Message));
        }
    }
}