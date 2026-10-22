using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OfferPrice
{
    public class OfferPriceErrorValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgIdTravelRepublic = "91281035";
        public static string participatingOrgIdOasisTravel = "35200056";
        public static string invalidorgID = "ABCD*&^";
        public OfferPriceErrorValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(participatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgIdOasisTravel, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgIdOasisTravel}-OcpSubID"));

        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC01_OfferPriceVal_ErrCode_143_InvalidPTC_ADT()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2);

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            airShoppingRequestXmlDocument.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingRequestXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestxmlNamespaceManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            string Invalid_AdultPaxId = "ADR";
            updatedOfferPriceRequest.SelectSingleNode("//ct:PTC[1]", offerPriceRequestxmlNamespaceManager).InnerText = Invalid_AdultPaxId;
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC02_OfferPriceVal_ErrCode_143_InvalidPTC_YP()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            airShoppingRequestXmlDocument.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingRequestXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestxmlNamespaceManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            string Invalid_YPPaxId = "GHT";
            updatedOfferPriceRequest.SelectSingleNode("//ct:Pax[2]//ct:PTC", offerPriceRequestxmlNamespaceManager).InnerText = Invalid_YPPaxId;
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC03_OfferPriceVal_ErrCode_143_InvalidPTC_CHD()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            airShoppingRequestXmlDocument.LoadXml(airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();

            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            string Invalid_CHDPaxId = "CNI";
            updatedOfferPriceRequest.SelectSingleNode("//ct:Pax[3]//ct:PTC", offerPriceRequestXmlnsManager).InnerText = Invalid_CHDPaxId;
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC04_OfferPriceVal_ErrCode_143_InvalidPTC_INF()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            string Invalid_INFPaxId = "INN";
            updatedOfferPriceRequest.SelectSingleNode("//ct:Pax[4]//ct:PTC", offerPriceRequestXmlnsManager).InnerText = Invalid_INFPaxId;
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC05_OfferPriceVal_ErrCode_143_MissingPTC_INF()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            string Invalid_INFPaxId = "";
            updatedOfferPriceRequest.SelectSingleNode("//ct:Pax[4]//ct:PTC", offerPriceRequestXmlnsManager).InnerText = Invalid_INFPaxId;
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC06_OfferPriceVal_ErrCode_143_DuplicatePTC_INF()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList Count_OfferID = airShoppingResponseXmlDocument.GetElementsByTagName("OfferID");
            XmlNodeList Count_ExpirationTTL = airShoppingResponseXmlDocument.GetElementsByTagName("OfferExpirationTimeLimitDateTime");
            XmlNodeList Count_PaymentTTLAir1 = airShoppingResponseXmlDocument.GetElementsByTagName("PaymentTimeLimitDateTime");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);
            string Dup_INFPaxId = "ADULT_1";
            updatedOfferPriceRequest.SelectSingleNode("//ct:Pax[4]//ct:PaxID", offerPriceRequestXmlnsManager).InnerText = Dup_INFPaxId;
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "Invalid or Ineligible Passenger Type Code", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC07_OfferPriceVal_ErrCode_143_PaxListEmpty()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;
            XmlNodeList nodesToRemove = updatedOfferPriceRequest.SelectNodes("//ct:PaxList", offerPriceRequestXmlnsManager);
            foreach (XmlNode nodeToRemove in nodesToRemove)
            {
                XmlNode parent = nodeToRemove.ParentNode;
                parent.RemoveChild(nodeToRemove);
            }
            offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "No passenger/ traveler list specified or empty", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC08_OfferPriceVal_ErrCode_143_TravelListEmpty()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;
            XmlNodeList nodesToRemove = updatedOfferPriceRequest.SelectNodes("//ct:PaxList", offerPriceRequestXmlnsManager);
            foreach (XmlNode nodeToRemove in nodesToRemove)
            {
                XmlNode parent = nodeToRemove.ParentNode;
                parent.RemoveChild(nodeToRemove);
            }
            offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "143", "No passenger/ traveler list specified or empty", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC09_OfferPrice_ErrCode_368_IATA_AgentTicketingAuthority()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdOasisTravel,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdOasisTravel)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdOasisTravel));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateOrdinalAirshopping(offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC10_OfferPrice_ErrCode_368_IATA_AgentTicketingAuthority()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgIdOasisTravel,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdOasisTravel)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            XmlNamespaceManager offerPriceRequestXmlnsManager = updatedOfferPriceRequest.CreateXmlNamespaceManager();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);
            string offerPriceRequest1 = updatedOfferPriceRequest.OuterXml;

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdOasisTravel));

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = await offerPriceResponse.Content.ReadAsStringAsync();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateOrdinalAirshopping(offerPriceResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC11_InvalidDataFormat()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);
            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");
            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();

            updatedOfferPriceRequest.LoadXml(offerPriceRequest);
            updatedOfferPriceRequest.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            string errorcode = offerPriceResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = offerPriceResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC12_IncorrectOrder()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);
            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");
            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            XmlNode nodeToRemove = updatedOfferPriceRequest.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = updatedOfferPriceRequest.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = updatedOfferPriceRequest.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post OfferPriceRequest request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            string errorcode = offerPriceResult.statusCode?.ToString();
            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = offerPriceResult.message?.ToString();
            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC13_InvalidOrgID()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();

            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            updatedOfferPriceRequest.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "ABC#$#$D";

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(invalidorgID));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            string errorcode = offerPriceResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = offerPriceResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";


            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC14_ReorderPaxlist()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager 
            XmlNamespaceManager xmlNamespaceManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();

            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            XmlNode nodeToRemove = updatedOfferPriceRequest.SelectSingleNode("//ct:PaxID", xmlNamespaceManager);
            XmlNode nodeToSwap = updatedOfferPriceRequest.SelectSingleNode("//ct:PaxID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = updatedOfferPriceRequest.SelectSingleNode("//ct:PTC", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post OfferPriceRequest request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            string errorcode = offerPriceResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = offerPriceResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Pax' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'PTC' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'AgeMeasure, Birthdate, CitizenshipCountryCode, ContactInfoRefID, Employer, FOID, IdentityDoc, Individual, LangUsage, LoyaltyProgramAccount, PaxID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. ";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OfferPriceAllRegression")]
        [Fact]
        public async Task TC15_ReorderSelectedOffer()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "TravelRepublic", participatingOrgOrgID: participatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager xmlNamespaceManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            XmlDocument updatedOfferPriceRequest = new XmlDocument();
            updatedOfferPriceRequest.LoadXml(offerPriceRequest);

            XmlNode nodeToRemove = updatedOfferPriceRequest.SelectSingleNode("//ct:OfferRefID", xmlNamespaceManager);
            XmlNode nodeToSwap = updatedOfferPriceRequest.SelectSingleNode("//ct:OfferRefID", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = updatedOfferPriceRequest.SelectSingleNode("//ct:PaxRefID", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post OfferPriceRequest request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(updatedOfferPriceRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string errorcode = offerPriceResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = offerPriceResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedOffer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OfferRefID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

    }
}
