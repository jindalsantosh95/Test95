using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;
using VAA.EAPI.Automation.NDC.Tests.OfferPrice;

namespace VAA.EAPI.Automation.NDC.Tests.V2Validations
{
    public class SeatAvailabilityPostSale : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public static string env = Config.Environment;

        public SeatAvailabilityPostSale(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC3_IncorrectOrderID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText = "VS932ETDCQX25";

            //Post SeatAvailabilityresponse Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string offerPriceResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "129", "No PNR or Order ID match found", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void SeatAvailabilityPostSale_TC4_OrdinalValue0()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            seatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            //Post SeatAvailabilityresponse Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string offerPriceResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC5_InvalidSubkey()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new();
            seatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            //Post SeatAvailabilityresponse Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent));

            string offerPriceResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(offerPriceResponseXmlDocument, "368", "Seller's subscription key is not valid", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC6_InvalidOrdinalValue()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new();
            SeatAvailabilityRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = SeatAvailabilityRequestXmlDocument.CreateXmlNamespaceManager();

            SeatAvailabilityRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            //Post order SeatAvailability Result
            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(SeatAvailabilityRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC7_InvalidDataformat()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new();
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

            Assert.True(errordesc.Contains(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC8_Incorrectorder()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument SeatAvailabilityRequestXmlDocument = new();
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

            Assert.True(errordesc.Contains(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC9_InvalidOrgID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new();
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

            Assert.True(errordesc.Contains(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatPostSaleV2")]
        [Fact]
        public void SeatAvailabilityPostSale_TC10_ReorderOrderID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\SeatAvailabilityRQ.xml");

            XmlDocument seatAvailabilityRequestXmlDocument = new();
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

            Assert.True(errordesc.Contains(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }
    }
}
