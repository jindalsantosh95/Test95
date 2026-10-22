using System.Globalization;
using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.OfferPrice;

namespace VAA.EAPI.Automation.NDC.Tests.V2Validations
{
    public class OrderReshopTests : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderReshopTests(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", Traits.NDCSanityV2)]
        [Fact]
        public void Reshop_OrdinalValueZero()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQv2.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:Ordinal", responseXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(resultDoc, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));

        }

        [Trait("Category", "ReshopTestsV2")]
        [Fact]
        public void Reshop_IncorrectBookingID()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQv2.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:BookingID", responseXmlnsManager).InnerText = "1234";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(resultDoc, "129", "No PNR match found", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "ReshopTestsV2")]
        [Fact]
        public void Reshop_IncorrectOrderID()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQv2.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:OrderRefID", responseXmlnsManager).InnerText = "1234";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(resultDoc, "129", "No PNR or Order ID match found", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "ReshopTestsV2")]
        [Fact]
        public void Reshop_IncorrectOrigin()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQv2.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:OriginDestCriteria//ct:DestArrivalCriteria//ct:IATA_LocationCode", responseXmlnsManager).InnerText = "TLV";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(resultDoc, "129", "Location Code is not matching with the Booked Order", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "ReshopTestsV2")]
        [Fact]
        public void Reshop_IncorrectOrderItem()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQv2.xml");

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager responseXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:DeleteOrderItem//ct:OrderItemRefID", responseXmlnsManager).InnerText = "LHR";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(resultDoc, "914", "Invalid OrderItem found", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "ReshopTestsV2")]
        [Fact]
        public void Reshop_InvalidDataformat()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQv2.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            OrderReshopRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml);

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value";

            Assert.True(errordesc.Contains(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ReshopTestsV2")]
        [Fact]
        public void Reshop_Incorrectorder()
        {
            string request = File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml);

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Contains(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

    }
}
