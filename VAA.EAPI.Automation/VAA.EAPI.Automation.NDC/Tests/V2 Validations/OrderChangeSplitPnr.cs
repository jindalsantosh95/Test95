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
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    public class OrderChangeSplitPnr : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderChangeSplitPnr(MemoryStoreFixture memoryStoreFixture)
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
        public void OrderChangePnrSplit()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedXmlRQ = new XmlDocument();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();
                XmlDocument airShoppingDoc = new XmlDocument();

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                airShoppingDoc.LoadXml(airShoppingResult);

                XmlNodeList paxNodes = airShoppingDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                List<string> parentPnrPax = new List<string>();

                string[] splitPax = {"ADULT_1"};

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

                softAssert.True(OrderCreateAssertions.ValidateChildPnrsInParentRetrieveRS(orderRetrieveParentPnrResult, 1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();
                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeSplitPnr)]
        public void OrderChangeSplitPnrRequestValidations()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

                #region split pnr only child
                string orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["CHILD_1"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                HttpResponseMessage orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                string orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out ValidationResultHelper result, code: "76H", desc: "Divide is not allowed for PNRs where child or young adult is selected without an accompanying adult", tagText: "Unable to process requested option", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                #region split pnr with Adult2 and Infant1
                orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["ADULT_2", "INFANT_1"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out result, code: "76H", desc: "Divide not allowed: infant linked to wrong adult", tagText: "Unable to process requested option", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                #region split pnr without Adult
                orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["CHILD_1", "INFANT_1", "YOUNG_1"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out result, code: "76H", desc: "Divide is not allowed for PNRs where child or young adult is selected without an accompanying adult", tagText: "Unable to process requested option", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                #region split pnr Adults Young Adult and Infant
                orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["ADULT_1", "ADULT_2", "CHILD_1", "INFANT_1"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out result, code: "76H", desc: "Invalid divide request: Atleast one adult should be associated in original PNR", tagText: "Unable to process requested option", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                #region split pnr only Young Adult
                orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["YOUNG_1"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out result, code: "76H", desc: "Divide is not allowed for PNRs where child or young adult is selected without an accompanying adult", tagText: "Unable to process requested option", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                #region split pnr only infant
                orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["INFANT_1"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out result, code: "76H", desc: "Divide is not allowed for PNRs where child or young adult or Infant is selected without an accompanying adult", tagText: "Unable to process requested option", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderChangeSplitPnr)]
        [InlineData(1, 150, 50, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, null, "Direct", null, "CPDVisa", new string[] { "ADULT_1"},SeatPriceType.None,0)]
        [InlineData(2, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, null, "Direct", null, "CPDMaster", new string[] { "ADULT_2", "YOUNG_1", "CHILD_1" },SeatPriceType.None, 0)]
        [InlineData(3, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.SEA, LocationType.SEA, LocationType.LHR, LocationType.None, LocationType.None, null, "Direct", null, "CPDVisa", new string[] { "ADULT_2", "CHILD_1" }, SeatPriceType.Paid, 1)]
        public void OrderChangeSplitPnrResponseValidations(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string corporate, string agentType, string participatingOrgOrgID2, string payment, string[] splitPax,SeatPriceType seatType,int serviceCount)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, corporate: corporate, bookingType: "InstantPurchase", agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, paymentType: payment,seatType: seatType, serviceCount: serviceCount, agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

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

                softAssert.True(OrderCreateAssertions.ValidateChildPnrsInParentRetrieveRS(orderRetrieveParentPnrResult, 1, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeSplitPnr)]
        public void OrderChangeSplitMulipleTimes()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 3, numberOfchildren:1,numberOfyouths:1, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

                //Split the parent pnr for 2nd time

                splitPax = new[] { "ADULT_2"};

                childPnrPax = splitPax.Select(x => x.ToString()).ToList();

                orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, childPnrPax);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml).EnsureIsSuccessStatusCode();

                orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

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

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                airShoppingDoc.LoadXml(airShoppingResult);

                paxNodes = airShoppingDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                childPnrPax = splitPax.Select(x => x.ToString()).ToList();

                removeChildPnrPaxNodes = orderChangeParentPnrRSDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                removeParentOrderNode = orderChangeChildPnrRSDoc.SelectSingleNode("//ct:Order[1]", xmlNamespaceManager);

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
                removeParentPnrPaxNodes = orderChangeChildPnrRSDoc.SelectNodes("//ct:DataLists//ct:PaxList//ct:Pax", xmlNamespaceManager);

                removeChildOrderNode = orderChangeParentPnrRSDoc.SelectSingleNode("//ct:Order[2]", xmlNamespaceManager);

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
                orderRetrieveParentPnrRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderChangeParentPnrRSDoc.OuterXml);

                orderRetrieveParentPnrResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveParentPnrRequest).EnsureIsSuccessStatusCode();

                orderRetrieveParentPnrResult = orderRetrieveParentPnrResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //retrieve child pnr
                orderRetrieveChildPnrRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderChangeChildPnrRSDoc.OuterXml);

                orderRetrieveChildPnrResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveChildPnrRequest).EnsureIsSuccessStatusCode();

                orderRetrieveChildPnrResult = orderRetrieveChildPnrResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderChangeChildPnrRSDoc.OuterXml, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeParentPnrRSDoc.OuterXml, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

                softAssert.True(OrderCreateAssertions.ValidateChildPnrsInParentRetrieveRS(orderRetrieveParentPnrResult, 2, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }


        [Fact]
        [Trait("Category", Traits.NDCOrderChangeSplitPnr)]
        public void OrderChangeSplitPnrTidsAgent()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdKayakUsa, numberOfadults: 2, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "Cash", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

                #region split pnr only child
                string orderChangeSplitPnrRQ = GenerateOrderChangeSplitPnrRequest(airShoppingRequest, orderCreateResult, ["ADULT_2"]);

                updatedXmlRQ.LoadXml(orderChangeSplitPnrRQ);

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                HttpResponseMessage orderChangeSplitPnrResponse = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                string orderChangeSplitPnrResult = orderChangeSplitPnrResponse.Content.ReadAsStringAsync().Result;

                xmlRsDoc.LoadXml(orderChangeSplitPnrResult);

                softAssert.True(OrderChangeValidators.ValidateSplitPNRErrorRS(orderChangeSplitPnrResult, out ValidationResultHelper result, code: "318", desc: "Provide Contact details - Add at least one ContactInfo record to proceed with the child Order", tagText: "Request Validation Error", typeCode: "NotProcessed"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                #endregion

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

    }
}
