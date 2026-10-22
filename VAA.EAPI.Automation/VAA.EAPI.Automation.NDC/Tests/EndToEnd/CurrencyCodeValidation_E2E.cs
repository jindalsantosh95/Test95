using System.Globalization;
using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.EndToEnd
{
    public class CurrecyCodeValidation_E2E : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public CurrecyCodeValidation_E2E(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 0, 0, 0, "5", "91281035", "GBP")]
        public void TC01_CurrVal_GBagent_reqGBP_JFKLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "GBP";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);

            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);


            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "91281035", "INR")]
        public void TC02_CurrVal_GBagent_reqINR_DELLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "GBP";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //   Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0);

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);


            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);


        }
        [Trait("Category", "OrderChangeAllRegression")]

        [Theory]
        [InlineData(1, 1, 0, 0, "5", "14302691", "GBP")]
        public void TC03_CurrVal_INagent_reqGBP_DELLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "GBP";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);

            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //   Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0);

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;
            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);


        }
        [Trait("Category", "OrderChangeAllRegression")]

        [Theory]
        [InlineData(1, 1, 0, 0, "5", "14302691", "INR")]
        public void TC04_CurrVal_INagent_reqINR_DELLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "GBP";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);
            //Assert warning message 
            Assert.True(Assertions.ValidateWarningwithCurrencyCodeReturned(airShoppingResult, out string errorWarningMessage), errorWarningMessage);

            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //   Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0);

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);


        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "00003333", "GBP")]
        public void TC05_CurrVal_USagent_reqGBP_JFKLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "USD";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);


        }
        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "00003333", "USD")]
        public void TC06_CurrVal_USagent_reqUSD_JFKLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "USD";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);
            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "59211202", "USD")]
        public void TC07_CurrVal_NGagent_reqUSD_JFKLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "USD";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);
            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);


            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "59211202", "NGN")]
        public void TC08_CurrVal_NGagent_reqNGN_JFKLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "NGN";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);
            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //   Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0);

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Theory]
        [InlineData(1, 1, 0, 0, "5", "59211202", "GBP")]
        public void TC09_CurrVal_NGagent_reqGBP_JFKLHR_return(int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, string participatingOrgOrgID, string CurCode)
        {
            string expCurrCode = "USD";
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, curCode: CurCode);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            //1 -- Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Asserting Currency code in Airshopping response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(airShoppingResult, expCurrCode, out string currCodeAirshoppingMessage), currCodeAirshoppingMessage);

            //Assert warning
            Assert.True(Assertions.ValidateWarningwithCurrencyCodeReturned(airShoppingResult, out string errorWarningMessage), errorWarningMessage);
            //2 --Generate OfferPrice Req
            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //   Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0);

            //3 - Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Create Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()} \r\n " + $"Order Create Request sent was {orderCreateResponse.ReadRequestAsString()}");

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Asserting currency code in Order create response
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderCreateResult, expCurrCode, out string currCodeOrderCreateMessage), currCodeOrderCreateMessage);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            orderCreateResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderCreateResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //4 Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Retrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()} \r\n " + $"OrderRetrieve Request sent was {orderRetrieveResponse.ReadRequestAsString()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert currency code in Order Retrieve
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderRetrieveResult, expCurrCode, out string currCodeOrderRetrieveMessage), currCodeOrderRetrieveMessage);

            //5 Reshop
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            orderRetrieveResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate1 = date.AddDays(5);
            string reshopJourney1Date = newDate1.ToString("yyyy-MM-dd");

            DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime newDate2 = date1.AddDays(5);
            string reshopJourney2Date = newDate2.ToString("yyyy-MM-dd");

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.Return, locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL
                , newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date);


            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Reshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()} \r\n " + $" OrderReshop Request sent was {orderReshopResponse.ReadRequestAsString()}");

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            //Assert currency code in REshop
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderReshopResult, expCurrCode, out string currCodeOrderReshopMessage), currCodeOrderReshopMessage);

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //6 -Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Quote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()} \r\n " + $" OrderQuote Request sent was {orderQuoteResponse.ReadRequestAsString()}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Assert currency code in Order Quote
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderQuoteResult, expCurrCode, out string currCodeOrderQuoteMessage), currCodeOrderQuoteMessage);

            // 7- Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaymentFunctions", orderChangeRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from Order Change Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()} \r\n " + $" Order Change Request sent was {orderChangeResponse.ReadRequestAsString()}");

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            //Assert currency code in Order change
            Assert.True(Assertions.ValidateCurrencyCodeOfOffersReturned(orderChangeResult, expCurrCode, out string currCodeOrderChangeMessage), currCodeOrderChangeMessage);


        }
    }
}
