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

namespace VAA.EAPI.Automation.NDC.Tests.OrderCreate
{
    public class OrderCreateEmailAddress : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        

        public OrderCreateEmailAddress(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));

        }
        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_FlightOnlyOneWay()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", flyingClubMember: "loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_flightOnlyReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", flyingClubMember: "loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_FreeSeatOneWay()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Free, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_FreeSeatReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Free, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_PaidSeatOneWay()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.Virgin);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Paid, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_PaidSeatReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Paid, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_NonIATA_Agent_FlightOnlyOneWay()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", flyingClubMember: "loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_NonIATA_Agent_flightOnlyReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", flyingClubMember: "loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_NonIATA_Agent_FreeSeatOneWay()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Free, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_NonIATA_Agent_FreeSeatReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            //Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Free, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_NonIATA_Agent_PaidSeatOneWay()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Paid, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_NonIATA_Agent_PaidSeatReturn()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Paid, seatAvailabilityResult);

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_PaidSeatReturnGoldMember()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date, flighttype: "Direct");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Paid, seatAvailabilityResult, flyingClubMember: "Loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:LoyaltyProgramAccount//def:AccountNumber", orderCreateRequestXmlnsManager).InnerText = "9999998868";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "HUNDREDSIX";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText = "pcctest";

            XmlDocument xmlDocument_1 = new XmlDocument();

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateRequestXmlnsManager).InnerText;

            string tagText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:TagText", orderCreateRequestXmlnsManager).InnerText;

            Assert.True(descText.Equals("Price mismatch – Total Amount charged for seats 0GBP"), $"actual {descText} actual Price mismatch – Total Amount charged for seats 0GBP ");

            Assert.True(tagText.Equals("Seat Price change"), $"Tag text is expected as Seat Price change , actual{tagText} ");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_PaidSeatReturnGoldMemberEconomyLight()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date, flighttype: "Direct");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", SeatPriceType.Paid, seatAvailabilityResult, flyingClubMember: "Loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:LoyaltyProgramAccount//def:AccountNumber", orderCreateRequestXmlnsManager).InnerText = "9999998868";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "HUNDREDSIX";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText = "pcctest";

            XmlDocument xmlDocument_1 = new XmlDocument();

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateRequestXmlnsManager).InnerText;

            string tagText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:TagText", orderCreateRequestXmlnsManager).InnerText;

            Assert.True(descText.Equals("Price mismatch – Total Amount charged for seats 0GBP"), $"actual {descText} actual Price mismatch – Total Amount charged for seats 0GBP ");

            Assert.True(tagText.Equals("Seat Price change"), $"Tag text is expected as Seat Price change , actual{tagText} ");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_OneWayGoldMember()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 1, deptDate: journey1Date, flighttype: "Direct");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa", SeatPriceType.Paid, seatAvailabilityResult, flyingClubMember: "Loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:LoyaltyProgramAccount//def:AccountNumber", orderCreateRequestXmlnsManager).InnerText = "9999998868";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "HUNDREDSIX";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText = "pcctest";

            XmlDocument xmlDocument_1 = new XmlDocument();

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateRequestXmlnsManager).InnerText;

            string tagText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:TagText", orderCreateRequestXmlnsManager).InnerText;

            Assert.True(descText.Equals("Price mismatch – Total Amount charged for seats 0GBP"), $"actual {descText} actual Price mismatch – Total Amount charged for seats 0GBP ");

            Assert.True(tagText.Equals("Seat Price change"), $"Tag text is expected as Seat Price change , actual{tagText} ");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_IATA_Agent_PaidSeatReturnSilverMember()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(6).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 1;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date, flighttype: "Direct");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized", flyingClubMember: "Loyality");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:LoyaltyProgramAccount//def:AccountNumber", orderCreateRequestXmlnsManager).InnerText = "1087326235";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Lori";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Virgintest";

            XmlDocument xmlDocument_1 = new XmlDocument();

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateRequestXmlnsManager).InnerText;

            string tagText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:TagText", orderCreateRequestXmlnsManager).InnerText;

            Assert.True(descText.Equals("Price mismatch – Total Amount charged for seats 0GBP"), $"actual {descText} actual Price mismatch – Total Amount charged for seats 0GBP ");

            Assert.True(tagText.Equals("Seat Price change"), $"Tag text is expected as Seat Price change , actual{tagText} ");

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }
    }
}
