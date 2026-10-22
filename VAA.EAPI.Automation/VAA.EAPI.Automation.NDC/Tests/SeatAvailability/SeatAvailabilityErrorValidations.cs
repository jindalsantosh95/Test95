using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class SeatAvailabilityErrorValidations : IClassFixture<MemoryStoreFixture>
    {

        private readonly MemoryStoreFixture memoryStoreFixture;



        public SeatAvailabilityErrorValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdOasisTravel, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdOasisTravel}-OcpSubID"));

        }

        [Trait("Category", "SeatPrimeSaleV2")]
        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void PrimeSaleSeatInvalidOrdinalValue()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            //Post order SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "SeatPrimeSaleV2")]
        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void PrimeSaleSeatInvalidOfferID()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityOfferRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OfferID", xmlNamespaceManager).InnerText = "0";

            //Post order SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "913", "Invalid or Timed-out OfferID entered", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "SeatPrimeSaleV2")]
        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void PrimeSaleSeatNonVsFlights()
        {
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "00002222",
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.YYZ, cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode,
                $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " +
                $"AirShopping Request sent was {airShoppingResponse.ReadRequestAsString()}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.NonVirginRequestedOperator, FlightType.AF);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            int nonVsCount = offerPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[string-length(ct:MarketingCarrierFlightNumberText) = 4]", airShoppingResponseXmlnsManager).Count;

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", airShoppingResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(seatAvailabilityResult);

            int nonVsCountSeat = resultDoc.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Error[ct:Code='200' and ct:DescText]", airShoppingResponseXmlnsManager).Count;

            Assert.True(nonVsCount==nonVsCountSeat, "Error message is not returned for the non vs flights");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_TC1_InvalidDataformat()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post order SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_TC2_Incorrectorder()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_TC3_InvalidOrgID()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new XmlDocument();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "ABC#$#$D";

            //Post SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.InvalidOrgId));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_TC4_ReorderOrderID()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new XmlDocument();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);
            XmlNode nodeToSwap = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OwnerCode", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Order' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void TC5_RemoveOrderID()
        {
            string request = File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new XmlDocument();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            //Post SeatAvailabilityresponse Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Order' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }
    }
}
