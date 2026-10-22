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
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OfferPrice
{
    public class OfferPriceRegression : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public OfferPriceRegression(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OneWay()
        {
            //Generate Air Shopping request
            //US TIDS agent for FormOfPayment tag
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdKayakUsa,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdKayakUsa)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdKayakUsa)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        //GBP currency UK agent
        public async Task Return()
        {
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdSkyscannerInDirect,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, agentType: "InDirect", participatingOrgOrgID2: AgentID.ParticipatingOrgIdTravelFusionAggregator);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdSkyscannerInDirect)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdSkyscannerInDirect));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        //INR currency IND agent
        public async Task ReturnCodeShare()
        {
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.YYZ, locationThree: LocationType.YYZ, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.NonVirginRequestedOperator, FlightType.DL);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCurrency(airShoppingRequestDocument, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task ReturnNonVS()
        {
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.CUN, locationThree: LocationType.CUN, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.NonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task MultiCity()
        {
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.MultiCity,
               locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task MultiCityCodeShare()
        {
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.Return,
               locationOne: LocationType.IAD, locationTwo: LocationType.LON, locationThree: LocationType.LON, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL,
            cabinTypeCodeOne: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_OneWayTC01()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_OneWayTC02()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 5, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.NonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_ReturnTC03()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_ReturnTC04()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.JNB, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 5, numberOfchildren: 2, numberOfyouths: 2, numberOfinfants: 4);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_ReturnTC05()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 6, numberOfchildren: 2, numberOfinfants: 3, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_OpenJawTC06()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_OpenJawTC07()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_MultiCityTC08()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_MultiCityTC09()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LON, locationThree: LocationType.LON, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_MultiCityTC10()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.MCO, locationFive: LocationType.MIA, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 2, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DifferentPOSAndPOCTC11()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.JFK, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JNB,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfinfants: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DifferentPOSAndPOCTC12()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.ATL,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Trait("Category", "NDC-ProdSanity")]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_FlexibleFareTC14()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.IAD, locationThree: LocationType.IAD, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Trait("Category", "NDC-ProdSanity")]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_NonFlexibleFareTC15()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:FareCriteria//def:FareTypeCode", airShoppingRequestXmlnsManager).InnerText = "731";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Trait("Category", "NDC-ProdSanity")]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_CheckedBaggageTC16()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode removeTaxonomyNode = updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:TaxonomyCode[1]", airShoppingRequestXmlnsManager);

            removeTaxonomyNode.ParentNode.RemoveChild(removeTaxonomyNode);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Trait("Category", "NDC-ProdSanity")]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_PreferredSeatTC17()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode removeTaxanomyNode = updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria//ct:TaxonomyCode[2]", airShoppingRequestXmlnsManager);

            removeTaxanomyNode.ParentNode.RemoveChild(removeTaxanomyNode);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_AllCabinTypesTC18()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfinfants: 2);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_WithOutCabinTypeTC19()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfinfants: 2);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:CabinType", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            XmlNode itemNode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:CabinType", airShoppingRequestXmlnsManager);

            itemNode2.ParentNode.RemoveChild(itemNode2);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(updatedAirShoppingRequest.OuterXml, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_CabinCodeFirstOAndDTC20()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, deptDate: deptDateTime);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_CalendarSearchTC21()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, calendarSearch: "calendarSearch");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_CalendarSearchTC22()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.EWR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, calendarSearch: "calendarSearch");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.NonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_CalendarSearchTC23()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.ATL, locationFive: LocationType.ATL, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "5", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, calendarSearch: "calendarSearch");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DirectFlightOneWayTC24()
        {
            string todaysdate = DateTime.UtcNow.AddDays(124).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 2, deptDate: todaysdate, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Trait("Category", "NDC-ProdSanity")]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DirectFlightReturnTC25()
        {
            string todaysdate = DateTime.UtcNow.AddDays(210).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 2, numberOfinfants: 1, deptDate: todaysdate, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.NonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DirectFlightMultiCityTC26()
        {
            string deptDateTime1 = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime1, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "2", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime1, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Upper Class Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Upper Class Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DirectFlightOneWayTC27()
        {
            string deptDateTime1 = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime1, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", cabinTypeCodeThree: "4", numberOfadults: 1, numberOfyouths: 1, deptDate: deptDateTime1, flighttype: "Direct");

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DepartureTimeSearchOneWayTC28()
        {
            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.BOS,
            cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 2, deptDate: deptDate);
            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 12;
            int outBoundJourneybeforeDeptTimeHours = 8;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria//def:OriginDepCriteria[1]", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT2H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT2H";
            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.NonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DepartureTimeSearchReturnTC29()
        {
            string deptDateTime1 = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime1, maxNumberOfDays);

            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");
            string returnDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
            locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 2, numberOfyouths: 1, deptDate: deptDateTime1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 19;

            int outBoundJourneybeforeDeptTimeHours = 1;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT9H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT9H";
            itemNode1.AppendChild(newElement3);

            //Node to add required fields to InBoundJourney
            XmlNode itemNode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement4 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement4.InnerText = deptDateTime;
            itemNode2.AppendChild(newElement4);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement5 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement5.InnerText = "PT9H";
            itemNode2.AppendChild(newElement5);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement6 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement6.InnerText = "PT9H";
            itemNode2.AppendChild(newElement6);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Premium", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Premium Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_DepartureTimeSearchMultiCityTC30()
        {
            string deptDateTime1 = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime1, maxNumberOfDays);

            string deptDateTime = "12:00:00";
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
            locationOne: LocationType.ATL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 3;

            int outBoundJourneybeforeDeptTimeHours = 21;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the Joureny1
            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the Joureny1
            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT9H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the Joureny1
            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT9H";
            itemNode1.AppendChild(newElement3);

            //Node to add required fields to Journey2
            XmlNode itemNode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the Journey2
            XmlElement newElement4 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement4.InnerText = deptDateTime;
            itemNode2.AppendChild(newElement4);

            //Adding TimeAfterDuration to the Journey2
            XmlElement newElement5 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement5.InnerText = "PT8H";
            itemNode2.AppendChild(newElement5);

            //Adding TimeBeforeDuration to the Journey2
            XmlElement newElement6 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement6.InnerText = "PT8H";
            itemNode2.AppendChild(newElement6);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_AllFiltersExceptCalendarSearchTC31()
        {
            string deptDateTime = "10:00:00";
            string deptDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");
            string returnDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
            JourneyType.Return,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
            cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 2, numberOfyouths: 1, flighttype: "Direct", taxonomy: "taxonomy", deptDate: deptDate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //OutBoundjourneyToleranceWindow
            int outBoundJuorneyafterDepTimeHours = 14;

            int outBoundJourneybeforeDeptTimeHours = 2;

            //Node to add required fields to OutBoundJourney
            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement1.InnerText = deptDateTime;
            itemNode1.AppendChild(newElement1);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement2.InnerText = "PT6H";
            itemNode1.AppendChild(newElement2);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement3.InnerText = "PT6H";
            itemNode1.AppendChild(newElement3);

            //Node to add required fields to InBoundJourney
            XmlNode itemNode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[2]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            //Adding Time Field in the OutBoundJourney
            XmlElement newElement4 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement4.InnerText = deptDateTime;
            itemNode2.AppendChild(newElement4);

            //Adding TimeAfterDuration to the OutBoundjourney
            XmlElement newElement5 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement5.InnerText = "PT6H";
            itemNode2.AppendChild(newElement5);

            //Adding TimeBeforeDuration to the OutBoundjourney
            XmlElement newElement6 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElement6.InnerText = "PT6H";
            itemNode2.AppendChild(newElement6);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Delight Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Delight Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_CalendarSearchWithTaxonomyTC32()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy", calendarSearch: "calendarSearch");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Delight Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Delight Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_ReturnTC33()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LON, locationTwo: LocationType.NYC, locationThree: LocationType.NYC, locationFour: LocationType.LON,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 1);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task NonIATAAgent_OfferPrice_RequestAndResponseValidation_OneWayTC34()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "CHEAP FLIGHTS LTD", participatingOrgOrgID: AgentID.ParticipatingOrgIdCheapflightsUKTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOS,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyLight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateServiceDefinitionList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Light", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Light", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_FourJourneyMultiCityTC35()
        {
            string deptDateTime1 = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime1, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.TLV, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK, locationFive: LocationType.JFK, locationSix: LocationType.LHR, locationSeven: LocationType.LHR, locationEight: LocationType.ATL,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", cabinTypeCodeThree: "2", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Upper Class Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Upper Class Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCOfferPriceRegression)]
        [Fact]
        public async Task OfferPrice_RequestAndResponseValidation_FiveJourneyMultiCityTC36()
        {
            string deptDateTime1 = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime1, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.MultiCity,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.BOM, locationFive: LocationType.BOM, locationSix: LocationType.LHR, locationSeven: LocationType.LHR, locationEight: LocationType.DXB, locationNine: LocationType.DXB, locationTen: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: deptDateTime1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(System.Environment.NewLine, airShoppingResponseExceptions));

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOfferPriceRequest(offerPriceRequest, out string[] offerPriceRequestExceptions), string.Join(System.Environment.NewLine, offerPriceRequestExceptions));

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateOfferPriceResponse(offerPriceResult, out string[] offerPriceResponseExceptions), string.Join(System.Environment.NewLine, offerPriceResponseExceptions));

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOriginDestList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateDatedMarketingAndOperatingList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePaxSegmentList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateBaggageAllowancelist(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePriceClassList(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateCountryCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferBaggageAssociations(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOtherOffersFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidatePricedOfferFareComponent(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetail(airShoppingRequest, offerPriceResult, "Economy Classic Flex", out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFormOfPayments(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareDetailOtherOffers(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateExpirationTTL(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateFareCodeType(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateOperatingLeg(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferPriceValidator.ValidateAirCarftTypeCode(offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

    }
}
