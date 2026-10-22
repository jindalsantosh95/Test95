using System.Xml;
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
    [Collection("SeatAvailability")]
    public class GDSSeatResponseValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        

        public GDSSeatResponseValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "GDSRegression")]

        [Fact]
        public void GDS_Seat_EM96L8()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.MultiCity,
                locationOne: LocationType.DEL, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR, locationFive: LocationType.LHR, locationSix: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "EM96L8";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_FFV9TK()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.ATL, locationTwo: LocationType.BLR, locationThree: LocationType.BLR, locationFour: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "FFV9TK";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_DXQTLY()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.MultiCity,
                locationOne: LocationType.BOS, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, locationFive: LocationType.LHR, locationSix: LocationType.IAD,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "DXQTLY";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_CPVE32()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.DEL, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "CPVE32";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_DU5763()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
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

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "DU5763";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_EML26C()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.GOI, locationThree: LocationType.GOI, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "CPVE32";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_DBXG7I()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
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

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "DBXG7I";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_ENGC69()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "ENGC69";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_CHY2QN()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.MultiCity,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "CHY2QN";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_CQRT6C()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "2", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "CQRT6C";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            //Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            ///Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_DDQUHX()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", numberOfadults: 2, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "DDQUHX";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_DU8MKX()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", numberOfadults: 2, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "DU8MKX";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_EMOD4V()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "EMOD4V";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }

        [Trait("Category", "GDSRegression")]
        [Fact]
        public void GDS_CZX6IT()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingID = "CZX6IT";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);
            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(System.Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: bookingID);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatrequest Response, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seat Request sent was {seatAvailabilityResponse}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatResponse = new XmlDocument();

            XmlNamespaceManager updatedSeatRequestXmlnsManager = new XmlNamespaceManager(updatedSeatResponse.NameTable);

            updatedSeatRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedSeatResponse.LoadXml(seatAvailabilityResult);

            int seatmap = updatedSeatResponse.SelectNodes("//def:SeatMap", updatedSeatRequestXmlnsManager).Count;

            Assert.False(seatmap < 1, $"Seat map is not returned Actual Response{seatAvailabilityResult}");
        }
    }
}
