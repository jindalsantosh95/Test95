using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests
{
    public class EndToEndBaseStructure : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public EndToEndBaseStructure(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }
        //Example test through to creating the Order with the ouput being the PTC generated

        [Fact]
        public void NDC_OrderCreateThenOrderChangeExample()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "91281035",
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Find offer based on Cabin Type and Flight Scenario
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.NonVirgin);

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
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/OfferPriceRS");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0);

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount);

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(orderCreateRequest);

            //Getting the Order Create response and converting to XML Document
            string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Create - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/OrderViewRS");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;


            //Order Retrieve 
            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);
            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderCreateResult}");

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/OrderViewRS");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;


            int countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//def:OrderItemID", orderRetrieveResponseXmlnsManager).Count;

            //Generating the Order Reshop Request
            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.TLV, newCabinTypeCodeOne: "5");

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Reshop - response was {orderReshopResult}");

            //Find offer based on Even Exchange
            Dictionary<string, string> reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");

            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            //Generate the Order Quote request based on the Order Reshop response data
            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Quote - response was {orderQuoteResult}");

            Console.WriteLine("Order quote result is " + orderQuoteResult);
            Console.WriteLine("Reshop Offer id is " + reshopOfferRefId);

            //Generating the Order Change Request
            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            //Post Order Change request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequest);

            //Getting the Order Change response and converting to XML Document
            string orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Change - response was {orderChangeResult}");

        }
    }
}