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
    public class OrderCreateHoldBooking : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        private readonly TokenFixture tokenFixture;

        
        public OrderCreateHoldBooking(MemoryStoreFixture memoryStoreFixture)
        {
            this.tokenFixture = tokenFixture;

        }

        public string offerpricettlDateTime = "";
        public string ordercreatettlDateTime = "";

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void TC01_with3DS2paymentandGenderandDOB_OnlineAgent()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.MCO, locationThree: LocationType.MCO, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 2);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");

            //return orderCreateRequest;

            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");

            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;



            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");

        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC02_without3DS2paymentandwithGenderandDOB_OfflineAgent()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC03_ReplacingtheAgentfromTheOriginal()
        {

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");

            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");

            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }


            //Generate the Order Quote request 
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document         
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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


            string FinalOrderChangeRequest = updatedOrderChangeRequest2.OuterXml;

            //Replacing the value in request
            XmlDocument AgentOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager AgentorderChangeRequestXmlnsManager = new XmlNamespaceManager(AgentOrderChangeRequest.NameTable);
            AgentorderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            AgentOrderChangeRequest.LoadXml(FinalOrderChangeRequest);
            string Agent = "Kayak";
            AgentOrderChangeRequest.SelectSingleNode("//def:ParticipatingOrg//def:Name", AgentorderChangeRequestXmlnsManager).InnerText = Agent;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), AgentOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Not authorized (combination of Corporate and/or Particpant IDs not valid)";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("368"),
            $"Error Code for Order Change error message is not as expected. Expected 368 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC04_ValidatingwithInvalidGender()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");

            //return orderCreateRequest;

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");

            }

            //Order Retrieve                      
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            //int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;
            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");

            List<string> PaymentTTLretrieve = new List<string>();
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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


            string FinalOrderChangeRequest = updatedOrderChangeRequest2.OuterXml;

            //Replacing the value in request
            XmlDocument AgentOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager AgentorderChangeRequestXmlnsManager = new XmlNamespaceManager(AgentOrderChangeRequest.NameTable);
            AgentorderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            AgentOrderChangeRequest.LoadXml(FinalOrderChangeRequest);
            string Gender = "T";
            AgentOrderChangeRequest.SelectSingleNode("//def:Pax//def:Individual//def:GenderCode", AgentorderChangeRequestXmlnsManager).InnerText = Gender;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), AgentOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Please complete all mandatory fields in request as per given specifications";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("912"),
            $"Error Code for Order Change error message is not as expected. Expected 912 " +
            $"but was {code}");
        }


        [Fact]
        public void TC05_ExpiredHoldBooking()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 2);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }

            string bookingId = "EE3UWS";
            //Order Retrieve 
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingId);

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");
            string orderId = "VS932EE3UWS23";

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);
            string currencyCode = "GBP";
            string Amount = "185";


            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Unable to retrieve Order - Payment Time Limit has expired and the itinerary has been cancelled";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("303"),
            $"Error Code for Order Change error message is not as expected. Expected 303 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC06_Validatingwith_Invalid_AdultDOB()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;
            XmlDocument updatedOrderCreateRequest2 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderCreateRequest2.NameTable);
            orderCreateRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest2.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove2 = updatedOrderCreateRequest2.SelectSingleNode("//def:PaxList//def:Pax[1]//def:Birthdate", orderCreateRequestXmlnsManager2);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
            string OrderCreateRequest2 = updatedOrderCreateRequest2.OuterXml;

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), OrderCreateRequest2);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");

            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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


            string FinalOrderChangeRequest = updatedOrderChangeRequest2.OuterXml;

            //Replacing the value in request
            XmlDocument AgentOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager AgentorderChangeRequestXmlnsManager = new XmlNamespaceManager(AgentOrderChangeRequest.NameTable);
            AgentorderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            AgentOrderChangeRequest.LoadXml(FinalOrderChangeRequest);
            string AdultDOB = "2022-03-20";
            AgentOrderChangeRequest.SelectSingleNode("//def:PaxList//def:Pax//def:Birthdate", AgentorderChangeRequestXmlnsManager).InnerText = AdultDOB;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), AgentOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Invalid age for PTC. Adult passengers must be above 16 years old on the date of departure";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("73"),
            $"Error Code for Order Change error message is not as expected. Expected 73 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC07_Validatingwith_Invalid_YouthDOB()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;
            XmlDocument updatedOrderCreateRequest2 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderCreateRequest2.NameTable);
            orderCreateRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest2.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove2 = updatedOrderCreateRequest2.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Birthdate", orderCreateRequestXmlnsManager2);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
            string OrderCreateRequest2 = updatedOrderCreateRequest2.OuterXml;

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), OrderCreateRequest2);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");

            }

            //Order Retrieve            
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document          
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);
            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string FinalOrderChangeRequest = updatedOrderChangeRequest2.OuterXml;

            //Replacing the value in request
            XmlDocument AgentOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager AgentorderChangeRequestXmlnsManager = new XmlNamespaceManager(AgentOrderChangeRequest.NameTable);
            AgentorderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            AgentOrderChangeRequest.LoadXml(FinalOrderChangeRequest);
            string YouthDOB = "1943-03-20";
            AgentOrderChangeRequest.SelectSingleNode("//def:PaxList//def:Pax[2]//def:Birthdate", AgentorderChangeRequestXmlnsManager).InnerText = YouthDOB;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), AgentOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Invalid age for PTC. Young Child passengers must be 12 to 16 years old on the date of departure";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("73"),
            $"Error Code for Order Change error message is not as expected. Expected 73 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC08_Validatingwith_Invalid_ChildDOB()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;

            XmlDocument updatedOrderCreateRequest2 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderCreateRequest2.NameTable);
            orderCreateRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest2.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove2 = updatedOrderCreateRequest2.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Birthdate", orderCreateRequestXmlnsManager2);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
            string OrderCreateRequest2 = updatedOrderCreateRequest2.OuterXml;

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), OrderCreateRequest2);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");

            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            //int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;
            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document           
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string FinalOrderChangeRequest = updatedOrderChangeRequest2.OuterXml;

            //Replacing the value in request
            XmlDocument AgentOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager AgentorderChangeRequestXmlnsManager = new XmlNamespaceManager(AgentOrderChangeRequest.NameTable);
            AgentorderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            AgentOrderChangeRequest.LoadXml(FinalOrderChangeRequest);
            string ChildDOB = "1952-02-20";
            AgentOrderChangeRequest.SelectSingleNode("//def:PaxList//def:Pax[3]//def:Birthdate", AgentorderChangeRequestXmlnsManager).InnerText = ChildDOB;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), AgentOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Invalid age for PTC. Child passengers must be 2 to 11 years old on the date of departure";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("73"),
            $"Error Code for Order Change error message is not as expected. Expected 73 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC09_ValidatingbyRemovingGender()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");

            //return orderCreateRequest;

            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);
            string OrderCreateRequest1 = updatedOrderCreateRequest1.OuterXml;

            XmlDocument updatedOrderCreateRequest2 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderCreateRequest2.NameTable);
            orderCreateRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest2.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove2 = updatedOrderCreateRequest2.SelectSingleNode("//def:PaxList//def:Pax//def:Birthdate", orderCreateRequestXmlnsManager2);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
            string OrderCreateRequest2 = updatedOrderCreateRequest2.OuterXml;

            XmlDocument updatedOrderCreateRequest3 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager3 = new XmlNamespaceManager(updatedOrderCreateRequest3.NameTable);
            orderCreateRequestXmlnsManager3.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest3.LoadXml(OrderCreateRequest2);
            XmlNode nodeToRemove3 = updatedOrderCreateRequest3.SelectSingleNode("//def:PaxList//def:Pax//def:GenderCode", orderCreateRequestXmlnsManager3);
            nodeToRemove3.ParentNode.RemoveChild(nodeToRemove3);



            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), updatedOrderCreateRequest3.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");

            }

            //Order Retrieve            
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager1).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:PaxList//def:Pax//def:Individual//def:GenderCode", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);
            string FinalChangeRequest = updatedOrderChangeRequest3.OuterXml;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), FinalChangeRequest);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Incomplete message - data missing in query. Missing Gender for passenger";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("912"),
            $"Error Code for Order Change error message is not as expected. Expected 912 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC10_ValidatingbyRemovingDOB()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);
            string OrderCreateRequest1 = updatedOrderCreateRequest1.OuterXml;

            XmlDocument updatedOrderCreateRequest2 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderCreateRequest2.NameTable);
            orderCreateRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest2.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove2 = updatedOrderCreateRequest2.SelectSingleNode("//def:PaxList//def:Pax//def:Birthdate", orderCreateRequestXmlnsManager2);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
            string OrderCreateRequest2 = updatedOrderCreateRequest2.OuterXml;

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), OrderCreateRequest2);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document           
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager1).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:PaxList//def:Pax//def:Individual//def:Birthdate", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);
            string FinalChangeRequest = updatedOrderChangeRequest3.OuterXml;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), FinalChangeRequest);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            // var expectedResponse = "Incomplete message - data missing in query. Missing Gender for passenger";
            string expectedResponse = "Incomplete message - data missing in query. Missing Date of Birth for passenger";
            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("912"),
            $"Error Code for Order Change error message is not as expected. Expected 912 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC16_ValidatingbyRemovingDOBandGender()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);
            string OrderCreateRequest1 = updatedOrderCreateRequest1.OuterXml;

            XmlDocument updatedOrderCreateRequest2 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderCreateRequest2.NameTable);
            orderCreateRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest2.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove2 = updatedOrderCreateRequest2.SelectSingleNode("//def:PaxList//def:Pax//def:Birthdate", orderCreateRequestXmlnsManager2);
            nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
            string OrderCreateRequest2 = updatedOrderCreateRequest2.OuterXml;

            XmlDocument updatedOrderCreateRequest3 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager3 = new XmlNamespaceManager(updatedOrderCreateRequest3.NameTable);
            orderCreateRequestXmlnsManager3.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest3.LoadXml(OrderCreateRequest2);
            XmlNode nodeToRemove3 = updatedOrderCreateRequest3.SelectSingleNode("//def:PaxList//def:Pax//def:GenderCode", orderCreateRequestXmlnsManager3);
            nodeToRemove3.ParentNode.RemoveChild(nodeToRemove3);
            string OrderCreateRequest3 = updatedOrderCreateRequest3.OuterXml;


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), OrderCreateRequest3);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Retrieve response and converting to XML Document           
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager1).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:PaxList//def:Pax//def:Individual//def:Birthdate", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);
            string FinalChangeRequest = updatedOrderChangeRequest3.OuterXml;


            string OrderChangeRequest3 = updatedOrderChangeRequest3.OuterXml;
            XmlDocument updatedOrderChangeRequest4 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager3 = new XmlNamespaceManager(updatedOrderChangeRequest4.NameTable);
            orderChangeRequestXmlnsManager3.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest4.LoadXml(FinalChangeRequest);
            // updatedOrderChangeRequest4.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove4 = updatedOrderChangeRequest4.SelectSingleNode("//def:PaxList//def:Pax//def:Individual//def:GenderCode", orderChangeRequestXmlnsManager3);
            nodeRemove4.ParentNode.RemoveChild(nodeRemove4);
            string FinalChangeRequest_RemoveBoth = updatedOrderChangeRequest4.OuterXml;



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), FinalChangeRequest_RemoveBoth);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";

            XmlNodeList Count_Error = xmlDocument_1.GetElementsByTagName("Error");

            Assert.True(Count_Error.Count == 2, "2 error codes are in response");
            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");
            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;

                    Assert.True(code.Equals("912"),
                $"Error Code for Order Change error message is not as expected. Expected 912 " +
                $"but was {code}");
                }
            }

        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC11_Validating_AlreadyTicketedPNR()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 2);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");
            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44", "Untokenized");

            //return orderCreateRequest;

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve            
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            //Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;




            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), OrderChangeRequest2);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Order/PNR already ticketed";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("395"),
            $"Error Code for Order Change error message is not as expected. Expected 395 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void TC12_ValidateviaNonNDCChannelRemoveOrderID()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 2);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");

            //return orderCreateRequest;

            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");

            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;



            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:Order", orderChangeRequestXmlnsManager1);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), updatedOrderChangeRequest3.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "OrderID is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("912"),
            $"Error Code for Order Change error message is not as expected. Expected 129 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC13_ValidateviaNonNDCChannelInvalidOrderID()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 2);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);


            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");

            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");

            //return orderCreateRequest;

            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);


            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");

            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);



            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//def:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;



            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgent");


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
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            string OrderId = "VSABCDEFG";

            updatedOrderChangeRequest3.SelectSingleNode("//def:OrderID", orderChangeRequestXmlnsManager2).InnerText = OrderId;


            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), updatedOrderChangeRequest3.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());


            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "No PNR or Order ID match found";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("129"),
            $"Error Code for Order Change error message is not as expected. Expected 129 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC14_without3DS2paymentandwithGenderandDOB_FourSegments()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "4", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList DatedMarketingSegment = orderCreateResponseXmlDocument.GetElementsByTagName("DatedMarketingSegment");
            Assert.True(DatedMarketingSegment.Count > 4, $"Order not created with Conjunction ticket as DMS is not greater than 4");
            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");

            XmlNodeList Ticket = xmlDocument_1.GetElementsByTagName("Ticket");
            Assert.True(Ticket.Count >= 2);

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void TC15_AlreadyTicketed_TicketingAfterOrderChange()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfyouths: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");

            HttpResponseMessage orderChangeResponse1 = NDCTransformationEndpoints.OrderChange(tokenFixture.GetToken("orderCreate"), updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult1 = orderChangeResponse1.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult1.ToString());

            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Order/PNR already ticketed";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order Change error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("395"),
            $"Error Code for Order Change error message is not as expected. Expected 395 " +
            $"but was {code}");

        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC17_HoldBooking_ServiceId_OrderViewRs_SingleSeg_4Pax()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);


            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            // Assert Service id in Order Create response
            Assert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC18_HoldBooking_Removing_TitlePAx1_4Pax()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;
            //remove title
            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:TitleName", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);



            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), updatedOrderCreateRequest1.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC19_HoldBooking_Removing_TitlePAx2_4Pax()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;
            //remove title 2nd PTC
            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:PaxList//def:Pax[2]//def:TitleName", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);



            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), updatedOrderCreateRequest1.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC20_HoldBooking_Removing_TitlePAx3_4Pax()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;
            //remove title 3rd PTC
            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:PaxList//def:Pax[3]//def:TitleName", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);



            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), updatedOrderCreateRequest1.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC21_HoldBooking_Removing_TitlePAx4_4Pax()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;
            //remove title 4th PTC
            XmlDocument updatedOrderCreateRequest1 = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderCreateRequest1.NameTable);
            orderCreateRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest1.LoadXml(OrderCreateRequest1);
            XmlNode nodeToRemove1 = updatedOrderCreateRequest1.SelectSingleNode("//def:PaxList//def:Pax[4]//def:TitleName", orderCreateRequestXmlnsManager1);
            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);



            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(tokenFixture.GetToken("orderCreate"), updatedOrderCreateRequest1.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");
        }

        [Trait("Category", "OrderChangeAllRegression")]

        [Fact]
        public void TC22_HoldBooking_OrderCreate_OnD_Return()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV, locationThree: LocationType.TLV, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);



            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            string boookingdate = DateTime.UtcNow.ToString("");

            string TravelDateExpected = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT23:59:00Z");


            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Mismatch in Offers Count and ExpirationTTL count in Airshopping");
            Assert.True(Count_PaymentTTLAir1.Count != 0, "PaymentTimeLimitDateTime not be 0");

            List<string> OfferItems = new List<string>();
            List<string> ExpirationTTL = new List<string>();
            List<string> PaymentTTLAir1 = new List<string>();

            foreach (XmlNode ExperationTTLDateTime in Count_ExpirationTTL)
            {
                ExpirationTTL.Add(ExperationTTLDateTime.InnerText);
                Assert.True(ExperationTTLDateTime.InnerText.Has("T"), "ExperationTTLDateTime not in correct format");
            }

            //Checking whether PaymentTTLDateTime is as Expected for
            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLAir1)
            {
                PaymentTTLAir1.Add(PaymentTTLDateTime.InnerText);
                string ttlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(ttlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in AirShopping");
            }
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

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);


            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList Count_PaymentTTLOffer1 = offerPriceResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");
            Assert.True(Count_PaymentTTLOffer1.Count != 0, "PaymentTimeLimitDateTime not be 0");
            List<string> PaymentTTLOffer1 = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLOffer1)
            {
                PaymentTTLOffer1.Add(PaymentTTLDateTime.InnerText);
                offerpricettlDateTime = PaymentTTLDateTime.InnerText;
                Assert.True(offerpricettlDateTime == TravelDateExpected, "PaymentTimeLimitDateTime is not as expected in OfferPrice");
            }
            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "44");


            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            string OrderCreateRequest1 = updatedOrderCreateRequest.OuterXml;

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            //Getting the Offer Price response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order create Hold booking - response was {orderCreateResult}");

            //Asserting OnD
            Assert.True(OrderCreateAssertions.ValidateOriginDestList(OrderCreateRequest1, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            // Asserting PaxJourneyList
            Assert.True(OrderCreateAssertions.ValidatePaxJourneyList(OrderCreateRequest1, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            //Asserting ContactList
            Assert.True(OrderCreateAssertions.ValidateContactList(OrderCreateRequest1, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList Count_OrderItemID = orderCreateResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLorder = orderCreateResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLorder.Count != 0, "PaymentTimeLimitDateTime not be 0 in Ordercreate");

            List<string> OfferItemsoffer2 = new List<string>();
            List<string> PaymentTTLorder = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLorder)
            {
                PaymentTTLorder.Add(PaymentTTLDateTime.InnerText);
                ordercreatettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(offerpricettlDateTime == ordercreatettlDateTime, "Mismatch of PaymentTTLDateTime in OrderCreate");
            }

            //Order Retrieve             
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            dynamic orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResult}");

            //Asserting ContactList in order retrieve

            Assert.True(OrderCreateAssertions.ValidateContactList(OrderCreateRequest1, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            XmlNodeList Count_RetrieveOrderItemID = orderRetrieveResponseXmlDocument.GetElementsByTagName("OrderItemID");
            XmlNodeList Count_PaymentTTLretrieve = orderRetrieveResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_PaymentTTLretrieve.Count != 0, "PaymentTimeLimitDateTime not be 0 in Orderretrieve");
            List<string> PaymentTTLretrieve = new List<string>();

            foreach (XmlNode PaymentTTLDateTime in Count_PaymentTTLretrieve)
            {
                PaymentTTLretrieve.Add(PaymentTTLDateTime.InnerText);
                string orderretrievettlDateTime = PaymentTTLDateTime.InnerText;

                Assert.True(ordercreatettlDateTime == orderretrievettlDateTime, "Mismatch of PaymentTTLDateTime in OrderRetrieve");
            }

            //Generate the Order Quote request
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);


            XmlDocument updatedOrderQuoteRequest = new XmlDocument();
            XmlNamespaceManager orderQuoteRequestXmlnsManager = new XmlNamespaceManager(updatedOrderQuoteRequest.NameTable);
            orderQuoteRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            //Adding namespace manager
            XmlNamespaceManager updatedOrderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            updatedOrderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlDocument ordercreateRequestXmlDocument = new XmlDocument();
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
            string OrderChangeRequest1 = updatedOrderChangeRequest1.OuterXml;

            XmlDocument updatedOrderChangeRequest2 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager1 = new XmlNamespaceManager(updatedOrderChangeRequest2.NameTable);
            orderChangeRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest2.LoadXml(OrderChangeRequest1);
            XmlNode nodeRemove2 = updatedOrderChangeRequest2.SelectSingleNode("//def:ContactInfoList", orderChangeRequestXmlnsManager1);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);
            string OrderChangeRequest2 = updatedOrderChangeRequest2.OuterXml;

            XmlDocument updatedOrderChangeRequest3 = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager2 = new XmlNamespaceManager(updatedOrderChangeRequest3.NameTable);
            orderChangeRequestXmlnsManager2.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderChangeRequest3.LoadXml(OrderChangeRequest2);
            XmlNode nodeRemove3 = updatedOrderChangeRequest3.SelectSingleNode("//def:SecureProgram", orderChangeRequestXmlnsManager2);
            nodeRemove3.ParentNode.RemoveChild(nodeRemove3);



            XmlDocument xmlDocument_1 = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest2.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            xmlDocument_1.LoadXml(orderChangeResult.ToString());

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for OrderChange - response was {orderChangeResult}");

            //Assert OnD in Order Change

            Assert.True(OrderCreateAssertions.ValidateOriginDestList(OrderCreateRequest1, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert Phone Number in Order Change

            Assert.True(OrderCreateAssertions.ValidateContactList(OrderCreateRequest1, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}