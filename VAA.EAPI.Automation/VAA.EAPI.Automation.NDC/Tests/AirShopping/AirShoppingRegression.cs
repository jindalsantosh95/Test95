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
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingRegression : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public AirShoppingRegression(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdDanishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdDanishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSwedishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSwedishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSaudiAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSaudiAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCanadianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCanadianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCarrebianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCarrebianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCaribjetAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCaribjetAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdEuropeanAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdEuropeanAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdNorwegiansAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdNorwegiansAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [MemberData(nameof(NDCTestCases.GetAirShoppingTestData), MemberType = typeof(NDCTestCases))]
        public async Task AirShopping_RequestAndResponseValidation_Return(int test, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType = "Direct", string participatingOrgOrgID2 = "")
        {
            //Generate Air Shopping request
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, 200);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,journeyType,locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgID)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " +test+ string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateTaxSummary(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC001_OneWay_1PTC_Economy()
        {
            //Generate Air Shopping request
            //US agent 2 decimal currency
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Kayak USA", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassListCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC002_OneWay_multiPTC_Economy()
        {
            //Generate Air Shopping request
            //UK agent for GBP currency
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC003_Return_multiPTC_Economy()
        {
            //Generate Air Shopping request
            //India Agent INR currency
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "India Test Agent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC004_Return_multiPTC_Economy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 2, numberOfyouths: 2);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC005_Return_multiPTC_Economy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 2, numberOfinfants: 2);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC006_OpenJaw_Return_multiPTC_Premium()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC007_OpenJaw_Return_multiPTC_Economy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Documentm
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC008_MultiCity_multiPTC_Economy_route1()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC009_MultiCity_multiPTC_Economy_route2()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL, locationSeven: LocationType.ATL, locationEight: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC010_MultiCity_multiPTC_Economy_route3()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LON, locationThree: LocationType.LON, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC011_DifferentPOSAndPOC_Return_multiPTC_Upper_route1()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.ATL,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC012_DifferentPOSAndPOC_Return_multiPTC_Upper_route2()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.ATL,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDCE2ESanity")]
        [Fact]
        public async Task AirShopping_TC013_FlexibleFare_Return_multiPTC_Economy_Taxanomy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFlexFare(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC014_NonFlexibleFare_Return_multiPTC_Premium_Taxanomy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.MIA, locationThree: LocationType.MIA, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            airShoppingRequestDocument.SelectSingleNode("//def:OfferCriteria//def:FareCriteria//def:FareTypeCode", airShoppingRequestXmlnsManager).InnerText = "731";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNonFlexFare(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC015_CheckedBaggage_Return_multiPTC_Premium_Taxanomy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            airShoppingRequestDocument.LoadXml(airShoppingRequest);
            XmlNamespaceManager airShoppingRequestXmlnsManager = airShoppingRequestDocument.CreateXmlNamespaceManager();

            XmlNode removeTaxonomyNode = airShoppingRequestDocument.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:TaxonomyCode[1]", airShoppingRequestXmlnsManager);

            removeTaxonomyNode.ParentNode.RemoveChild(removeTaxonomyNode);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC016_CheckedBaggage_Return_multiPTC_Premium_Taxanomy_errorvalidation()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode removeTaxonomyNode = updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:TaxonomyCode[1]", airShoppingRequestXmlnsManager);

            removeTaxonomyNode.ParentNode.RemoveChild(removeTaxonomyNode);

            updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:IncludeInd", airShoppingRequestXmlnsManager).InnerText = "false";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");

            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("496"),
           $"Text value for air shopping error message is not as expected. Expected 496 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("No Offers found"),
            $"Error Code for air shopping error message is not as expected. Expected No Offers found " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC017_PreferredSeat_Return_multiPTC_Premium()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = airShoppingRequestDocument.CreateXmlNamespaceManager();

            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlNode itemnode2 = airShoppingRequestDocument.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:TaxonomyCode[2]", airShoppingRequestXmlnsManager);

            itemnode2.ParentNode.RemoveChild(itemnode2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC018_PreferredSeat_Return_MultiPTC_Premium_Errorvalidation()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = airShoppingRequestDocument.CreateXmlNamespaceManager();

            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlNode itemnode2 = airShoppingRequestDocument.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:TaxonomyCode[2]", airShoppingRequestXmlnsManager);

            itemnode2.ParentNode.RemoveChild(itemnode2);

            airShoppingRequestDocument.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:IncludeInd", airShoppingRequestXmlnsManager).InnerText = "false";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");

            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("496"),
           $"Text value for air shopping error message is not as expected. Expected 496 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("No Offers found"),
            $"Error Code for air shopping error message is not as expected. Expected No Offers found " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC019_AllCabinTypes_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfinfants: 2);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC020_WithOutCabinType_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfinfants: 2);

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[1]//def:CabinType", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            XmlNode itemNode2 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[2]//def:CabinType", airShoppingRequestXmlnsManager);

            itemNode2.ParentNode.RemoveChild(itemNode2);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequestDocument.OuterXml, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert.True(Validator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert.True(Validator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Assert.True(Validator.ValidatePriceClassList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC021_CabinCodeFirstOAndD_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingResponseXmlDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC022_CalendarSearch_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, calendarSearch: "calendarSearch");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC023_CalendarSearch_Oneway_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.EWR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, calendarSearch: "calendarSearch");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC024_CalendarSearch_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.DEL, locationFour: LocationType.JFK,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, calendarSearch: "calendarSearch");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC025_DirectFlightOneWay_multiPTC()
        {
            string todaysdate = DateTime.UtcNow.AddDays(124).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 2, deptDate: todaysdate, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList paxJourneyCount = airShoppingResponseXmlDocument.SelectNodes("//def:PaxJourney", airShoppingResponseXmlnsManager);

            for (int i = 1; i <= paxJourneyCount.Count; i++)
            {
                XmlNodeList segmentCount = airShoppingResponseXmlDocument.SelectNodes($"//def:PaxJourney[{i}]//def:PaxSegmentRefID", airShoppingResponseXmlnsManager);

                Assert.True(segmentCount.Count == 1, $"In direct flight search each journey Should have only one segment");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC026_DirectFlightReturn_Return_multiPTC_Premium()
        {
            string todaysdate = DateTime.UtcNow.AddDays(124).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 2, numberOfinfants: 1, deptDate: todaysdate, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList paxJourneyCount = airShoppingResponseXmlDocument.SelectNodes("//def:PaxJourney", airShoppingResponseXmlnsManager);

            for (int i = 1; i <= paxJourneyCount.Count; i++)
            {
                XmlNodeList segmentCount = airShoppingResponseXmlDocument.SelectNodes($"//def:PaxJourney[{i}]//def:PaxSegmentRefID", airShoppingResponseXmlnsManager);

                Assert.True(segmentCount.Count == 1, $"In direct flight search each journey Should have only one segment");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC027_DirectFlightMultiCity_multiPTC_Economy()
        {
            string todaysdate = DateTime.UtcNow.AddDays(248).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", cabinTypeCodeThree: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList paxJourneyCount = airShoppingResponseXmlDocument.SelectNodes("//def:PaxJourney", airShoppingResponseXmlnsManager);

            for (int i = 1; i <= paxJourneyCount.Count; i++)
            {
                XmlNodeList segmentCount = airShoppingResponseXmlDocument.SelectNodes($"//def:PaxJourney[{i}]//def:PaxSegmentRefID", airShoppingResponseXmlnsManager);

                Assert.True(segmentCount.Count == 1, $"In direct flight search each journey Should have only one segment");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC028_DirectFlightOneWay_MultiCity_multiPTC_Upper()
        {
            string todaysdate = DateTime.UtcNow.AddDays(268).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.ATL, locationFive: LocationType.ATL, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "2", numberOfadults: 1, numberOfyouths: 1, deptDate: todaysdate, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList paxJourneyCount = airShoppingResponseXmlDocument.SelectNodes("//def:PaxJourney", airShoppingResponseXmlnsManager);

            for (int i = 1; i <= paxJourneyCount.Count; i++)
            {
                XmlNodeList segmentCount = airShoppingResponseXmlDocument.SelectNodes($"//def:PaxJourney[{i}]//def:PaxSegmentRefID", airShoppingResponseXmlnsManager);

                Assert.True(segmentCount.Count == 1, $"In direct flight search each journey Should have only one segment");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC029_DepartureTimeSearchOneWay_multiPTC()
        {
            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.BOS,
            cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 2, deptDate: deptDate);

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 12;
            int outBoundJourneybeforeDeptTimeHours = 8;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria//def:OriginDepCriteria[1]", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT2H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT2H";
            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting deptarure timings of every flight from response
            XmlNodeList dmsCount = airShoppingResponseXmlDocument.SelectNodes("//def:DatedMarketingSegment", airShoppingResponseXmlnsManager);

            XmlNode code = airShoppingResponseXmlDocument.SelectSingleNode("//def:Code", airShoppingResponseXmlnsManager);

            string origin = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:OriginCode", airShoppingResponseXmlnsManager).InnerText;

            string dest = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:DestCode", airShoppingResponseXmlnsManager).InnerText;

            if (dmsCount.Count > 0)
            {
                for (int i = 1; i <= dmsCount.Count; i++)
                {
                    XmlNode dateAndTime = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:AircraftScheduledDateTime", airShoppingResponseXmlnsManager);

                    string flightTime = (dateAndTime.InnerText).Substring(11, 8);
                    string flightDate = (dateAndTime.InnerText).Substring(0, 10);

                    //Comparing the date of the flight with current date

                    string dep = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                    if ((flightDate.Equals(deptDate) && dep.Equals(origin)))
                    {
                        int responseHours = int.Parse(flightTime.Substring(0, 2));
                        Assert.True(responseHours <= outBoundJuorneyafterDepTimeHours && responseHours >= outBoundJourneybeforeDeptTimeHours, $"Expected offers between {outBoundJuorneyafterDepTimeHours} to {outBoundJourneybeforeDeptTimeHours} but actual offer was {responseHours})");
                    }
                }
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Failed with error code {code.InnerText} ");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task AirShopping_TC030_DepartureTimeSearchReturn_multiPTC_Premium()
        {
            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");
            string returnDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
            locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 2, numberOfyouths: 1, deptDate: deptDate);

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 16;

            int outBoundJourneybeforeDeptTimeHours = 4;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT6H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT6H";
            itemNode1.AppendChild(newElement3);

            //Node to add required fields to InBoundJourney
            XmlNode itemNode2 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement4 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement4.InnerText = deptDateTime;
            itemNode2.AppendChild(newElement4);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement5 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement5.InnerText = "PT6H";
            itemNode2.AppendChild(newElement5);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement6 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement6.InnerText = "PT6H";
            itemNode2.AppendChild(newElement6);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlNodeList dmsCount = airShoppingResponseXmlDocument.SelectNodes("//def:DatedMarketingSegment", airShoppingResponseXmlnsManager);

            XmlNode code = airShoppingResponseXmlDocument.SelectSingleNode("//def:Code", airShoppingResponseXmlnsManager);

            string origin = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:OriginCode", airShoppingResponseXmlnsManager).InnerText;

            string dest = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:DestCode", airShoppingResponseXmlnsManager).InnerText;

            if (dmsCount.Count > 0)
            {
                for (int i = 1; i <= dmsCount.Count; i++)
                {
                    XmlNode dateAndTime = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:AircraftScheduledDateTime", airShoppingResponseXmlnsManager);

                    string flightTime = (dateAndTime.InnerText).Substring(11, 8);
                    string flightDate = (dateAndTime.InnerText).Substring(0, 10);

                    //Comparing the date of the flight with current date

                    string dep = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                    if ((flightDate.Equals(deptDate) && dep.Equals(origin)) || (flightDate.Equals(returnDate) && dep.Equals(dest)))
                    {
                        int responseHours = int.Parse(flightTime.Substring(0, 2));
                        Assert.True(responseHours <= outBoundJuorneyafterDepTimeHours && responseHours >= outBoundJourneybeforeDeptTimeHours, $"Expected offers between {outBoundJuorneyafterDepTimeHours} to {outBoundJourneybeforeDeptTimeHours} but actual offer was {responseHours})");
                    }
                }
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Failed with error code {code.InnerText} ");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC031_DepartureTimeSearchMultiCity_multiPTC_Economy()
        {
            string deptDateTime = "08:00:00";
            string deptDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");
            string returnDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd");
            string returnDate1 = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
            locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.ATL,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDate);

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 17;

            int outBoundJourneybeforeDeptTimeHours = 1;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the Joureny1
            XmlElement newElement1 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the Joureny1
            XmlElement newElement2 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT9H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the Joureny1
            XmlElement newElement3 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT7H";
            itemNode1.AppendChild(newElement3);

            //Node to add required fields to Journey2
            XmlNode itemNode2 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the Journey2
            XmlElement newElement4 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement4.InnerText = deptDateTime;
            itemNode2.AppendChild(newElement4);

            //Adding TimeAfterDuration to the Journey2
            XmlElement newElement5 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement5.InnerText = "PT9H";
            itemNode2.AppendChild(newElement5);

            //Adding TimeBeforeDuration to the Journey2
            XmlElement newElement6 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement6.InnerText = "PT7H";
            itemNode2.AppendChild(newElement6);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting deptarure timings of every flight from response
            XmlNodeList dmsCount = airShoppingResponseXmlDocument.SelectNodes("//def:DatedMarketingSegment", airShoppingResponseXmlnsManager);
            XmlNode code = airShoppingResponseXmlDocument.SelectSingleNode("//def:Code", airShoppingResponseXmlnsManager);

            string origin = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:OriginCode", airShoppingResponseXmlnsManager).InnerText;

            string dest = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:DestCode", airShoppingResponseXmlnsManager).InnerText;

            if (dmsCount.Count > 0)
            {
                for (int i = 1; i <= dmsCount.Count; i++)
                {
                    XmlNode dateAndTime = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:AircraftScheduledDateTime", airShoppingResponseXmlnsManager);

                    string flightTime = (dateAndTime.InnerText).Substring(11, 8);
                    string flightDate = (dateAndTime.InnerText).Substring(0, 10);

                    //Comparing the date of the flight with current date

                    string dep = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                    if ((flightDate.Equals(deptDate) && dep.Equals(origin)) || (flightDate.Equals(returnDate) && dep.Equals(dest)))
                    {
                        int responseHours = int.Parse(flightTime.Substring(0, 2));
                        Assert.True(responseHours <= outBoundJuorneyafterDepTimeHours && responseHours >= outBoundJourneybeforeDeptTimeHours, $"Expected offers between {outBoundJuorneyafterDepTimeHours} to {outBoundJourneybeforeDeptTimeHours} but actual offer was {responseHours})");
                    }
                }
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Failed with error code {code.InnerText} ");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC - AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC032_AllFiltersExceptCalendarSearch_Return_multiPTC_Economy()
        {
            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");
            string returnDate = DateTime.UtcNow.AddDays(105).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
            locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 2, numberOfyouths: 1, flighttype: "Direct", fareTypeCode: "758", subTypeCode: RequestHelper.FareSubTypeCode.SeatOnly, taxonomy: "taxonomy", deptDate: deptDate);

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 19;

            int outBoundJourneybeforeDeptTimeHours = 1;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT9H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT9H";
            itemNode1.AppendChild(newElement3);

            //Node to add required fields to InBoundJourney
            XmlNode itemNode2 = airShoppingRequestDocument.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement4 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement4.InnerText = deptDateTime;
            itemNode2.AppendChild(newElement4);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement5 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement5.InnerText = "PT9H";
            itemNode2.AppendChild(newElement5);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement6 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement6.InnerText = "PT9H";
            itemNode2.AppendChild(newElement6);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            XmlNodeList dmsCount = airShoppingResponseXmlDocument.SelectNodes("//def:DatedMarketingSegment", airShoppingResponseXmlnsManager);

            XmlNode code = airShoppingResponseXmlDocument.SelectSingleNode("//def:Code", airShoppingResponseXmlnsManager);

            string origin = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:OriginCode", airShoppingResponseXmlnsManager).InnerText;

            string dest = airShoppingResponseXmlDocument.SelectSingleNode("//def:OriginDest[1]//def:DestCode", airShoppingResponseXmlnsManager).InnerText;

            if (dmsCount.Count > 0)
            {
                for (int i = 1; i <= dmsCount.Count; i++)
                {
                    XmlNode dateAndTime = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:AircraftScheduledDateTime", airShoppingResponseXmlnsManager);

                    string flightTime = (dateAndTime.InnerText).Substring(11, 8);
                    string flightDate = (dateAndTime.InnerText).Substring(0, 10);

                    //Comparing the date of the flight with current date

                    string dep = airShoppingResponseXmlDocument.SelectSingleNode($"//def:DatedMarketingSegment[{i}]//def:Dep//def:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                    if ((flightDate.Equals(deptDate) && dep.Equals(origin)) || (flightDate.Equals(returnDate) && dep.Equals(dest)))
                    {
                        int responseHours = int.Parse(flightTime.Substring(0, 2));
                        Assert.True(responseHours <= outBoundJuorneyafterDepTimeHours && responseHours >= outBoundJourneybeforeDeptTimeHours, $"Expected offers between {outBoundJuorneyafterDepTimeHours} to {outBoundJourneybeforeDeptTimeHours} but actual offer was {responseHours})");
                    }
                }
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Failed with error code {code.InnerText} ");
            }

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC033_CalendarSearchWithDirectFlight_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, flighttype: "Direct", calendarSearch: "calendarSearch");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");

            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("915"),
           $"Text value for air shopping error message is not as expected. Expected 915" +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Calendar search cannot be combined with the departure time or direct flight search"),
            $"Error Code for air shopping error message is not as expected. Expected Calendar search cannot be combined with the departure time or direct flight search " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC034_CalendarSearchWithDepartureTime_OneWay_multiPTC()
        {
            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(89).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, deptDate: deptDate, calendarSearch: "calendarSearch");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = airShoppingRequestDocument.CreateXmlNamespaceManager();

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = airShoppingRequestDocument.SelectSingleNode("//ct:OriginDestCriteria//ct:OriginDepCriteria[1]", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = airShoppingRequestDocument.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = airShoppingRequestDocument.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT2H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = airShoppingRequestDocument.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT2H";
            itemNode1.AppendChild(newElement3);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");

            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("915"),
           $"Text value for air shopping error message is not as expected. Expected 915" +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Calendar search cannot be combined with the departure time or direct flight search"),
            $"Error Code for air shopping error message is not as expected. Expected Calendar search cannot be combined with the departure time or direct flight search " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC035_CalendarSearchWithTaxonomy_Return_multiPTC()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy", calendarSearch: "calendarSearch");

            XmlDocument airShoppingRequestDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(airShoppingRequestDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequestDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFlexFare(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateTaxonomyCode(airShoppingRequestDocument, airShoppingResponseXmlDocument, true, true, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_TC036_Return_MultiPTC_Economy()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LON, locationTwo: LocationType.NYC, locationThree: LocationType.NYC, locationFour: LocationType.LON,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_OrdinalErrorValidation_Value0()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager AirShoppingRequestXmlnsManager1 = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            AirShoppingRequestXmlnsManager1.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedAirShoppingRequest.SelectSingleNode("//def:Ordinal", AirShoppingRequestXmlnsManager1).InnerText = "0";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            Assert.True(AirshoppingValidator.ValidateOrdinalAirshopping(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

    }
}
