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

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class GDSOrderChangeSeatRequestValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public GDSOrderChangeSeatRequestValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeIncorrectAgent()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "25";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "CPDVisa");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ParticipatingOrg//ct:OrgID", updatedSeatRequestXmlnsManager).InnerText = "35200056";

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("368"), $"Error code expected is 368 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("The requested Seller is not authorised to access the service "), $"Error code expected is Seller's subscription key is not valid but actual is {descText}");

            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeIncorrectOfferID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();



            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {
                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:OfferRefID", updatedSeatRequestXmlnsManager).InnerText = "62eade65-f764-4e93-85a9-d3c66b7a44c0";

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ)"), $"Error code expected is Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ) but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeIncorrectOfferItemID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:OfferItemRefID", updatedSeatRequestXmlnsManager).InnerText = "@#$%^";

                XmlNode itemNode = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]", updatedSeatRequestXmlnsManager);

                //Adding Time Field in the OutBoundJourney
                XmlElement newElement1 = updatedOrderChangeSeatRequest.CreateElement("Individual", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                //newElement1.InnerText = deptDateTime;
                itemNode.AppendChild(newElement1);

                XmlNode itemNode1 = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual", updatedSeatRequestXmlnsManager);

                XmlElement newElement2 = updatedOrderChangeSeatRequest.CreateElement("GivenName", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement2.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:GivenName", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement2);

                XmlElement newElement3 = updatedOrderChangeSeatRequest.CreateElement("Surname", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement3.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:Surname", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement3);

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ)"), $"Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ) but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeSeatAmountGreaterThanOriginal()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:TotalOfferPriceAmount", updatedSeatRequestXmlnsManager).InnerText = "500";

                XmlNode itemNode = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]", updatedSeatRequestXmlnsManager);

                //Adding Time Field in the OutBoundJourney
                XmlElement newElement1 = updatedOrderChangeSeatRequest.CreateElement("Individual", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                //newElement1.InnerText = deptDateTime;
                itemNode.AppendChild(newElement1);

                XmlNode itemNode1 = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual", updatedSeatRequestXmlnsManager);

                XmlElement newElement2 = updatedOrderChangeSeatRequest.CreateElement("GivenName", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement2.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:GivenName", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement2);

                XmlElement newElement3 = updatedOrderChangeSeatRequest.CreateElement("Surname", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement3.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:Surname", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement3);

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match"), $"Error code expected is Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeSeatAmountLesserThanOriginal()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:TotalOfferPriceAmount", updatedSeatRequestXmlnsManager).InnerText = "10";

                XmlNode itemNode = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]", updatedSeatRequestXmlnsManager);

                //Adding Time Field in the OutBoundJourney
                XmlElement newElement1 = updatedOrderChangeSeatRequest.CreateElement("Individual", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                //newElement1.InnerText = deptDateTime;
                itemNode.AppendChild(newElement1);

                XmlNode itemNode1 = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual", updatedSeatRequestXmlnsManager);

                XmlElement newElement2 = updatedOrderChangeSeatRequest.CreateElement("GivenName", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement2.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:GivenName", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement2);

                XmlElement newElement3 = updatedOrderChangeSeatRequest.CreateElement("Surname", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement3.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:Surname", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement3);

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match"), $"Error code expected is Unable to Process Advance Seat Request. Price Mismatch - Total Amount for Seats does not match but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeSeatIncorrectCurrency()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "USD", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                XmlNode itemNode = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]", updatedSeatRequestXmlnsManager);

                //Adding Time Field in the OutBoundJourney
                XmlElement newElement1 = updatedOrderChangeSeatRequest.CreateElement("Individual", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                //newElement1.InnerText = deptDateTime;
                itemNode.AppendChild(newElement1);

                XmlNode itemNode1 = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual", updatedSeatRequestXmlnsManager);

                XmlElement newElement2 = updatedOrderChangeSeatRequest.CreateElement("GivenName", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement2.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:GivenName", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement2);

                XmlElement newElement3 = updatedOrderChangeSeatRequest.CreateElement("Surname", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement3.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:Surname", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement3);

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("727"), $"Error code expected is 727 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Mismatch in Amount or Currency for the SelectedOffer (Call again OrderCreateRQ)"), $"Error code expected is Mismatch in Amount or Currency for the SelectedOffer (Call again OrderCreateRQ) but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeSeatIncorrectColumnID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:ColumnID", updatedSeatRequestXmlnsManager).InnerText = "BCDH";

                XmlNode itemNode = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]", updatedSeatRequestXmlnsManager);

                //Adding Time Field in the OutBoundJourney
                XmlElement newElement1 = updatedOrderChangeSeatRequest.CreateElement("Individual", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                //newElement1.InnerText = deptDateTime;
                itemNode.AppendChild(newElement1);

                XmlNode itemNode1 = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual", updatedSeatRequestXmlnsManager);

                XmlElement newElement2 = updatedOrderChangeSeatRequest.CreateElement("GivenName", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement2.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:GivenName", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement2);

                XmlElement newElement3 = updatedOrderChangeSeatRequest.CreateElement("Surname", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                newElement3.InnerText = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:DataLists//ct:PaxList//ct:Pax[1]//ct:Individual//ct:Surname", updatedSeatRequestXmlnsManager).InnerText;
                itemNode1.AppendChild(newElement3);

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - Seat ColumnID format passed in the request is not valid"), $"Error code expected is Unable to Process Advance Seat Request - Seat ColumnID format passed in the request is not valid but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDSSeat_OrderChangeOrdinal()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OnlineAgent");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:Ordinal", updatedSeatRequestXmlnsManager).InnerText = "";

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("368"), $"Error code expected is 368 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("The requested Seller is not authorised to access the service "), $"Error code expected is Seller's subscription key is not valid but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[2]//ct:PaxRefID", updatedSeatRequestXmlnsManager).InnerText = "ADULT_1";

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - more than one seat is requested per passenger per flight segment"), $"Error code expected is Unable to Process Advance Seat Request - more than one seat is requested per passenger per flight segmen but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:ColumnID", updatedSeatRequestXmlnsManager).InnerText = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[2]//ct:SelectedSeat//ct:ColumnID", updatedSeatRequestXmlnsManager).InnerText;

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:SeatRowNumber", updatedSeatRequestXmlnsManager).InnerText = updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[2]//ct:SelectedSeat//ct:SeatRowNumber", updatedSeatRequestXmlnsManager).InnerText;

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("290"), $"Error code expected is 290 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Equals("Unable to Process Advance Seat Request - same seat is requested for more than one passenger for the same flight segment"), $"Error code expected is Unable to Process Advance Seat Request - same seat is requested for more than one passenger for the same flight segment but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatRequest = new XmlDocument();

                updatedSeatRequest.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatRequest.CreateXmlNamespaceManager();

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Paid, paymentMethod: "OfflineAgentUntokenized");

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:SeatRowNumber", updatedSeatRequestXmlnsManager).InnerText = "124445";

                HttpResponseMessage orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml);

                string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeSeatResponse = new XmlDocument();

                updatedOrderChangeSeatResponse.LoadXml(orderChangeSeatResult);

                XmlNodeList code = updatedOrderChangeSeatResponse.GetElementsByTagName("Code");

                Assert.True(code[0].InnerText.Equals("256"), $"Error code expected is 256 but actual is {code}");

                XmlNodeList descText = updatedOrderChangeSeatResponse.GetElementsByTagName("DescText");

                Assert.True(descText[0].InnerText.Has("Invalid Seat Number for Aircraft Type"), $"Error code expected is Please complete all mandatory fields in request as per given specifications but actual is {descText}");
            }
        }

        [Trait("Category", "Local-Run")]
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSeatResponse = new XmlDocument();

                updatedSeatResponse.LoadXml(seatAvailabilityResult);

                XmlNamespaceManager updatedSeatRequestXmlnsManager = updatedSeatResponse.CreateXmlNamespaceManager();

                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                XmlNode[] offerItems = updatedSeatResponse.SelectNodes("//ct:ALaCarteOffer//ct:OfferItem", updatedSeatRequestXmlnsManager).OfType<XmlNode>().ToArray();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

                XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

                updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

                int occupiedSeatInd = 0;

                foreach (XmlElement element in offerItems)
                {
                    string occupationStatusCode = element.SelectSingleNode("//ct:SeatMap//ct:SeatRow[2]//ct:OccupationStatusCode", updatedSeatRequestXmlnsManager).InnerText;

                    if (occupationStatusCode == "O")
                    {
                        updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:ColumnID", updatedSeatRequestXmlnsManager).InnerText = element.SelectSingleNode("//ct:SeatMap//ct:SeatRow[2]//ct:ColumnID", updatedSeatRequestXmlnsManager).InnerText;

                        updatedOrderChangeSeatRequest.SelectSingleNode("//ct:ChangeOrderChoice//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem[1]//ct:SelectedSeat//ct:SeatRowNumber", updatedSeatRequestXmlnsManager).InnerText = element.SelectSingleNode("//ct:SeatMap//ct:SeatRow[2]//ct:RowNumber", updatedSeatRequestXmlnsManager).InnerText;

                        occupiedSeatInd++;

                        break;
                    }
                }


                XmlNode nodeToRemove4 = updatedOrderChangeSeatRequest.SelectSingleNode($"//ct:PaymentFunctions", updatedSeatRequestXmlnsManager);
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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingAgentName()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOrdinal()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingBookingID()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }


        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOfferID()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOfferItemID()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingPaxRefID()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingRowNumber()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingPaxList()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailabilityPSSPMissingOrderID()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
        }


        [Trait("Category", "Local-Run")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOwnerCode()
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

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            string[] bookingIds = GDSPNRs.BookingIds;

            foreach (string bookingID in bookingIds)
            {

                string orderID = "VS932" + bookingID + "24";
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.GDSOrderRetrieves(bookingID, orderID);
                Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                string orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: "GBP", totalAmount: "0.00", bookingId: bookingID, orderId: orderID, RequestHelper.SeatPriceType.Free);

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
}
