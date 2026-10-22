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
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class SeatAvailabilityPostSaleRequestValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public SeatAvailabilityPostSaleRequestValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeIncorrectAgent()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:DistributionChainLink//def:ParticipatingOrg//def:OrgID", orderCreateResponseXmlnsManager).InnerText = "14369880";

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent));

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("368"), $"Error code expected is 368 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("The requested Seller is not authorised to access the service"), $"Error code expected is Seller's subscription key is not valid but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeIncorrectTMC()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdAmericanExpress,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date, corporate: "9000000000387");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode().EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:DistributionChainLink//def:ParticipatingOrg//def:OrgID", orderCreateResponseXmlnsManager).InnerText = "00003333";

            XmlNode nodeRemove1 = updatedOrderChangeSeatRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateResponseXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdKayakUsa));

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("368"), $"Error code expected is 368 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("The requested Seller is not authorised to access the service"), $"Error code expected is Seller's subscription key is not valid but actual is {descText[0].InnerText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeIncorrectOfferID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdAmericanExpress,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date, corporate: "9000000000387");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:OfferRefID", orderCreateResponseXmlnsManager).InnerText = "0e15719b-779e-4cc4-bf90-20f8e62a8268";

            XmlNode nodeRemove1 = updatedOrderChangeSeatRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateResponseXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid Request/CreateOrder/SelectedOffer/OfferID value is entered "), $"Error code expected is Invalid OfferItemID for the SelectedOffer/OfferID but actual is {descText}");


        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeIncorrectOfferItemID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdAmericanExpress,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date, corporate: "9000000000387");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "USD", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:OfferItemRefID", orderCreateResponseXmlnsManager).InnerText = "a399bd53-e870-48e3-b26a-6e9fde245f06_FA|1";

            XmlNode nodeRemove1 = updatedOrderChangeSeatRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateResponseXmlnsManager);
            nodeRemove1.ParentNode.RemoveChild(nodeRemove1);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ)"), $"Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ) but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatAmountGreaterThanOriginal()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "USD", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:TotalOfferPriceAmount", orderCreateResponseXmlnsManager).InnerText = "500";

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match"), $"Error code expected is Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatAmountLessThanOriginal()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "USD", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:TotalOfferPriceAmount", orderCreateResponseXmlnsManager).InnerText = "10";

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match"), $"Error code expected is Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatIncorrectCurrency()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "USD", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("727"), $"Error code expected is 727 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Mismatch in Amount or Currency for the SelectedOffer (Call again OrderCreateRQ)"), $"Error code expected is Mismatch in Amount or Currency for the SelectedOffer (Call again OrderCreateRQ) but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatIncorrectColumnID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:SelectedSeat//def:ColumnID", orderCreateResponseXmlnsManager).InnerText = "BCDH";

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - Seat ColumnID format passed in the request is not valid"), $"Error code expected is Unable to Process Advance Seat Request - Seat ColumnID format passed in the request is not valid but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatMorethan1SeatRequestedForPax()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "4", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[2]//def:PaxRefID", orderCreateResponseXmlnsManager).InnerText = "ADULT_1";

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - more than one seat is requested per passenger per flight segment"), $"Error code expected is Unable to Process Advance Seat Request - more than one seat is requested per passenger per flight segmen but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatSameSeatRequestedTwice()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.TLV,
                cabinTypeCodeOne: "5", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:SelectedSeat//def:ColumnID", orderCreateResponseXmlnsManager).InnerText = updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[2]//def:SelectedSeat//def:ColumnID", orderCreateResponseXmlnsManager).InnerText;

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:SelectedSeat//def:SeatRowNumber", orderCreateResponseXmlnsManager).InnerText = updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[2]//def:SelectedSeat//def:SeatRowNumber", orderCreateResponseXmlnsManager).InnerText;

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - same seat is requested for more than one passenger for the same flight segment"), $"Error code expected is Unable to Process Advance Seat Request - same seat is requested for more than one passenger for the same flight segment but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatIncorrectRowNumber()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatRequest = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatRequest.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:SelectedSeat//def:SeatRowNumber", orderCreateResponseXmlnsManager).InnerText = "123445";

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("256"), $"Error code expected is 256 but actual is {code}");

            XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Has("Invalid Seat Number for Aircraft Type"), $"Error code expected is Please complete all mandatory fields in request as per given specifications but actual is {descText}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_OrderChangeSeatRequestOccupiedSeats()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB,
                cabinTypeCodeOne: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

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
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "UnsecureUntokenized");

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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);
            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            XmlNode[] offerItems = updatedSeatResponse.SelectNodes("//def:ALaCarteOffer//def:OfferItem", updatedSeatRequestXmlnsManager).OfType<XmlNode>().ToArray();

            string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            int occupiedSeatInd = 0;

            foreach (XmlElement element in offerItems)
            {
                string occupationStatusCode = element.SelectSingleNode("//def:SeatMap//def:SeatRow[2]//def:OccupationStatusCode", updatedSeatRequestXmlnsManager).InnerText;

                if (occupationStatusCode == "O")
                {
                    updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:SelectedSeat//def:ColumnID", orderCreateResponseXmlnsManager).InnerText = element.SelectSingleNode("//def:SeatMap//def:SeatRow[2]//def:ColumnID", updatedSeatRequestXmlnsManager).InnerText;

                    updatedOrderChangeSeatRequest.SelectSingleNode("//def:ChangeOrderChoice//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem[1]//def:SelectedSeat//def:SeatRowNumber", orderCreateResponseXmlnsManager).InnerText = element.SelectSingleNode("//def:SeatMap//def:SeatRow[2]//def:RowNumber", updatedSeatRequestXmlnsManager).InnerText;

                    occupiedSeatInd++;

                    break;
                }
            }


            XmlNode nodeToRemove4 = updatedOrderChangeSeatRequest.SelectSingleNode($"//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove4.ParentNode.RemoveChild(nodeToRemove4);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

            updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

            if (occupiedSeatInd > 0)
            {
                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - the requested seat(s) have already been reserved"), $"Error code expected is Unable to Process Advance Seat Request - the requested seat(s) have already been reserved but actual is {descText}");
            }
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingAgentName()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ParticipatingOrg//ct:Name", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 878";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOrdinal()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DistributionChainLink//ct:Ordinal", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 353";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingBookingID()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:BookingRef//ct:BookingID", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'BookingRef' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has incomplete content. List of possible elements expected: 'BookingEntity, BookingID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 625";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOfferID()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();
            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:OfferRefID", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OfferRefID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 815";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOfferItemID()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:OfferItemRefID", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 901";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingPaxRefID()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:PaxRefID", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 901";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingRowNumber()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:SeatRowNumber", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 901";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingPaxList()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 901";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOrderID()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:Order//ct:OrderID", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 901";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOwnerCode()
        {
            string orderChangeSeatRequest = Core.Extensions.FileExtensions.ReadFile("OrderChangeSeatRequest", "xml");
            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedOrderChangeSeatRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:Order//ct:OwnerCode", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic OrderchangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeSeatResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderchangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderchangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedPricedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'TotalOfferPriceAmount' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 901";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }
    }
}