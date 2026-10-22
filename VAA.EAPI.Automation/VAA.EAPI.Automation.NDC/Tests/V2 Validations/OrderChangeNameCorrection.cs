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
using VAA.EAPI.Automation.NDC.Tests.OrderChange;
using VAA.EAPI.Automation.NDC.Tests.OrderReshop;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    public class OrderChangeNameCorrection : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderChangeNameCorrection(MemoryStoreFixture memoryStoreFixture)
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
        public void OrderChangeNameCorrectionSinglePax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(200, 100, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 1, flight: FlightScenarioType.Virgin, cabinCode: "4", cabinName: CabinType.PremiumFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.MBJ, locationThree: LocationType.JFK, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                //update adult pax name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                //Getting the Order retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                XmlDocument orderChangeUpdatedPaxNameXmlDocument = new XmlDocument();

                //validate Adult_1 name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

                XmlNamespaceManager xmlNamespaceManager = orderChangeUpdatedPaxNameXmlDocument.CreateXmlNamespaceManager();

                string adult1UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
                {
                    result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeNameCorrectionAndAmendAPIS)]
        public void OrderChangeNameCorrectionAllPax()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.MIA, locationThree: LocationType.JFK, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                //update adult pax name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //update adult 2 pax name
                orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_2", orderCreateRS: orderCreateResult);

                orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionADULT2Request = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionADULT2Response = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionADULT2Request).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionADULT2Result = orderChangeNameCorrectionADULT2Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionADULT2Result, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //update child pax name
                orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "CHILD_1", orderCreateRS: orderCreateResult);

                orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionChildRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionChildResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionChildRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionChildResult = orderChangeNameCorrectionChildResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionChildResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //update YOUNG ADULT pax name
                orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "YOUNG_1", orderCreateRS: orderCreateResult);

                orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionYoungRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionYoungResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionYoungRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionYoungResult = orderChangeNameCorrectionYoungResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionYoungResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //update Infant pax name
                orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "INFANT_1", orderCreateRS: orderCreateResult);

                orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionInfantRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionInfantResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionInfantRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionInfantResult = orderChangeNameCorrectionInfantResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionInfantResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                //Getting the Order retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                XmlDocument orderChangeUpdatedPaxNameXmlDocument = new XmlDocument();

                //validate Adult_1 name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

                XmlNamespaceManager xmlNamespaceManager = orderChangeUpdatedPaxNameXmlDocument.CreateXmlNamespaceManager();

                string adult1UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
                {
                    result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                //validate Adult_2 name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionADULT2Result);

                string adult2UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult2UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult2UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult2UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult2UpdatedGivenName.Is(adult2UpdatedGivenNameRetrieveRS) || !adult2UpdatedSurNameRetrieveRS.Is(adult2UpdatedSurName))
                {
                    result.Message.Add($"Adult2 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                //validate YOUNG ADULT name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionYoungResult);

                string youngUpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[3]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string youngUpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[3]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string youngUpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[3]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string youngUpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[3]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!youngUpdatedGivenName.Is(youngUpdatedGivenNameRetrieveRS) || !youngUpdatedSurNameRetrieveRS.Is(youngUpdatedSurName))
                {
                    result.Message.Add($"Young1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                //validate CHILD name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionChildResult);

                string childUpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[4]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string childUpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[4]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string childUpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[4]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string childUpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[4]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!childUpdatedGivenName.Is(childUpdatedGivenNameRetrieveRS) || !childUpdatedSurNameRetrieveRS.Is(childUpdatedSurName))
                {
                    result.Message.Add($"Child1 updated givename and surname are not returned in retrieve response. Expected given name --->{childUpdatedGivenName} but actual is {childUpdatedGivenNameRetrieveRS}, Expected surname ---> {childUpdatedSurName} but actual is {childUpdatedSurNameRetrieveRS}");
                }

                //validate infant name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionChildResult);

                string infantUpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[5]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string infantUpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[5]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string infantUpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[5]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string infantUpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[5]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!infantUpdatedGivenName.Is(infantUpdatedGivenNameRetrieveRS) || !infantUpdatedSurNameRetrieveRS.Is(infantUpdatedSurName))
                {
                    result.Message.Add($"infant updated givename and surname are not returned in retrieve response. Expected given name --->{infantUpdatedGivenName} but actual is {infantUpdatedGivenNameRetrieveRS}, Expected surname ---> {childUpdatedSurName} but actual is {childUpdatedSurNameRetrieveRS}");
                }

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeNameCorrectionAndAmendAPIS)]
        public void OrderChangeNameCorrectionDLflight()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.VirginAndNonVirginRequestedOperator, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.JFK, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", operatorName: FlightType.DL, agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                //update adult pax name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //update adult 2 pax name
                orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_2", orderCreateRS: orderCreateResult);

                orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionADULT2Request = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionADULT2Response = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionADULT2Request).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionADULT2Result = orderChangeNameCorrectionADULT2Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionADULT2Result, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                //Getting the Order retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                XmlDocument orderChangeUpdatedPaxNameXmlDocument = new XmlDocument();

                //validate Adult_1 name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

                XmlNamespaceManager xmlNamespaceManager = orderChangeUpdatedPaxNameXmlDocument.CreateXmlNamespaceManager();

                string adult1UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
                {
                    result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                //validate Adult_2 name in retrieve response
                orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionADULT2Result);

                string adult2UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult2UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult2UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult2UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[2]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult2UpdatedGivenName.Is(adult2UpdatedGivenNameRetrieveRS) || !adult2UpdatedSurNameRetrieveRS.Is(adult2UpdatedSurName))
                {
                    result.Message.Add($"Adult2 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        //Error Cases
        [Fact]
        [Trait("Category", Traits.NDCOrderChangeNameCorrectionAndAmendAPIS)]
        public void OrderChangeNameCorrectionPaxNegativeScenario()
        {
            Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

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

            //surname change with more than 3 characters
            string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

            updatedXmlRQ.LoadXml(orderReshopUpdatePaxRequest);

            XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

            updatedXmlRQ.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:UpdatePaxName//ct:Surname", xmlNamespaceManager).InnerText = "SANND";

            HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(updatedXmlRQ.OuterXml);

            string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().Result;

            xmlRsDoc.LoadXml(orderReshopUpdatePaxResult);

            softAssert.True(OrderReshopValidator.ValidateNameCorrectionErrorRS(orderReshopUpdatePaxResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //complete surname change
            orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

            updatedXmlRQ.LoadXml(orderReshopUpdatePaxRequest);

            updatedXmlRQ.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:UpdatePaxName//ct:Surname", xmlNamespaceManager).InnerText = "abCD";

            orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(updatedXmlRQ.OuterXml);

            orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().Result;

            xmlRsDoc.LoadXml(orderReshopUpdatePaxResult);

            softAssert.True(OrderReshopValidator.ValidateNameCorrectionErrorRS(orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //update child pax name
            orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "CHILD_1", orderCreateRS: orderCreateResult);

            orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

            orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderChangeNameCorrectionChildRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

            HttpResponseMessage orderChangeNameCorrectionChildResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionChildRequest).EnsureIsSuccessStatusCode();

            string orderChangeNameCorrectionChildResult = orderChangeNameCorrectionChildResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionChildResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //second attemp child name change
            orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "CHILD_1", orderCreateRS: orderCreateResult);

            updatedXmlRQ.LoadXml(orderReshopUpdatePaxRequest);

            updatedXmlRQ.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:UpdatePaxName//ct:Surname", xmlNamespaceManager).InnerText = "abCD";

            orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(updatedXmlRQ.OuterXml);

            orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().Result;

            xmlRsDoc.LoadXml(orderReshopUpdatePaxResult);

            softAssert.True(AirshoppingValidator.ValidateErrorXmlResponse(xmlRsDoc, "137", "Name Correction not permitted more than once per booking", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
            HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
            
            softAssert.AssertAll();
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderChangeNameCorrectionAndAmendAPIS)]
        public void OrderChangeNameCorrectionwithNONVS_NegativeScenario()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(200, 65, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.NonVirginRequestedWithOtherOperators, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", operatorName: FlightType.KL, agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedXmlRQ = new XmlDocument();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                //update adult pax name
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest);

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().Result;

                updatedOrderCreateResponse.LoadXml(orderReshopUpdatePaxResult);

                Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(updatedOrderCreateResponse, "137", "Name Correction permitted only on VS and DL operated flights", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                Assert.True(true);

            }, 1);
        }
    }
}
