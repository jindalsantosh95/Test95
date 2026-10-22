using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingErrorCodeValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgId = "91281035";

        public AirShoppingErrorCodeValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgId, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgId}-OcpSubID"));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC01_AirShopping_ErrCode_8_PastDate()
        {
            string pastdate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: pastdate);

            XmlDocument airShoppingRequestXmlDocument  = new XmlDocument();
            airShoppingRequestXmlDocument.LoadXml(airShoppingRequest);

            XmlNamespaceManager xmlNamespaceManager = airShoppingRequestXmlDocument.CreateXmlNamespaceManager();

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "386", "Invalid days of operation.",out ValidationResultHelper result ), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC02_AirShopping_ErrCode_8_Future1YearDate()
        {
            string future1YearDate = DateTime.UtcNow.AddDays(370).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: future1YearDate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
           
            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "386", "Invalid days of operation.", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC03_AirShopping_ErrCode_12_InvalidProductDetails()
        {

            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            updatedAirShoppingRequest.SelectSingleNode("//ct:CabinTypeCode", airShoppingRequestXmlnsManager).InnerText = "9";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "12", "Invalid request – The requested CabinCode is not supported", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC05_AirShopping_ErrCode_68_MaxNonInfantPax10()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 6, numberOfchildren: 2, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "68", "Max accepted non-infant passengers is 9", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC04_AirShopping_Success_MaxNonInfantPax9()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 6, numberOfchildren: 2, numberOfyouths: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId)).EnsureSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTL = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            Assert.True(Count_OfferID.Count == Count_ExpirationTTL.Count, "Offer Count missmatch");
            Assert.True(Count_PaymentTTL.Count != 0, "PaymentTimeLimitDateTime not be 0");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC06_AirShopping_ErrCode_100_NoAvailableFlight()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.HAV,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "100", "No available flights for the requested query - Invalid route", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC07_AirShopping_ErrCode_130_InvalidOnDPair()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "130", "Invalid Origin and Destination Pair", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC09_AirShopping_ErrCode_143_InvalidPAXCode_ADT()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            updatedAirShoppingRequest.SelectSingleNode("//ct:Pax[1]//ct:PTC", airShoppingRequestXmlnsManager).InnerText = "ADR";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC10_AirShopping_ErrCode_143_InvalidPAXCode_YP()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            updatedAirShoppingRequest.SelectSingleNode("//ct:Pax[2]//ct:PTC", airShoppingRequestXmlnsManager).InnerText = "ADR";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC11_AirShopping_ErrCode_143_InvalidPAXCode_CHD()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            updatedAirShoppingRequest.SelectSingleNode("//ct:Pax[3]//ct:PTC", airShoppingRequestXmlnsManager).InnerText = "ADR";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC12_AirShopping_ErrCode_143_InvalidPAXCode_INF()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1,numberOfinfants:1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            updatedAirShoppingRequest.SelectSingleNode("//ct:Pax[4]//ct:PTC", airShoppingRequestXmlnsManager).InnerText = "ADR";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC13_AirShopping_ErrCode_324_INFMoreThanADT()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 0, numberOfinfants: 3);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "324", "Number of infants exceeds maximum allowed per adult passenger", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC14_AirShopping_ErrCode_476_DuplicatePAXCode_ADT()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();
            updatedAirShoppingRequest.SelectSingleNode("//ct:Pax[2]//ct:PaxID", airShoppingRequestXmlnsManager).InnerText = "ADULT_1";
            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC15_AirShopping_ErrCode_496_NoOffersFound()
        {
            string deptDate = DateTime.Now.ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,locationOne: LocationType.LHR, locationTwo: LocationType.TLS,cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, deptDate: deptDate,flighttype:"Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
            
            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "496", "No Offers found", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC16_AirShopping_ErrCode_368_IncorrectAgentSubKey()
        {
            //This test would never work

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "", participatingOrgName: "Skyscanner", participatingOrgOrgID: "35200056",
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "368", "Seller's subscription key is not valid", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC16_AirShopping_ErrCode_368_InvalidAgent()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdSwedishAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(airShoppingResponseXmlDocument, "368", "The requested Seller is not authorised to access the service", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC18_InvalidDataFormat()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            updatedAirShoppingRequest.SelectSingleNode("//ct:Ordinal", airShoppingRequestXmlnsManager).InnerText = "ABCD";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            dynamic airShoppingResult = JsonConvert.DeserializeObject<dynamic>( await airShoppingResponse.Content.ReadAsStringAsync());

            string errorcode = airShoppingResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = airShoppingResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC18_IncorrectOrder()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgOrgId,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedAirShoppingRequest.SelectSingleNode("//ct:Ordinal", airShoppingRequestXmlnsManager);

            XmlNode nodeToSwap = updatedAirShoppingRequest.SelectSingleNode("//ct:Ordinal", airShoppingRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = updatedAirShoppingRequest.SelectSingleNode("//ct:OrgRole", airShoppingRequestXmlnsManager);

            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgId));

            //Getting the Air Shopping response and converting to XML Document
            dynamic airShoppingResult = JsonConvert.DeserializeObject<dynamic>(await airShoppingResponse.Content.ReadAsStringAsync());

            string errorcode = airShoppingResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = airShoppingResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

    }
}
