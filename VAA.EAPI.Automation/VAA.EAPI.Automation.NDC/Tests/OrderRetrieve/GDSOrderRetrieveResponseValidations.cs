using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderRetrieve
{
    public class GDSOrderRetrieveResponseValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public GDSOrderRetrieveResponseValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_DNYBBR()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "DNYBBR";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_EXTL8D()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "EXTL8D";

            string orderID = "VS932" + bookingID + "23";

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_EXEUMB()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "EKA8UM";

            string orderID = "VS932" + bookingID + "23";

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_CH9VLL()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "CH9VLL";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_DKLRVI()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "DKLRVI";

            string orderID = "VS932" + bookingID + "23";

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_CPVE32()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_DBXG7I()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_ENGC69()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_EBUAJK()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "EBUAJK";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_FFV9TK()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "FFV9TK";

            string orderID = "VS932" + bookingID + "23";

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_CHY2QN()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "CHY2QN";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_DFJN9G()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "DFJN9G";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_EMOD4V()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void GDS_DUJVHH()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "14080776",
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

            string bookingID = "DUJVHH";

            string orderID = "VS932" + bookingID + "23";
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingID);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was ");

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(XsdValidator.ValidateOrderRetrieveResponse(orderRetrieveResult, out string[] orderRetrieveResponseExceptions), string.Join(Environment.NewLine, orderRetrieveResponseExceptions));

            string orderIDRS = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:Order//def:OrderID", orderRetrieveRequestXmlnsManager).InnerText;

            Assert.True(bookingID.Equals(orderIDRS), $"Order Retrieve Response not found Actual{orderRetrieveResult}");

        }
    }
}
