using System.Globalization;
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
using VAA.EAPI.Automation.NDC.Tests.EndToEnd;
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OrderRetrieve
{
    public class OrderRetrieveRegression : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderRetrieveRegression(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Trait("Category", "NDCE2ESanity")]
        [Fact]
        public void OrderRetrieve_TC01_OneWayDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");
            int maxNumberOfDays = 60;
            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdSkyscannerInDirect,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date,participatingOrgOrgID2:AgentID.ParticipatingOrgIdTravelFusionAggregator,agentType:"InDirect");

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyLight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml).EnsureIsSuccessStatusCode();

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC02_OneWayDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "CHEAP FLIGHTS LTD", participatingOrgOrgID: "00002222",
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOS,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC03_ReturnDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC04_ReturnDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(160).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 140;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.IAD, locationThree: LocationType.IAD, locationFour: LocationType.LHR,

                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC05_ReturnDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(60).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 240;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC06_ReturnConnectingFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.NonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC07_ReturnConnectingFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.EWR, locationThree: LocationType.EWR, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC08_ReturnConnectingFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.MBJ, locationThree: LocationType.MBJ, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UntokenizedVisa");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC09_MultiCityDirectFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "2", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC10_MultiCityConnectingFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "2", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC11_PaidSeats_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC12_PaidSeats_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            

        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC13_PaidSeatsCodeShare_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC14_PaidSeatsCodeShare_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 1, deptDate: journey1Date);


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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            Assert.False(warning.Count > 0, $"seats are not booked");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC15_FreeSeats_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, cabinTypeCodeOne: "4", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.Virgin);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC16_FreeSeats_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC17_FreeSeastOutBoundOnly_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.Virgin);

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            for (int i = 4; i >= 3; i--)
            {
                XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[{i}]", orderCreateRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            }

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC18_FreeSeastInBoundOnly_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

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

            for (int i = 1; i <= 2; i++)
            {
                XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode($"//def:CreateOrder//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[{i}]", orderCreateRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            }

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.False(warning.Count > 0, $"seats are not booked");

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC19_HoldBookingOneWayDirectFlight_Scenario()
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            

        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC20_HoldBookingOneWayConnectingFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC21_HoldBookingReturnDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            XmlNode bookingIdNodeToRemove = updatedOrderRetrieveRequest.SelectSingleNode("//def:BookingRefFilterCriteria", orderRetrieveRequestXmlnsManager);
            bookingIdNodeToRemove.ParentNode.RemoveChild(bookingIdNodeToRemove);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}\r\n " + $"OrderRetrieve Request sent was {updatedOrderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC22_HoldBookingReturnDirectFlight_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(166).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 140;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC23_HoldBookingReturnConnectingFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            XmlNode orderIdNodeToRemove = updatedOrderRetrieveRequest.SelectSingleNode("//def:OrderFilterCriteria", orderRetrieveRequestXmlnsManager);
            orderIdNodeToRemove.ParentNode.RemoveChild(orderIdNodeToRemove);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC24_HoldBookingMultiCityDirectFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 100;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", cabinTypeCodeThree: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC25_HoldBookingMultiCityConnectingFlights_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "2", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "HoldBooking"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC26_ConjunctionTicket_Scenario()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            //Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "59211202", "NGN")]
        [InlineData(1, 1, 0, 0, "5", "59211202", "USD")]
        public void OrderRetrieve_TC27_NGN_USD_Currency_NGagent_JFKLHR_Return_Scenario(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = CurCode;
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);
            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);
            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from Offer Price - response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()} \r\n " + $"Offer Price - Request sent was {offerPriceResponse.ReadRequestAsString()}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Asserting Currency code in offer price response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(offerPriceResult, expCurrCode, out string currCodeOfferMessage), currCodeOfferMessage);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/OfferPriceRS");

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UntokenizedVisa");

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC28_GBP_Currency_USagent_JFKLHR_Return_Scenario()
        {
            string expCurrCode = "USD";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "00003333",
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, curCode: "GBP");
            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);
            //Assert warning message 
            Assert.True(Assertions.ValidateWarningwithCurrencyCodeReturned(airShoppingResult, out string errorWarningMessage), errorWarningMessage);
            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);
            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from Offer Price - response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()} \r\n " + $"Offer Price - Request sent was {offerPriceResponse.ReadRequestAsString()}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Asserting Currency code in offer price response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(offerPriceResult, expCurrCode, out string currCodeOfferMessage), currCodeOfferMessage);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/OfferPriceRS");

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC29_OrderChangeCompleteIternaryChangeOneWay()
        {
            //Generate Air Shopping request

            string deptDate = DateTime.Now.AddDays(125).ToString("yyyy-MM-dd");

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, deptDate: deptDate,
                cabinTypeCodeOne: "5", numberOfadults: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            XmlNode originalDeptDate = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "", paymentMethod: "Untokenized");

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = 1;
            int counter = 0;
            string orderReshopRequest = "";
            string orderReshopResult = "";
            IDictionary<string, string> reshopOfferDetails = new Dictionary<string, string>();

            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            do
            {

                DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime newDate = date.AddDays(counter);

                string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                    JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOM, newCabinTypeCodeOne: "5", deptDate: reshopJourney1Date);

                //Post Order Reshop request
                HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                //Getting the Order Reshop response and converting to XML Document
                orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                orderReshopResponseXmlDocument = new XmlDocument();
                orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                //Find offer based on Even Exchange
                reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

                counter++;

                if (reshopOfferDetails.Count > 0)
                {
                    break;
                }

            } while (counter < 6);

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");
            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string orderRetrieveRequest1 = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest1, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest1);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponseReissuedTicket = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponseReissuedTicket.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponseReissuedTicket.StatusCode} with message: {orderRetrieveResponseReissuedTicket.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResultReissuedTicket = orderRetrieveResponseReissuedTicket.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveReissuedResponseXmlDocument = new XmlDocument();
            orderRetrieveReissuedResponseXmlDocument.LoadXml(orderRetrieveResultReissuedTicket);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResultReissuedTicket, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResultReissuedTicket, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResultReissuedTicket, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResultReissuedTicket, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResultReissuedTicket, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResultReissuedTicket, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResultReissuedTicket, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResultReissuedTicket, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResultReissuedTicket, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResultReissuedTicket, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            

        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void OrderRetrieve_TC30_OpenJaw()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode,
                $"Success status code was not returned from Offer Price Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()} \r\n " +
                $"Offer Price Request sent was {offerPriceResponse.ReadRequestAsString()}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Tokenized");

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

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //validate xsd against response
            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage), contactListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage), originDestListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage), paxJourneyListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage), datedMarketingAndOperatingListMessage);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage), paxSegmentListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out string serviceDefinitionListMessage), serviceDefinitionListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage), orderItemIdServiceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage), baggageAllowanceListMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage), orderItemIdPriceMessage);

            Assert.True(OrderRetrieveAssertions.ValidateOperatingLeg(orderRetrieveResult, out string OperatingLegMessage), OperatingLegMessage);

            Assert.True(OrderRetrieveAssertions.ValidateAirCarftTypeCode(orderRetrieveResult, out string AirCarftTypeCodeMessage), AirCarftTypeCodeMessage);

            
        }

        [Fact]
        public void OrderRetrieve_OrdinalValueZero()
        {
            XmlDocument xmlDocument_1 = new XmlDocument();

            string request = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderRetrieve.xml"), "EF5YD4");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = new XmlNamespaceManager(updatedRequest.NameTable);
            responseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedRequest.SelectSingleNode("//def:Ordinal", responseXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderRetrieveRequest(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid ordinal number - should start from 1"), $"Error code expected is Invalid ordinal number - should start from 1 but actual is {descText}");

        }

        [Fact]
        public void OrderRetrieve_OrdinalValueNonNumerical()
        {
            XmlDocument xmlDocument_1 = new XmlDocument();

            string request = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderRetrieve.xml"), "EF5YD4");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = new XmlNamespaceManager(updatedRequest.NameTable);
            responseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedRequest.SelectSingleNode("//def:Ordinal", responseXmlnsManager).InnerText = "ABCD";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderRetrieveRequest(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid ordinal number - should start from 1"), $"Error code expected is Invalid ordinal number - should start from 1 but actual is {descText}");

        }
    }
}
