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
using VAA.EAPI.Automation.NDC.Tests.OfferPrice;

namespace VAA.EAPI.Automation.NDC.Tests.V2Validations
{
    public class ServiceListPostSaleV2 : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public ServiceListPostSaleV2(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC3_IncorrectOrderID()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(260).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, LocationType.TLV, cabinTypeCodeOne: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().Result;
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            string orderID = "123456";

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(updatedSerivceListResponse, "129", "No PNR or Order ID match found", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void ServiceListPostSale_TC4_OrdinalValue0()
        {
            string request = File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new XmlDocument();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            //Post SeatAvailabilityresponse Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.ServiceListRequest( seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string offerPriceResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC5_InvalidSubkey()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(260).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
               JourneyType.OneWay,
               locationOne: LocationType.LHR, locationTwo: LocationType.TLV, LocationType.TLV, cabinTypeCodeOne: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().Result;
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            string orderID = "VS932CMR43724";

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdKayakUsa));

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(updatedSerivceListResponse, "368", "Seller's subscription key is not valid", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC6_InvalidOrdinalValue()
        {
            string request = File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            //Post order SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.ServiceListRequest( SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC7_InvalidDataformat()
        {
            string request = File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post order SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.ServiceListRequest( SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC8_Incorrectorder()
        {
            string request = File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new XmlDocument();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.ServiceListRequest( SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC9_InvalidOrgID()
        {
            string request = File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new XmlDocument();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "ABC#$#$D";

            //Post SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.ServiceListRequest( seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.InvalidOrgId));

            //Getting the SeatAvailabilityresponse and converting to XML Document
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListPostSaleV2")]
        [Fact]
        public void ServiceListPostSale_TC10_ReorderOrderID()
        {
            string request = File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new XmlDocument();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);
            XmlNode nodeToSwap = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OwnerCode", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.ServiceListRequest( seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

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
