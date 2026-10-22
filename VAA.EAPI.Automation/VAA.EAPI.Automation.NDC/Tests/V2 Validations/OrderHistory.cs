using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    public class OrderHistory : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderHistory(MemoryStoreFixture memoryStoreFixture)
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
        [Trait("Category", Traits.NDCOrderHistory)]
        public void OrderHistoryRequestValidation()
        {
            Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(180, 120, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.MIA, bookingType: "InstantPurchase", paymentType: "CPDVisa");

            string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

            string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

            string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

            string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

            string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedXmlRQ = new XmlDocument();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlDocument xmlRsDoc = new XmlDocument();

            //Incorrect  order ID validation
            string orderHistoryRequest = RequestHelper.GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRequest);

            string orderHistoryPaxResult = orderHistoryResponse.Content.ReadAsStringAsync().Result;

            xmlRsDoc.LoadXml(orderHistoryPaxResult);

            softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "129","PNR/OrderID match found", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

    }
}
