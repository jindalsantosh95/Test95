using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using VAA.EAPI.Automation.NDC.Tests.OrderChange;

namespace VAA.EAPI.Automation.NDC.Tests.V2_Validations
{
    [Collection("OrderChange")]
    public class OrderChangeErrorCaseValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public OrderChangeErrorCaseValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        //Run this file only in V2 environment version

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeAPIs_InvalidPaxlist_TC01()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(300).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdKayakUsa, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCodeOne: "5", numberOfadults: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", airShoppingRequestXmlnsManager).InnerText;

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ContactInfo//ct:EmailAddress//ct:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //Order Retrieve 
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

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

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, newCabinTypeCodeOne: "5", deptDate: reshopJourney1Date);

                //Post Order Reshop request
                HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from orderReshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()}" + $"OrderReshop Request sent was {orderReshopRequest}");

                //Getting the Order Reshop response and converting to XML Document
                orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteRequest}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            // Namespace manager setup
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            // Locate the PaxList node
            XmlNode paxListNode = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:PaxList", orderChangeRequestXmlnsManager);

            // Remove the entire PaxList node
            if (paxListNode != null && paxListNode.ParentNode != null)
            {
                paxListNode.ParentNode.RemoveChild(paxListNode);
            }

            // (Optional) Remove other nodes if necessary, for example, PaymentFunctions or ContactInfoList
            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:PaymentFunctions", orderChangeRequestXmlnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeToRemove1?.ParentNode.RemoveChild(nodeToRemove1);

            // Now the PaxList node has been removed from the updatedOrderChangeRequest

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderChangeValidators.ValidateUniqueOrderItemId(updatedOrderChangeResponse, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("143"), $"Expected 143 but actual was {code}");

            Assert.True(descText.Equals("No passenger/ traveler list specified or empty"), $"Expected : No passenger/ traveler list specified or empty. but actual was {descText}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeAPIs_NoAdultInPaxlist_TC02()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(300).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdKayakUsa, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", airShoppingRequestXmlnsManager).InnerText;

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ContactInfo//ct:EmailAddress//ct:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //Order Retrieve 
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = 2;

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

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, newCabinTypeCodeOne: "5", deptDate: reshopJourney1Date);

                //Post Order Reshop request
                HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from orderReshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()}" + $"OrderReshop Request sent was {orderReshopRequest}");

                //Getting the Order Reshop response and converting to XML Document
                orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteRequest}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            // Generate the initial order change request and load it into an XmlDocument

            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            // Find the <SelectedOfferItem> element with a <PaxRefID> that contains "ADULT"
            XmlNode selectedOfferItemWithAdultPax = updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer/ct:SelectedOfferItem[ct:PaxRefID[contains(text(), 'ADULT')]]", orderChangeRequestXmlnsManager);

            // Remove the <SelectedOfferItem> with <PaxRefID> containing "ADULT" if it exists
            if (selectedOfferItemWithAdultPax != null)
            {
                selectedOfferItemWithAdultPax.ParentNode.RemoveChild(selectedOfferItemWithAdultPax);
            }

            // Find the <Pax> element with a <PTC> that contains "ADT"
            XmlNode paxWithADT = updatedOrderChangeRequest.SelectSingleNode("//ct:PaxList/ct:Pax[ct:PTC[contains(text(), 'ADT')]]", orderChangeRequestXmlnsManager);

            // Remove the <Pax> element with <PTC> containing "ADT" if it exists
            if (paxWithADT != null)
            {
                paxWithADT.ParentNode.RemoveChild(paxWithADT);
            }

            // (Optional) Remove other nodes if necessary, for example, PaymentFunctions or ContactInfoList
            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:PaymentFunctions", orderChangeRequestXmlnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeToRemove1?.ParentNode.RemoveChild(nodeToRemove1);

            // Now the PaxList node has been removed from the updatedOrderChangeRequest

            XmlDocument updatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderChangeValidators.ValidateUniqueOrderItemId(updatedOrderChangeResponse, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("143"), $"Expected 143 but actual was {code}");

            Assert.True(descText.Equals("Invalid or Ineligible Passenger Type Code"), $"Expected : Invalid or Ineligible Passenger Type Code. but actual was {descText}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeAPIs_NumberOfInfantGreaterThanOtherPax_TC03()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(300).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdKayakUsa, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfinfants: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", airShoppingRequestXmlnsManager).InnerText;

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ContactInfo//ct:EmailAddress//ct:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //Order Retrieve 
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = 2;

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

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, newCabinTypeCodeOne: "5", deptDate: reshopJourney1Date);

                //Post Order Reshop request
                HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from orderReshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()}" + $"OrderReshop Request sent was {orderReshopRequest}");

                //Getting the Order Reshop response and converting to XML Document
                orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteRequest}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            // Assuming orderChangeRequest is generated and updatedOrderChangeRequest is loaded with XML.
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            // Locate the SelectedOfferItem node for the first infant (INFANT_1)
            XmlNode selectedOfferItemNode = updatedOrderChangeRequest.SelectSingleNode("//ct:SelectedOfferItem[ct:PaxRefID='INFANT_1']", orderChangeRequestXmlnsManager);

            // Add second PaxRefID for INFANT_2 under the same SelectedOfferItem
            if (selectedOfferItemNode != null)
            {
                XmlElement newPaxRefIDNode = updatedOrderChangeRequest.CreateElement("PaxRefID", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newPaxRefIDNode.InnerText = "INFANT_2";
                selectedOfferItemNode.AppendChild(newPaxRefIDNode);
            }

            // Locate the PaxList node
            XmlNode paxListNode = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:PaxList", orderChangeRequestXmlnsManager);

            // Add a new Pax node for INFANT_2 to PaxList
            if (paxListNode != null)
            {
                XmlElement newPaxNode = updatedOrderChangeRequest.CreateElement("Pax", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                XmlElement paxIDNode = updatedOrderChangeRequest.CreateElement("PaxID", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                paxIDNode.InnerText = "INFANT_2";

                XmlElement ptcNode = updatedOrderChangeRequest.CreateElement("PTC", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                ptcNode.InnerText = "INF";

                newPaxNode.AppendChild(paxIDNode);
                newPaxNode.AppendChild(ptcNode);

                paxListNode.AppendChild(newPaxNode);
            }

            // Remove PaymentFunctions node
            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:PaymentFunctions", orderChangeRequestXmlnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            // Remove ContactInfoList node
            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeToRemove1?.ParentNode.RemoveChild(nodeToRemove1);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderChangeValidators.ValidateUniqueOrderItemId(updatedOrderChangeResponse, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("324"), $"Expected 324 but actual was {code}");

            Assert.True(descText.Equals("Number of infants exceeds maximum allowed per adult passenger"), $"Expected : Number of infants exceeds maximum allowed per adult passenger. but actual was {descText}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeAPIs_MaximumNumberOfPax_TC04()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(300).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdKayakUsa, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", airShoppingRequestXmlnsManager).InnerText;

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ContactInfo//ct:EmailAddress//ct:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //Order Retrieve 
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = 2;

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

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.JFK, newCabinTypeCodeOne: "5", deptDate: reshopJourney1Date);

                //Post Order Reshop request
                HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from orderReshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()}" + $"OrderReshop Request sent was {orderReshopRequest}");

                //Getting the Order Reshop response and converting to XML Document
                orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteRequest}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            // Load the XML into the XmlDocument object
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);
            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            // Namespace manager setup
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            // Locate the PaxList node and clear existing entries if necessary
            XmlNode paxListNode = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:PaxList", orderChangeRequestXmlnsManager);

            // Clear existing PaxList entries if it exists
            if (paxListNode != null)
            {
                // Remove existing Pax nodes
                while (paxListNode.HasChildNodes)
                {
                    paxListNode.RemoveChild(paxListNode.FirstChild);
                }
            }
            else
            {
                // If PaxList doesn't exist, create a new one
                paxListNode = updatedOrderChangeRequest.CreateElement("PaxList", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists", orderChangeRequestXmlnsManager)?.AppendChild(paxListNode);
            }

            // Add 5 Adults
            for (int i = 1; i <= 5; i++)
            {
                XmlElement adultNode = updatedOrderChangeRequest.CreateElement("Pax", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                XmlElement paxIDNode = updatedOrderChangeRequest.CreateElement("PaxID", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                paxIDNode.InnerText = $"ADULT_{i}";

                XmlElement ptcNode = updatedOrderChangeRequest.CreateElement("PTC", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                ptcNode.InnerText = "ADT";

                adultNode.AppendChild(paxIDNode);
                adultNode.AppendChild(ptcNode);

                paxListNode.AppendChild(adultNode);
            }

            // Add 5 Children
            for (int i = 1; i <= 5; i++)
            {
                XmlElement childNode = updatedOrderChangeRequest.CreateElement("Pax", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                XmlElement paxIDNode = updatedOrderChangeRequest.CreateElement("PaxID", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                paxIDNode.InnerText = $"CHILD_{i}";

                XmlElement ptcNode = updatedOrderChangeRequest.CreateElement("PTC", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                ptcNode.InnerText = "CHD";

                childNode.AppendChild(paxIDNode);
                childNode.AppendChild(ptcNode);

                paxListNode.AppendChild(childNode);
            }

            // Now updatedOrderChangeRequest contains the updated PaxList with 5 adults and 5 children

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:PaymentFunctions", orderChangeRequestXmlnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeToRemove1?.ParentNode.RemoveChild(nodeToRemove1);

            // Now the PaxList node has been removed from the updatedOrderChangeRequest

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderChangeValidators.ValidateUniqueOrderItemId(updatedOrderChangeResponse, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Error//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("68"), $"Expected 68 but actual was {code}");

            Assert.True(descText.Equals("Max accepted non-infant passengers is 9"), $"Expected : Max accepted non-infant passengers is 9. but actual was {descText}");
        }

        //HoldBooking

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeHoldBooking_UpdatingIndividualTag_atOrderChange()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
               cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            XmlNode RemovePaymentFunctionNode = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", orderCreateRequestXmlnsManager);
            RemovePaymentFunctionNode.ParentNode.RemoveChild(RemovePaymentFunctionNode);

            XmlNode RemoveDOB = updatedOrderCreateRequest.SelectSingleNode("//ct:PaxList//ct:Pax//ct:Individual//ct:Birthdate", orderCreateRequestXmlnsManager);
            RemoveDOB.ParentNode.RemoveChild(RemoveDOB);

            XmlNode RemoveGender = updatedOrderCreateRequest.SelectSingleNode("//ct:PaxList//ct:Pax//ct:Individual//ct:GenderCode", orderCreateRequestXmlnsManager);
            RemoveGender.ParentNode.RemoveChild(RemoveGender);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = updatedOrderRetrieveRequest.CreateXmlNamespaceManager();

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//ct:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//ct:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized");

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            XmlNode nodeRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest.SelectSingleNode("//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            // Get all <Pax> nodes
            XmlNodeList paxNodes = updatedOrderChangeRequest.SelectNodes("//ct:PaxList//ct:Pax", orderChangeRequestXmlnsManager);

            foreach (XmlNode paxNode in paxNodes)
            {
                // Create the <Individual> element
                XmlElement individualElement = updatedOrderChangeRequest.CreateElement("Individual", paxNode.NamespaceURI);

                // Add <Birthdate> to <Individual>
                XmlElement birthdateElement = updatedOrderChangeRequest.CreateElement("Birthdate", paxNode.NamespaceURI);
                birthdateElement.InnerText = "2000-11-20";
                individualElement.AppendChild(birthdateElement);

                // Add <GenderCode> to <Individual>
                XmlElement genderCodeElement = updatedOrderChangeRequest.CreateElement("GenderCode", paxNode.NamespaceURI);
                genderCodeElement.InnerText = "U";
                individualElement.AppendChild(genderCodeElement);

                // Add <GivenName> to <Individual>
                XmlElement givenNameElement = updatedOrderChangeRequest.CreateElement("GivenName", paxNode.NamespaceURI);
                givenNameElement.InnerText = "DAVID";
                individualElement.AppendChild(givenNameElement);

                // Add <SurName> to <Individual>
                XmlElement surNameElement = updatedOrderChangeRequest.CreateElement("Surname", paxNode.NamespaceURI);
                surNameElement.InnerText = "SMITH";
                individualElement.AppendChild(surNameElement);

                // Insert <Individual> as the first child of <Pax>
                paxNode.PrependChild(individualElement);
            }

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Warning//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Warning//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeHoldBooking_OverwritingIndividualTag_atOrderChange()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
               cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            XmlNode RemovePaymentFunctionNode = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", orderCreateRequestXmlnsManager);
            RemovePaymentFunctionNode.ParentNode.RemoveChild(RemovePaymentFunctionNode);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);
            XmlNamespaceManager orderRetrieveRequestXmlnsManager = updatedOrderRetrieveRequest.CreateXmlNamespaceManager();

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//ct:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//ct:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized");

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            XmlNode nodeRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest.SelectSingleNode("//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            // Get all <Pax> nodes
            XmlNodeList paxNodes = updatedOrderChangeRequest.SelectNodes("//ct:PaxList//ct:Pax", orderChangeRequestXmlnsManager);

            foreach (XmlNode paxNode in paxNodes)
            {
                // Create the <Individual> element
                XmlElement individualElement = updatedOrderChangeRequest.CreateElement("Individual", paxNode.NamespaceURI);

                // Add <Birthdate> to <Individual>
                XmlElement birthdateElement = updatedOrderChangeRequest.CreateElement("Birthdate", paxNode.NamespaceURI);
                birthdateElement.InnerText = "2000-11-20";
                individualElement.AppendChild(birthdateElement);

                // Add <GenderCode> to <Individual>
                XmlElement genderCodeElement = updatedOrderChangeRequest.CreateElement("GenderCode", paxNode.NamespaceURI);
                genderCodeElement.InnerText = "U";
                individualElement.AppendChild(genderCodeElement);

                // Add <GivenName> to <Individual>
                XmlElement givenNameElement = updatedOrderChangeRequest.CreateElement("GivenName", paxNode.NamespaceURI);
                givenNameElement.InnerText = "DAVID";
                individualElement.AppendChild(givenNameElement);

                // Add <SurName> to <Individual>
                XmlElement surNameElement = updatedOrderChangeRequest.CreateElement("Surname", paxNode.NamespaceURI);
                surNameElement.InnerText = "SMITH";
                individualElement.AppendChild(surNameElement);

                // Insert <Individual> as the first child of <Pax>
                paxNode.PrependChild(individualElement);
            }

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Warning//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Warning//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderChangeHoldBooking_InavlidIndividualTag_atOrderChange()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "1", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
               cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            XmlNode RemovePaymentFunctionNode = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", orderCreateRequestXmlnsManager);
            RemovePaymentFunctionNode.ParentNode.RemoveChild(RemovePaymentFunctionNode);

            XmlNode RemoveDOB = updatedOrderCreateRequest.SelectSingleNode("//ct:PaxList//ct:Pax//ct:Individual//ct:Birthdate", orderCreateRequestXmlnsManager);
            RemoveDOB.ParentNode.RemoveChild(RemoveDOB);

            XmlNode RemoveGender = updatedOrderCreateRequest.SelectSingleNode("//ct:PaxList//ct:Pax//ct:Individual//ct:GenderCode", orderCreateRequestXmlnsManager);
            RemoveGender.ParentNode.RemoveChild(RemoveGender);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateResponseXmlnsManager).InnerText;

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = updatedOrderRetrieveRequest.CreateXmlNamespaceManager();

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuoteResponse, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteResponse}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderQuoteBookingId = orderQuoteResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(orderQuoteBookingId == orderId, "Order Quote BookingId is invalid.");

            string noChangeIndex = orderQuoteResponseXmlDocument.SelectSingleNode("//ct:NoChangeInd", orderCreateResponseXmlnsManager).InnerText;
            Assert.True(noChangeIndex == "true", "Order Quote NoChangeInd is invalid.");

            //Getting the Order Quote response and converting to XML Document            
            XmlDocument ordercreateRequestXmlDocument = new();
            ordercreateRequestXmlDocument.LoadXml(orderCreateRequest);

            string Amount = ordercreateRequestXmlDocument.SelectSingleNode("//ct:TotalOfferPriceAmount", orderCreateRequestXmlnsManager).InnerText;

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteResult, currencyCode, Amount, bookingId, orderId, "OnlineAgentUntokenized");

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:ChangeOrderChoice", orderChangeRequestXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            XmlNode nodeRemove2 = updatedOrderChangeRequest.SelectSingleNode("//ct:ContactInfoList", orderChangeRequestXmlnsManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            string x = updatedOrderChangeRequest.OuterXml;
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            string code = updatedOrderChangeResponse.SelectSingleNode("//ct:Warning//ct:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderChangeResponse.SelectSingleNode("//ct:Warning//ct:DescText", orderCreateResponseXmlnsManager).InnerText;

        }

    }
}
