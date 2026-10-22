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
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    public class OrderChangeAmendAPIs : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderChangeAmendAPIs(MemoryStoreFixture memoryStoreFixture)
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
        public void OrderChangeAmendAPIsSinglePax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(195, 100, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.PremiumFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", visa: "visa", passport: "passport", knowntravller: "knowntraveller", redress: "redress", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                string orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress");

                HttpResponseMessage orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(orderChangeAmendAPISRequestAdult).EnsureIsSuccessStatusCode();

                dynamic orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeAmendAPIsResultAdult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.OrderChangeRQValidateAPIs(orderChangeAmendAPISRequestAdult, orderChangeAmendAPIsResultAdult, out result, passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeNameCorrectionAndAmendAPIS)]
        public void OrderChangeAmendAPIsAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(195, 100, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.PremiumFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", visa: "visa", passport: "passport", knowntravller: "knowntraveller", redress: "redress", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                #region amend Adult APIs
                string orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress");

                HttpResponseMessage orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(orderChangeAmendAPISRequestAdult).EnsureIsSuccessStatusCode();

                dynamic orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeAmendAPIsResultAdult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.OrderChangeRQValidateAPIs(orderChangeAmendAPISRequestAdult, orderChangeAmendAPIsResultAdult, out result, passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region amend Child APIs
                string orderChangeAmendAPISRequestChild = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "CHILD_1", "CHD", passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress");

                HttpResponseMessage orderChangeAmendAPIsRSChild = NDCTransformationEndpoints.OrderChange(orderChangeAmendAPISRequestChild).EnsureIsSuccessStatusCode();

                dynamic orderChangeAmendAPIsResult = orderChangeAmendAPIsRSChild.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeAmendAPIsResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.OrderChangeRQValidateAPIs(orderChangeAmendAPISRequestChild, orderChangeAmendAPIsResult, out result, passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                #region amend Young Adult APIs

                string orderChangeAmendAPISRequestYoungAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "YOUNG_1", "GBE", passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress");

                HttpResponseMessage orderChangeAmendAPIsRSYoungAdult = NDCTransformationEndpoints.OrderChange(orderChangeAmendAPISRequestYoungAdult).EnsureIsSuccessStatusCode();

                dynamic orderChangeAmendAPIsResultYoungAdult = orderChangeAmendAPIsRSYoungAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeAmendAPIsResultYoungAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.OrderChangeRQValidateAPIs(orderChangeAmendAPISRequestYoungAdult, orderChangeAmendAPIsResult, out result, passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeNameCorrectionAndAmendAPIS)]
        public void OrderChangeAmendAPISRQValidations()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(195, 100, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.PremiumFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", greencard: "greencard", knowntravller: "knowntraveller", visa: "visa", redress: "redress", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRequest = result1["orderCreateRQ"];

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                #region amend Adult APIs validations

                XmlDocument updatedXmlRQ = new XmlDocument();

                XmlDocument xmlRsDoc = new XmlDocument();

                //passport Issue date in future
                string orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[1]/ct:IssueDate", xmlNamespaceManager).InnerText = "2027-08-10";

                HttpResponseMessage orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                dynamic orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //passport expiry date in past
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[1]/ct:ExpiryDate", xmlNamespaceManager).InnerText = "2020-08-10";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //incorrect given name
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[1]/ct:GivenName", xmlNamespaceManager).InnerText = "dam";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //incorrect surname
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[1]/ct:Surname", xmlNamespaceManager).InnerText = "dam";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //incorrect IdentityDocTypeCode
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[1]/ct:IdentityDocTypeCode", xmlNamespaceManager).InnerText = "ST";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));


                //green card Issue date in future
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:IssueDate", xmlNamespaceManager).InnerText = "2027-08-10";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //green card expiry date in past
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:ExpiryDate", xmlNamespaceManager).InnerText = "2020-08-10";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //green card incorrect given name
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:GivenName", xmlNamespaceManager).InnerText = "dam";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //green card incorrect surname
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:Surname", xmlNamespaceManager).InnerText = "dam";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //green card incorrect IdentityDocTypeCode
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", greencard: "greencard");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:IdentityDocTypeCode", xmlNamespaceManager).InnerText = "ST";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));


                //knownttaveller Issue date in future
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", knowntraveller: "knowntraveller");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:IssueDate", xmlNamespaceManager).InnerText = "2027-08-10";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //knownttaveller expiry date in past
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", knowntraveller: "knowntraveller");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:ExpiryDate", xmlNamespaceManager).InnerText = "2020-08-10";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //knownttaveller incorrect IdentityDocTypeCode
                orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderCreateResult, "ADULT_1", "ADT", passport: "passport", knowntraveller: "knowntraveller");

                updatedXmlRQ.LoadXml(orderChangeAmendAPISRequestAdult);

                updatedXmlRQ.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:IdentityDoc[2]/ct:IdentityDocTypeCode", xmlNamespaceManager).InnerText = "ST";

                orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(updatedXmlRQ.OuterXml);

                orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                xmlRsDoc.LoadXml(orderChangeAmendAPIsResultAdult);

                softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "914", "APIS data: Invalid data found for ADULT_1", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                #endregion
                softAssert.AssertAll();

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();

                Assert.True(true);
            }, 2);
        }

    }
}
