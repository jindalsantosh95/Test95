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
    public class PrivateFare : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public PrivateFare(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdOasisTravel, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdOasisTravel}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSwedishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSwedishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSaudiAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSaudiAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCanadianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCanadianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCarrebianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCarrebianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCaribjetAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCaribjetAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdEuropeanAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdEuropeanAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgICorporateAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgICorporateAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", "Local-Run")]

        [InlineData(1, AgentID.ParticipatingOrgIdTravelRepublic, "2", CabinType.UpperClass, LocationType.LHR, LocationType.JFK, null, "758", FareSubTypeCode.Marine, "CPDVisa", SeatPriceType.Paid, "CPDVisa")]
        [InlineData(3, AgentID.ParticipatingOrgIVFRAgent, "2", CabinType.UpperClass, LocationType.LHR, LocationType.DEL, null, "758", FareSubTypeCode.VFR, "Cash", SeatPriceType.None, "Cash")]
        [InlineData(4, AgentID.ParticipatingOrgCharityAgent, "2", CabinType.UpperClass, LocationType.LHR, LocationType.JNB, null, "758", FareSubTypeCode.Charity, "Cash", SeatPriceType.None, "Cash")]
        [InlineData(5, AgentID.ParticipatingOrgIdCarrebianAgent, "2", CabinType.UpperClass, LocationType.LHR, LocationType.JNB, null, "758", FareSubTypeCode.TourOperator, "Cash", SeatPriceType.None, "Cash")]
        [InlineData(6, AgentID.ParticipatingOrgIdIndiaTestAgent, "2", CabinType.UpperClass, LocationType.LHR, LocationType.JFK, null, "758", FareSubTypeCode.SME, "Cash", SeatPriceType.None, "Cash")]
        [InlineData(7, AgentID.ParticipatingOrgIdEuropeanAgent, "2", CabinType.UpperClass, LocationType.LHR, LocationType.DEL, null, "758", FareSubTypeCode.Student, "Cash", SeatPriceType.None, "Cash")]
        [InlineData(8, AgentID.ParticipatingOrgCharityAgent, "2", CabinType.UpperClass, LocationType.LHR, LocationType.JNB, "9000000000387", "70G", FareSubTypeCode.Corporate, "Cash", SeatPriceType.None, "Cash")]
        [InlineData(9, AgentID.ParticipatingOrgIdAmericanExpressServicesUK, "4", CabinType.Premium, LocationType.LHR, LocationType.JFK, null, "758", FareSubTypeCode.SeatOnly, "CPDVisa", SeatPriceType.Paid, "CPDVisa")]
        public async Task PrivateFare_VariousScenarios(int test, string participatingOrgOrgID, string cabinCode, CabinType cabinName, LocationType locationOne, LocationType locationTwo, string corporate, string fareTypeCode, FareSubTypeCode fareSubType, string payment, SeatPriceType seatPriceType, string orderChangePayment)
        {
            Dictionary<string, string> shoppingResult = ShoppingHelper.ShoppingRequest(180, 20, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: cabinCode, cabinName: cabinName, journeyType: JourneyType.Return, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationTwo, locationFour: locationOne, locationFive: LocationType.None, locationSix: LocationType.None, offerRequestType: "PrivateFare", taxonomy: "taxonomy", corporate: corporate, fareTypeCode: fareTypeCode, fareSubType: fareSubType, paymentType: payment, changeOrder: ChangeOrder.CompleteJourney, exchangeType: "AddCol", seatType: seatPriceType, orderChangePayment: orderChangePayment);

            string airShoppingRequest = shoppingResult["airShoppingRQ"];
            string airShoppingResult = shoppingResult["airShoppingResult"];
            string offerPriceRequest = shoppingResult["offerPriceRQ"];
            string offerPriceResult = shoppingResult["offerPriceResult"];
            string orderCreateRequest = shoppingResult["orderCreateRQ"];
            string orderCreateResult = shoppingResult["orderCreateResult"];
            string orderReshopRequest = shoppingResult["orderReshopRQ"];
            string orderReshopResult = shoppingResult["orderReshopRS"];
            string orderQuoteRequest = shoppingResult["orderQuoteRQ"];
            string orderQuoteResult = shoppingResult["orderQuoteRS"];
            string orderChangeRequest = shoppingResult["orderChangeRQ"];
            string orderChangeResult = shoppingResult["OrderChangeResult"];

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlDocument updatedAirshoppingResponse = new XmlDocument();
            updatedAirshoppingResponse.LoadXml(airShoppingResult);

            string fareSubTypeDescription = GetEnumDescription(fareSubType);

            SoftAssertHelper softAssert = new SoftAssertHelper();
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

            HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

            string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

            HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

            string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //retrieve the pnr
            string orderRetrieveReRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveReResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveReRequest).EnsureIsSuccessStatusCode();

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult1 = orderRetrieveReResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument1 = new XmlDocument();
            orderRetrieveResponseXmlDocument1.LoadXml(orderRetrieveResult1);

            XmlDocument orderChangeUpdatedPaxNameXmlDocument = new XmlDocument();

            //validate Adult_1 name in retrieve response
            orderChangeUpdatedPaxNameXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

            XmlNamespaceManager xmlNamespaceManager = orderChangeUpdatedPaxNameXmlDocument.CreateXmlNamespaceManager();
            XmlNamespaceManager orderRetrieveXmlNamespaceManager = orderRetrieveResponseXmlDocument1.CreateXmlNamespaceManager();

            string adult1UpdatedGivenName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

            string adult1UpdatedSurName = orderChangeUpdatedPaxNameXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

            string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument1.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", orderRetrieveXmlNamespaceManager).InnerText;

            string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument1.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", orderRetrieveXmlNamespaceManager).InnerText;

            if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
            {
                result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
            }

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(updatedAirshoppingResponse, out result), "Offers returned validation failed.");
            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(updatedAirshoppingResponse, out result), "Operating leg validation failed.");
            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, updatedAirshoppingResponse, out result), "Cabin type validation failed.");
            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequestDocument, out result), "Dated marketing and operating list validation failed.");
            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, updatedAirshoppingResponse, out result), "Passenger segment list validation failed.");
            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(updatedAirshoppingResponse, out result), "Baggage allowance list validation failed.");
            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, updatedAirshoppingResponse, out result), "Price class list validation failed.");
            softAssert.True(AirshoppingValidator.ValidatePrice(updatedAirshoppingResponse, out result), "Price validation failed.");
            softAssert.True(AirshoppingValidator.ValidateFareCodeType(updatedAirshoppingResponse, out result, fareTypeCode, fareSubTypeDescription), "Fare code type validation failed.");
            softAssert.True(AirshoppingValidator.ValidatePriceClassListCabinType(airShoppingRequestDocument, updatedAirshoppingResponse, out result), "Price class list cabin type validation failed.");
            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(updatedAirshoppingResponse, out result), "Mandatory indicator validation failed.");
            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, updatedAirshoppingResponse, out result), "Currency validation failed.");
            softAssert.True(AirshoppingValidator.ValidateTaxSummary(airShoppingRequestDocument, updatedAirshoppingResponse, out result), "Tax summary validation failed.");
            softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
            Assert.True(true);
        }
    }
}
