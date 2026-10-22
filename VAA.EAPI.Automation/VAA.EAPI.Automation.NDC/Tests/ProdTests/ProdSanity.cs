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
using VAA.EAPI.Automation.NDC.Tests.OfferPrice;
using VAA.EAPI.Automation.NDC.Tests.SeatAvailability;
using VAA.EAPI.Automation.NDC.Tests.ServiceList;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.ProdTests
{
    public class ProdSanity : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public ProdSanity(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public async Task AirShopAndOfferPriceReturn()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 5, numberOfchildren: 2, numberOfyouths: 2, numberOfinfants: 4);

            //validate xsd against requests
            Assert.True(XsdValidator.ValidateAirShoppingRequest(airShoppingRequest, out string[] airShoppingRequestExceptions), string.Join(System.Environment.NewLine, airShoppingRequestExceptions));

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);
            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

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

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public async Task AirShopAndOfferPriceFlexibleFareTC14()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfinfants: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public async Task AirShopAndOfferPriceNonFlexibleFareTC15()
        {
            //Generate Air Shopping request
            string airShoppingRequest = GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfyouths: 1, taxonomy: "taxanomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria", airShoppingRequestXmlnsManager);

            itemNode1.ParentNode.RemoveChild(itemNode1);

            updatedAirShoppingRequest.SelectSingleNode("//ct:OfferCriteria//ct:FareCriteria//ct:FareTypeCode", airShoppingRequestXmlnsManager).InnerText = "731";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public async Task AirShopAndOfferPriceCheckedBaggageTC16()
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

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public async Task AirShopAndOfferPricePreferredSeatTC17()
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

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(AirshoppingValidator.ValidateOffersReturned(airShoppingResponseXmlDocument, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(airShoppingResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePrice(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateCabinType(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOriginDestList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxJourneylist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateDatedMarketingAndOperatingList(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePaxSegmentList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAllowancelist(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidatePriceClassList(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateBaggageAssociations(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareComponent(updatedAirShoppingRequest, airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateFareCodeType(airShoppingResponseXmlDocument, out result, "70J"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateOperatingLeg(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateMandatoryInd(airShoppingResponseXmlDocument, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

            softAssert.True(OfferPriceValidator.ValidatePaxList(airShoppingRequest, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

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

        [Theory]
        [Trait("Category", Traits.NDCProdSanity)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.NonVirginRequestedOperator, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 2, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.NonVirginRequestedWithOtherOperators, JourneyType.Return, LocationType.LHR, LocationType.CUN, LocationType.CUN, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.OneWay, LocationType.LHR, LocationType.YYZ, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(4, 200, 100, AgentID.ParticipatingOrgIdKayakUsa, 1, 0, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.JFK, LocationType.LHR, "Direct", null)]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        [InlineData(6, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.NonVirginRequestedOperator, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, FlightType.DL)]
        public void SeatPrimeSaleDLTests(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, FlightType operatorName = FlightType.VS)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, serviceCount: 0, seatType: SeatPriceType.Paid, apiResult: ApiResult.SeatAvailabilityRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, operatorName: operatorName);

                string airShoppingRequest = result1["airShoppingRQ"];

                string seatAvailabilityResult = result1["seatAvailabilityResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //Seat availability assertions
                softAssert.True(SeatAvailabilityValidator.ValidateAlaCarteOffer(seatAvailabilityResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(AirshoppingValidator.ValidateNullValues(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDatedMarketingAndOperatingList(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidatePaxList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateServiceDefinitionList(airShoppingRequest, seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateDisclosureAndSeatProfile(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateSeatMap(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateOperatingLeg(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(SeatAvailabilityValidator.ValidateAirCarftTypeCode(seatAvailabilityResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCProdSanity)]
        [InlineData(1, 200, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(2, 120, 60, AgentID.ParticipatingOrgIdIndiaTestAgent, 2, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(3, 100, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(4, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, "Direct", null)]
        [InlineData(5, 250, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        public void ServiceListPrimeSale(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2)
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, serviceCount: 1, apiResult: RequestHelper.ApiResult.ServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2);

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceResult"];

            string serviceListResult = result1["serviceListResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //serviceListResult assertion

            softAssert.True(ServiceListAssertions.ValidateOffers(serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidateDatedMarketingAndOperatingList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidatePaxList(serviceListResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidatePaxSegmentList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidateServiceDefnitionList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidateAlcarteOffers(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public void OrderRetrieve()
        {
            StorageAccountHelper.CleanupExpiredPNRs(Config.Environment);
            string[] pnrs = StorageAccountHelper.RetrievePNRFromStorageTable("NDCPNRsTable");

            foreach (string pnr in pnrs)
            {
                string request = string.Format(File.ReadAllText("requests\\Body\\OrderRetrieve.xml"), pnr);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(request).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Assert.True(AirshoppingValidator.ValidateNullValues(orderRetrieveResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            }
        }

        [Fact]
        [Trait("Category", Traits.NDCProdSanity)]
        public void OrderList()
        {
            RetryHelper.RunWithRetry(() =>
            {
                //creation date time limit filter more than 180 days
                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

                string orderListRQ = RequestHelper.GenerateOrderListRequest(airShoppingRequest, creationDateTimeFrom: "2025-09-17T14:30:47Z", creationDateTimeTo: "2025-12-17T14:30:47Z");

                XmlDocument updatedXmlRQ = new XmlDocument();

                updatedXmlRQ.LoadXml(orderListRQ);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlNamespaceManager xmlNamespaceManager = updatedXmlRQ.CreateXmlNamespaceManager();

                HttpResponseMessage orderListResponse = NDCTransformationEndpoints.OrderList(updatedXmlRQ.OuterXml).EnsureIsSuccessStatusCode();

                string orderListResult = orderListResponse.Content.ReadAsStringAsync().Result;

                Assert.True(AirshoppingValidator.ValidateNullValues(orderListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                Assert.True(true);
            }, 1);
        }

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public void OrderQuoteIncorrectDistributionChain()
        {
            XmlDocument xmlDocument_1 = new XmlDocument();

            string request = File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:Ordinal", responseXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderQuote(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "914", "Invalid format/data - Carrier org Id is missing in the request.", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public void OrderReshopIncorrectDistributionChain()
        {
            string request = string.Format(File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml"));

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "914", "Invalid format/data - Carrier org Id is missing in the request.", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", Traits.NDCProdSanity)]
        [Fact]
        public void OrderChangeIncorrectOrdinalValue()
        {
            XmlDocument xmlDocument_1 = new XmlDocument();

            string request = File.ReadAllText("Requests\\Body\\OrderChangeRQ.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:Ordinal", responseXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderChange(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

    }
}
