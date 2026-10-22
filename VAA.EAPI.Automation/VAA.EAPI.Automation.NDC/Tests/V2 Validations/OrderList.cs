using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    public class OrderList : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderList(MemoryStoreFixture memoryStoreFixture)
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
        public void OrderListRequestvalidations()
        {
            RetryHelper.RunWithRetry(() =>
            {
                //creation date time limit filter more than 180 days
                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

                string orderListRQ = RequestHelper.GenerateOrderListRequest(airShoppingRequest, creationDateTimeFrom: "2025-09-17T14:30:47Z", creationDateTimeTo: "2027-10-17T14:30:47Z");

                XmlDocument updatedXmlRQ = new XmlDocument();

                updatedXmlRQ.LoadXml(orderListRQ);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                HttpResponseMessage orderListResponse = NDCTransformationEndpoints.OrderList(updatedXmlRQ.OuterXml);

                string orderListResult = orderListResponse.Content.ReadAsStringAsync().Result;

                XmlDocument xmlRsDoc = new XmlDocument();

                xmlRsDoc.LoadXml(orderListResult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "912", "Invalid date range in search filter. Please select a range within 180 days", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //past date in TravelDateFilterCriteria
                orderListRQ = RequestHelper.GenerateOrderListRequest(airShoppingRequest, travelEffectiveDate: "2025-09-17");

                updatedXmlRQ = new XmlDocument();

                updatedXmlRQ.LoadXml(orderListRQ);

                orderListResponse = NDCTransformationEndpoints.OrderList(updatedXmlRQ.OuterXml);

                orderListResult = orderListResponse.Content.ReadAsStringAsync().Result;

                updatedXmlRQ = new XmlDocument();

                xmlRsDoc.LoadXml(orderListResult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "912", "The selected flight date is in the past. Please choose a valid upcoming date", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
               
                //filter criteria with payment status
                orderListRQ = RequestHelper.GenerateOrderListRequest(airShoppingRequest, travelEffectiveDate: "2025-10-17", paymentStatusCode: "paid");

                updatedXmlRQ = new XmlDocument();

                updatedXmlRQ.LoadXml(orderListRQ);

                orderListResponse = NDCTransformationEndpoints.OrderList(updatedXmlRQ.OuterXml);

                orderListResult = orderListResponse.Content.ReadAsStringAsync().Result;

                updatedXmlRQ = new XmlDocument();

                xmlRsDoc.LoadXml(orderListResult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "912", "Invalid search filter: Use 'CreationDateTime' filter along with payment filter criteria", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }
    }
}
