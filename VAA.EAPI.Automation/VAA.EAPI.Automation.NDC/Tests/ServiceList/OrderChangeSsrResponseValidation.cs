using System.Globalization;
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

namespace VAA.EAPI.Automation.NDC.Tests.ServiceList
{
    public class OrderChangeSsrResponseValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public OrderChangeSsrResponseValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void OrderChangeSeatService()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

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

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderItem[1]//def:Service//def:BookingRef//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml);

            Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "CPDVisa");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            //get the service order change request
            string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRequest, serviceListResult, currencyCode, bookingID, orderID, paymentMethod: "CPDVisa", serviceCount: 1);

            XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

            updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

            //get the service nodes
            XmlNode selectServiceNodes = updatedorderChangeServiceRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer", orderCreateRequestXmlnsManager);

            XmlNode cloneNode = updatedOrderChangeSeatRequest.ImportNode(selectServiceNodes, true);

            XmlNode selectNode = updatedOrderChangeSeatRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer", orderCreateRequestXmlnsManager);

            selectNode.ParentNode.AppendChild(cloneNode);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderChangeSeatResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void OrderChangeServiceEvenExchange()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(150).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 150;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, deptDate: journey1Date,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

            string originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria//def:Date", airShoppingRequestXmlnsManager).InnerText;

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "testemail@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            //Order Retrieve 
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
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseXmlnsManager = new XmlNamespaceManager(orderRetrieveResponseXmlDocument.NameTable);
            orderRetrieveResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseXmlnsManager).InnerText;

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

                DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime newDate1 = date1.AddDays(counter);

                string reshopJourney2Date = newDate1.ToString("yyyy-MM-dd");
                //Generating the Order Reshop Request

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID,
                    JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, newCabinTypeCodeOne: "5", newCabinTypeCodeTwo: "5", deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date, flighttype: "Direct");

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

            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId, apiInfo: "apiInfo",orderChangePayment:"CPDVisa");

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderChangeRequestXmlnsManager = new XmlNamespaceManager(updatedOrderChangeRequest.NameTable);
            orderChangeRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            XmlNode nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//def:DataLists//def:ContactInfoList", orderChangeRequestXmlnsManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            //Service List request
            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderId);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml);

            Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRequest, serviceListResult, currencyCode, bookingId, orderId, "OnlineAgentUntokenized", serviceCount: 1);

            XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

            updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

            //get the service nodes
            XmlNode selectServiceNodes = updatedorderChangeServiceRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer", orderCreateRequestXmlnsManager);

            XmlNode cloneNode = updatedOrderChangeRequest.ImportNode(selectServiceNodes, true);

            XmlNode selectNode = updatedOrderChangeRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer", orderCreateRequestXmlnsManager);

            selectNode.ParentNode.AppendChild(cloneNode);

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml).EnsureIsSuccessStatusCode();

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderChangeResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void OrderChangeServiceConfirmHoldBooking()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

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

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderItem[1]//def:Service//def:BookingRef//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml);

            Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRequest, serviceListResult, currencyCode, bookingID, orderID, serviceCount: 1, paymentMethod: "CPDMaster",selectedService:"FPML");

            XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

            updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

            XmlNode cloneNode = updatedorderChangeServiceRequest.ImportNode(nodeToRemove, true);

            XmlNode selectNode = updatedorderChangeServiceRequest.SelectSingleNode("//def:Order", orderCreateRequestXmlnsManager);

            selectNode.ParentNode.AppendChild(cloneNode);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureIsSuccessStatusCode();

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            int warning = updatedOrderChangeSeatResponse.SelectNodes("//def:Warning[def:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and def:Code='182' and def:LangCode='EN' and def:TagText='SSR Pending' and def:TypeCode='Warning']", orderCreateResponseXmlnsManager).Count;

            ValidationResultHelper result = new ValidationResultHelper();

            int serviceOrderItemCount = updatedOrderChangeSeatResponse.SelectNodes("//def:OrderItem[not(.//def:FareDetail)]", orderCreateResponseXmlnsManager).Count;

            Assert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

            //retrieve the pnr
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            string orderRetrieveResult = "";

            HttpResponseMessage orderRetrieveResponse = null;

            RetryHelper.RunWithRetry(() =>
            {
                orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderRetrieveResult, serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            }, 5);

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);
            
            dynamic orderHistoryRQ = RequestHelper.GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
            dynamic orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void OrderChangeService()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2,numberOfchildren:0,numberOfinfants:1, deptDate: journey1Date);

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa");

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

            string orderID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderID", orderCreateResponseXmlnsManager).InnerText;

            string bookingID = updatedOrderCreateResponse.SelectSingleNode("//def:Order//def:OrderItem[1]//def:Service//def:BookingRef//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml);

            Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRequest, serviceListResult, currencyCode, bookingID, orderID, serviceCount: 1,selectedService:"AVML");

            XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

            updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

            HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureIsSuccessStatusCode();

            string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

            updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

            Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderChangeServiceResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

    }
}
