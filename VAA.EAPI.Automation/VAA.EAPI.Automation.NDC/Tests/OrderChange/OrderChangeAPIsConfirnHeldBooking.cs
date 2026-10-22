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
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;

namespace VAA.EAPI.Automation.NDC.Tests.OrderChange
{

    public class OrderChangeAPIsConfirnHeldBooking : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public OrderChangeAPIsConfirnHeldBooking(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void OrderChangeAPIsHoldBooking_InvalidSurname_TC01()
        {
            RetryHelper.RunWithRetry(() =>
            {

                string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");
                int maxNumberOfDays = 150;
                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                //Generate Air Shopping request
                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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
                string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, paymentType:"CPDVisa", passport: "passport");

                XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
                XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
                orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

                XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
                nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

                XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
                nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

                updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = "Satish";

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml).EnsureIsSuccessStatusCode();

                dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

                string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

                Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

                Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");
                
                dynamic orderHistoryRQ = RequestHelper.GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                dynamic orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();
                
                Assert.True(true);
            }, 3);
        }

        [Trait("Category", "orderchangeAPIsAndGSTRegression")]
        [Fact]
        public void OrderChangeAPIsHoldBooking_InvalidGivenName_TC02()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = "!@#$%";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml).EnsureIsSuccessStatusCode();

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");
        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Fact]
        public void OrderChangeAPIsHoldBooking_WrongSurname_TC03()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = "Sammy";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Fact]
        public void OrderChangeAPIsHoldBooking_WrongGivenName_TC04()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = "Adams";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");




        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectCountryOfCitizenShip_TC05()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.EWR, locationThree: LocationType.EWR, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:CitizenshipCountryCode", orderChangeRequestXmlnsManager).InnerText = "ABCDEF";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectResidenceCountryCode_TC06()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:ResidenceCountryCode", orderChangeRequestXmlnsManager).InnerText = "ABCDEF";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectDocumnentID_TC07()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:IdentityDocID", orderChangeRequestXmlnsManager).InnerText = "ABC@#$%DEF";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectIssuingCountryCode_TC08()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.EWR, locationThree: LocationType.EWR, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:IssuingCountryCode", orderChangeRequestXmlnsManager).InnerText = "ABCDEF";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectHostCountryCode_TC09()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "visa", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:Visa//def:HostCountryCode", orderChangeRequestXmlnsManager).InnerText = "ABCDEF";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }


        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectFormatIssueDate_TC10()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.IAD, locationThree: LocationType.IAD, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "visa", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:IssueDate", orderChangeRequestXmlnsManager).InnerText = "17/02/2009";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("912"), $"Expected 912 but actual was {code}");

            Assert.True(descText.Equals("Please complete all mandatory fields in request as per given specifications"), $"Expected : Please complete all mandatory fields in request as per given specifications but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__FutureIssueDate_TC11()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:IssueDate", orderChangeRequestXmlnsManager).InnerText = "2028-07-03";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectFormatExpiryDate_TC12()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:ExpiryDate", orderChangeRequestXmlnsManager).InnerText = "17/02/2009";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("912"), $"Expected 912 but actual was {code}");

            Assert.True(descText.Equals("Please complete all mandatory fields in request as per given specifications"), $"Expected : Please complete all mandatory fields in request as per given specifications but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__PastExpiryDate_TC13()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:ExpiryDate", orderChangeRequestXmlnsManager).InnerText = "2016-06-03";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectDocumentID_TC14()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:IdentityDocTypeCode", orderChangeRequestXmlnsManager).InnerText = "ATREWEDF";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_1"), $"Expected : APIS data: Invalid data found for ADULT_1. but actual was {descText}");



        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking__IncorrectAPIsInfoFor1Pax_TC15()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, deptDate: journey1Date);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("APIS data: Invalid data found for ADULT_2"), $"Expected : APIS data: Invalid data found for ADULT_2. but actual was {descText}");



        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_Passport_TC16()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "CPDMaster", passport: "passport");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml).EnsureIsSuccessStatusCode();

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_Redress_TC17()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", redress: "Redress");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);


            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            // updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = "!@#$%";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", redress: "Redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_KTN_TC18()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);


            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", knowntraveller: "KTN"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_Passport_ORDERCREATEANDCHANGE_TC19()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", passport: "PASSPORT");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:IdentityDocID", orderCreateRequestXmlnsManager).InnerText = "3265985PT";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_Redress_ORDERCREATEANDCHANGE_TC20()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", passport: "passport", redress: "Redress");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", redress: "Redress");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:RedressCase//def:RedressCaseID", orderCreateRequestXmlnsManager).InnerText = "red123456789";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", redress: "Redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_KTN_ORDERCREATEANDCHANGE_TC21()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", passport: "passport", knowntraveller: "KTN");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", knowntraveller: "ktn");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc[2]//def:IdentityDocID", orderCreateRequestXmlnsManager).InnerText = "CR000011111";


            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", knowntraveller: "KTN"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_PTRedressKTN_TC22()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", passport: "PASSPORT");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", redress: "Redress", knowntraveller: "KTN");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            // updatedOrderChangeRequest1.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = "!@#$%";

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", redress: "Redress", knowntraveller: "KTN"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_PTordercreate_RedressKTNVisaGCorderchange_TC23()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,

                JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, deptDate: journey1Date,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "via");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            ////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            ////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            //////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            //////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            //updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            //updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            ////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            ////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            //updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            //updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            ////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            ////updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;


            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", knowntraveller: "KTN", visa: "Visa", redress: "Redress", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_PTGreencard_TC24()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", greencard: "Greencard");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_PTVisa_TC25()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "Visa");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", visa: "Visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));


        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_PTordercreate_VisaGCorderchange_TC26()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", passport: "PASSPORT");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", visa: "Visa", greencard: "Greencard");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", visa: "Visa", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        //Positive scenarios for Passport VISA APIS
        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_1Pax_PassportVISA_TC27()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "visa", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderChangeRequest, orderCreateResult, out ValidationResultHelper result, passport: "passport", visa: "Visa", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_OCreateGC_OchangePassportVISA_TC27()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", greencard: "greencard");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "visa", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            updatedOrderChangeResponse.SelectSingleNode("//def:PaxList//def:Pax[1]//def:IdentityDoc[2]", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateResponse.SelectSingleNode("//def:PaxList//def:Pax[1]//def:IdentityDoc", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeResponse.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[2]", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateResponse.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc", orderChangeRequestXmlnsManager).InnerText;

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderChangeRequest, orderChangeResult, out ValidationResultHelper result, passport: "passport", visa: "Visa", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        //Greencardd

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_1Pax_PassportGC_TC28()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
               cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", passport: "passport");

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", greencard: "greencard", apiInfo: "apiInfo", passport: "passport");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderChangeResult, out ValidationResultHelper result, passport: "passport", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "OrderChangeAPIsRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_2Pax_GCPassport_TC29()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized", greencard: "greencard");

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


            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//def:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;


            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:GivenName", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc//def:Surname", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderChangeRequestXmlnsManager).InnerText;

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            updatedOrderChangeResponse.SelectSingleNode("//def:PaxList//def:Pax[1]//def:IdentityDoc[2]", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateResponse.SelectSingleNode("//def:PaxList//def:Pax[1]//def:IdentityDoc", orderChangeRequestXmlnsManager).InnerText;
            updatedOrderChangeResponse.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[2]", orderChangeRequestXmlnsManager).InnerText = updatedOrderCreateResponse.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc", orderChangeRequestXmlnsManager).InnerText;
            SoftAssertHelper softAssert = new SoftAssertHelper();
            Assert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderChangeResult, out ValidationResultHelper result, passport: "passport", greencard: "Greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }

        [Trait("Category", "OrderChangeAPIsRegression")]
        [Trait("Category", "orderchangeAPIsAndGSTRegression")]

        [Fact]
        public void OrderChangeAPIsHoldBooking_MultiPax_AllAPIS_TC30()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
               cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized", passport: "passport", visa: "visa", greencard: "greencard", redress: "redress", knowntraveller: "KTN", apiInfo: "apiInfo");

            XmlDocument updatedOrderChangeRequest1 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest1.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest1.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest1.SelectSingleNode("//def:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest1.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:IdentityDoc[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:IdentityDoc[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;

            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:GivenName", orderCreateRequestXmlnsManager).InnerText;
            updatedOrderChangeRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:IdentityDoc[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//def:PaxList//def:Pax[4]//def:Individual//def:Surname", orderCreateRequestXmlnsManager).InnerText;

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest1.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderChangeResult, out ValidationResultHelper result, passport: "passport", visa: "Visa", greencard: "greencard"), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
        }


    }
}