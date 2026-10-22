using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderChange.ErrorMessageValidation
{
    public class OrderChangePaxIdValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        

        public OrderChangePaxIdValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenPaxlistIsNotPresentInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));
            
            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();
            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:PaxList", orderChangeRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlDocument updatedorderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());
            
            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();
            
            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1651";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenPaxlistIsNotProprelyDefinedInDatalist()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();
            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:PaxList//ct:Pax[2]", orderChangeRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlDocument UpdatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());
            
            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1651";
           
            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenPaxlistIsNotProprelyDefinedInSelectedOfferItem()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();
            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//ct:SelectedPricedOffer//ct:SelectedOfferItem[2]/ct:PaxRefID", orderChangeRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            
            XmlDocument updatedorderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());
            
            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has incomplete content. List of possible elements expected: 'PaxRefID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1216";
            
            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenPaxlistIsMismatchWithSelectedOfferItemInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();
            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            string paxRefId_1 = updatedOrderChangeRequest.SelectSingleNode("//ct:SelectedPricedOffer/ct:SelectedOfferItem[2]/ct:PaxRefID", orderChangeRequestXmlnsManager).InnerText;
            string paxRefId_2 = updatedOrderChangeRequest.SelectSingleNode("//ct:SelectedPricedOffer/ct:SelectedOfferItem[3]/ct:PaxRefID", orderChangeRequestXmlnsManager).InnerText;

            //Replacing the values 
            updatedOrderChangeRequest.SelectSingleNode("//ct:SelectedPricedOffer/ct:SelectedOfferItem[3]/ct:PaxRefID", orderChangeRequestXmlnsManager).InnerText = paxRefId_1;
            updatedOrderChangeRequest.SelectSingleNode("//ct:SelectedPricedOffer/ct:SelectedOfferItem[2]/ct:PaxRefID", orderChangeRequestXmlnsManager).InnerText = paxRefId_2;

            XmlDocument UpdatedOrderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1651";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateSuccessResponseWhenPtcAndIndividualRemovedFromDatalist()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            XmlNamespaceManager orderChangeRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();
            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            XmlNodeList nodesToRemove_1 = updatedOrderChangeRequest.SelectNodes("//ct:PaxList//ct:Pax//ct:PTC", orderChangeRequestXmlnsManager);

            foreach (XmlNode nodeToRemove in nodesToRemove_1)
            {
                XmlNode parent = nodeToRemove.ParentNode;
                parent.RemoveChild(nodeToRemove);
            }

            XmlNodeList nodesToRemove_2 = updatedOrderChangeRequest.SelectNodes("//ct:PaxList//ct:Pax//ct:Individual", orderChangeRequestXmlnsManager);

            foreach (XmlNode nodeToRemove in nodesToRemove_2)
            {
                XmlNode parent = nodeToRemove.ParentNode;
                parent.RemoveChild(nodeToRemove);
            }

            XmlDocument updatedorderChangeResponse = new XmlDocument();
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1651";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
       
        }
            private string NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, deptDate: journey1Date,
                cabinTypeCodeOne: "5", numberOfadults: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", airShoppingRequestXmlnsManager).InnerText;

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

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
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseorderChangeRequestnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseorderChangeRequestnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNamespaceManager orderCreateRequestorderChangeRequestnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ContactInfo//ct:EmailAddress//ct:EmailAddressText", orderCreateRequestorderChangeRequestnsManager).InnerText = "testemail@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            XmlNamespaceManager orderCreateorderChangeRequestnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//ct:BookingID", orderCreateorderChangeRequestnsManager).InnerText;

            //Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

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
            XmlNamespaceManager orderRetrieveResponseorderChangeRequestnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", orderRetrieveResponseorderChangeRequestnsManager).InnerText;

            int countOrderItemID = 1;

            int counter = 0;
            string orderReshopRequest = "";
            string orderReshopResult = "";
            IDictionary<string, string> reshopOfferDetails = new Dictionary<string, string>();

            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            do
            {

                //DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //DateTime newDate = date.AddDays(counter);

                //string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");

                //Generating the Order Reshop Request

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, newCabinTypeCodeOne: "5",  flighttype: "Direct");

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

            //Post Order Create request
            return orderChangeRequest;
        }

    }
}
