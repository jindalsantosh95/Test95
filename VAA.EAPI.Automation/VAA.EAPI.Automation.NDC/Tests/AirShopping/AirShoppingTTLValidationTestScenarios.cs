using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using AirshoppingValidator = VAA.EAPI.Automation.NDC.Tests.AirShopping.AirshoppingValidator;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingTTLValidationTestScenarios : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";

        public AirShoppingTTLValidationTestScenarios(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC018_Airshopping_TTL_Validation_UK_9days_Travel_date()
        {
            //Generate Air Shopping request
            //comment
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC001_Airshopping_TTL_Validation_UK_7days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC002_Airshopping_TTL_Validation_UK_6days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(6).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC003_Airshopping_TTL_Validation_IND_5days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.DEL, locationTwo: LocationType.LON,
                locationThree: LocationType.LON, locationFour: LocationType.DEL,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC015_Airshopping_TTL_Validation_UK_3days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC004_Airshopping_TTL_Validation_UK_3days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC016_Airshopping_TTL_Validation_US_4days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(4).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC017_Airshopping_TTL_Validation_UK_4days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(4).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC005_Airshopping_TTL_Validation_UK_7days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LHR,
                locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", cabinTypeCodeThree: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC006_Airshopping_TTL_Validation_US_6days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(6).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LAS, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "2", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC007_Airshopping_TTL_Validation_US_8days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(8).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.SEA, locationTwo: LocationType.LHR,
                locationThree: LocationType.LHR, locationFour: LocationType.SEA,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC008_Airshopping_TTL_Validation_UK_5days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC009_Airshopping_TTL_Validation_UK_6days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(6).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC010_Airshopping_TTL_Validation_IND_9days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(9).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.JFK,
                locationThree: LocationType.JFK, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: 2, numberOfchildren: 1, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC011_Airshopping_TTL_Validation_US_2days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC013_Airshopping_TTL_Validation_UK_2days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC014_Airshopping_TTL_Validation_UK_1days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC012_Airshopping_TTL_Validation_US_1days_Travel_date()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //Assertions
            softAssert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateExpirationTTL(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }
    }
}