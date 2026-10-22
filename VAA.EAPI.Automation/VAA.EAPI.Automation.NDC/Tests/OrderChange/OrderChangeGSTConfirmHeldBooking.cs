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
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;

namespace VAA.EAPI.Automation.NDC.Tests.OrderChange
{
    [Collection("OrderChange")]
    public class OrderChangeGSTConfirmHeldBooking : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        

        public OrderChangeGSTConfirmHeldBooking(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        public string offerpricettlDateTime = "";
        public string ordercreatettlDateTime = "";

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_1Adult_TC01()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");
            int maxNumberOfDays = 60;
            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml).EnsureIsSuccessStatusCode();

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml).EnsureIsSuccessStatusCode();

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(OrderChangeValidators.ValidateGST(updatedOrderChangeRequest2.OuterXml, orderChangeResult, out ValidationResultHelper result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_AllPax_TC02()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Daiman";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "sana";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Jam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlNode node3 = updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:Employer", orderCreateRequestXmlnsManager);

            node3.ParentNode.RemoveChild(node3);

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(OrderChangeValidators.ValidateGST(updatedOrderChangeRequest2.OuterXml, orderChangeResult, out ValidationResultHelper result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_1Youth_TC03()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfyouths: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "damin";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "linga";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlNode node2 = updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(OrderChangeValidators.ValidateGST(updatedOrderChangeRequest2.OuterXml, orderChangeResult, out ValidationResultHelper result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_1Child_TC04()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "damin";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "linga";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlNode node2 = updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(OrderChangeValidators.ValidateGST(updatedOrderChangeRequest2.OuterXml, orderChangeResult, out ValidationResultHelper result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeResult, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidOrganizationName_TC05()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:Name", orderCreateRequestXmlnsManager).InnerText = "QWERTYUIOPASDFGHJKLZXCVBNMQWERTYUIOOJHGFGHXCNMVCJGCNCNHCGNCCHNCGGC";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Organaization Name in the request"), $"GST Details Validation - Organaization Name in the request but actual was {descText}");

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidEmailAddressText_TC06()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid E-Mail details in the request"), $"Expected 914 but actual was {descText}");

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidCityName_TC07()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:CityName", orderCreateRequestXmlnsManager).InnerText = "1234567890oikmnbvcxzsertyhnbvcxdfgvghh";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid address details in the request"), $"GST Details Validation - Invalid address details in the request but actual was {descText}");

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidPhoneNumber_TC08()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:PhoneNumber", orderCreateRequestXmlnsManager).InnerText = "123456789876543234567876543234567845658654543";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid Phone number in the request"), $"Expected GST Details Validation - Invalid Phone number in the request but actual was {descText}");

        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidPostalCode_TC09()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 100;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:PostalCode", orderCreateRequestXmlnsManager).InnerText = "12345678987654345678764534577645765755575765";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid address details in the request"), $"Expected GST Details Validation - Invalid address details in the request but actual was {descText}");

        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidSalesTaxRegistrationID_TC10()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");
            int maxNumberOfDays = 200;
            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml).EnsureIsSuccessStatusCode();

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml).EnsureIsSuccessStatusCode();

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:SalesTaxRegistrationID", orderCreateRequestXmlnsManager).InnerText = "1234579909876543234567898765434567876543456867686";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml).EnsureIsSuccessStatusCode();

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - GST Number in the request"), $"Expected GST Details Validation - GST Number in the requestt but actual was {descText}");
        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeGSTConfirmHeldBooking_InvalidStreetTextTC11()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 100;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.Virgin);

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//def:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", gst: "gst");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest2.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:StreetText", orderCreateRequestXmlnsManager).InnerText = "aesdhjfgkjhcgjhfxgxbvxbgnchmvhcghtfhmcnxvfhdhfmvcbxvnzbdfxgnchmc bvnfdhmcbxnvghmcgxnc";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderChangeResponse.LoadXml(orderChangeResult.ToString());

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid address details in the request"), $"Expected GST Details Validation - Invalid address details in the request but actual was {descText}");

        }

    }
}
