using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.NDC.Constants;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public class NDCTestCases
    {
        public static IEnumerable<object[]> GetAirShoppingTestData => AirShoppingTestData();

        private static IEnumerable<object[]> AirShoppingTestData()
        {
            // One Way
            yield return TestCase(1, AgentID.ParticipatingOrgIdTravelRepublic, 4, 2, 0, 2, "5", JourneyType.OneWay, LocationType.LHR, LocationType.AUS);
            yield return TestCase(2, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 3, 1, 0, 0, "4", JourneyType.OneWay, LocationType.LHR, LocationType.KOA);
            yield return TestCase(3, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 1, 1, 1, "2", JourneyType.OneWay, LocationType.LHR, LocationType.LAS);
            yield return TestCase(4, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 2, 1, "5", JourneyType.OneWay, LocationType.LHR, LocationType.BGI);
            yield return TestCase(5, AgentID.ParticipatingOrgIdSkyscannerInDirect, 5, 1, 2, 1, "2", JourneyType.OneWay, LocationType.LHR, LocationType.MIA,
                                  channelType: "InDirect", aggregatorId: AgentID.ParticipatingOrgIdTravelFusionAggregator);
            yield return TestCase(6, AgentID.ParticipatingOrgIdTravelRepublic, 4, 2, 0, 2, "5", JourneyType.OneWay, LocationType.LHR, LocationType.LAX);
            yield return TestCase(32, AgentID.ParticipatingOrgIdBhamasAgent, 1, 0, 1, 0, "4", JourneyType.OneWay, LocationType.LHR, LocationType.SFO);
            yield return TestCase(34, AgentID.ParticipatingOrgIdCarrebianAgent, 1, 1, 0, 0, "4", JourneyType.OneWay, LocationType.LHR, LocationType.BGI);

            // Return
            yield return TestCase(12, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 3, 1, 0, 0, "4", JourneyType.Return, LocationType.LHR, LocationType.KOA, LocationType.KOA, LocationType.LHR);
            yield return TestCase(13, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 1, 1, 1, "2", JourneyType.Return, LocationType.LHR, LocationType.LAS, LocationType.LAS, LocationType.LHR);
            yield return TestCase(14, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 2, 1, "5", JourneyType.Return, LocationType.LHR, LocationType.MIA, LocationType.MIA, LocationType.LHR);
            yield return TestCase(15, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 5, 1, 2, 1, "2", JourneyType.Return, LocationType.LHR, LocationType.MEX, LocationType.MEX, LocationType.LHR);
            yield return TestCase(16, AgentID.ParticipatingOrgIdTravelRepublic, 4, 2, 0, 2, "5", JourneyType.Return, LocationType.LHR, LocationType.LAX, LocationType.LAX, LocationType.LHR);
            yield return TestCase(17, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 3, 1, 0, 0, "4", JourneyType.Return, LocationType.LHR, LocationType.NYC, LocationType.NYC, LocationType.LHR);
            yield return TestCase(18, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 1, 1, 1, "2", JourneyType.Return, LocationType.LHR, LocationType.ICN, LocationType.ICN, LocationType.LHR);
            yield return TestCase(19, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 2, 1, "5", JourneyType.Return, LocationType.LHR, LocationType.SJU, LocationType.SJU, LocationType.LHR);
            yield return TestCase(20, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 5, 1, 2, 1, "2", JourneyType.Return, LocationType.LHR, LocationType.YYZ, LocationType.YYZ, LocationType.LHR);
            yield return TestCase(33, AgentID.ParticipatingOrgIdCanadianAgent, 1, 1, 1, 0, "5", JourneyType.Return, LocationType.LHR, LocationType.YVR, LocationType.YVR, LocationType.LHR);
            yield return TestCase(35, AgentID.ParticipatingOrgIdCaribjetAgent, 2, 2, 1, 1, "5", JourneyType.Return, LocationType.LHR, LocationType.BGI, LocationType.BGI, LocationType.LHR);
            yield return TestCase(36, AgentID.ParticipatingOrgIdEuropeanAgent, 1, 1, 0, 0, "5", JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR);
            yield return TestCase(37, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 1, 1, "5", JourneyType.Return, LocationType.LHR, LocationType.OSL, LocationType.OSL, LocationType.LHR);

            // Multi City
            yield return TestCase(21, AgentID.ParticipatingOrgIdTravelRepublic, 4, 2, 0, 2, "5", JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR);
            yield return TestCase(22, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 3, 1, 0, 0, "4", JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR);
            yield return TestCase(23, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 1, 1, 1, "2", JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR);
            yield return TestCase(24, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 2, 1, "5", JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL);
            yield return TestCase(25, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 5, 1, 2, 1, "2", JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL);
            yield return TestCase(26, AgentID.ParticipatingOrgIdTravelRepublic, 4, 2, 0, 2, "5", JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL);
            yield return TestCase(27, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 3, 1, 0, 0, "4", JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR);
            yield return TestCase(28, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 1, 1, 1, "2", JourneyType.MultiCity, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.IAD, LocationType.IAD, LocationType.LHR);
            yield return TestCase(29, AgentID.ParticipatingOrgIdKayakUsa, 1, 1, 2, 1, "5", JourneyType.MultiCity, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR);
            yield return TestCase(30, AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, 5, 1, 2, 1, "2", JourneyType.MultiCity, LocationType.LHR, LocationType.TLV, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JNB);
        }

        private static object[] TestCase(
            int id,
            string agentId,
            int param1,
            int param2,
            int param3,
            int param4,
            string param5,
            JourneyType journeyType,
            LocationType loc1,
            LocationType loc2,
            LocationType loc3 = LocationType.None,
            LocationType loc4 = LocationType.None,
            LocationType loc5 = LocationType.None,
            LocationType loc6 = LocationType.None,
            string channelType = null,
            string aggregatorId = null)
        {
            var testCase = new List<object>
        {
            id, agentId,
            param1, param2, param3, param4, param5,
            journeyType,
            loc1, loc2, loc3, loc4, loc5, loc6
        };

            if (channelType != null) testCase.Add(channelType);
            if (aggregatorId != null) testCase.Add(aggregatorId);

            return testCase.ToArray();
        }
    }
}