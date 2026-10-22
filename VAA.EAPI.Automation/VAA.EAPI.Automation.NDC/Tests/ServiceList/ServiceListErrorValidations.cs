using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.OfferPrice;

namespace VAA.EAPI.Automation.NDC.Tests.ServiceList
{
    public class ServiceListErrorValidations : IClassFixture<MemoryStoreFixture>
    {

        private readonly MemoryStoreFixture memoryStoreFixture;

        

        public ServiceListErrorValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdOasisTravel, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdOasisTravel}-OcpSubID"));

        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void ServiceList_TC1_InvalidDataformat()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument ServiceListRequestXmlDocument = new XmlDocument();
            ServiceListRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = ServiceListRequestXmlDocument.CreateXmlNamespaceManager();

            ServiceListRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post ServiceList Result
            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(ServiceListRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the ServiceListresponse and converting to XML Document
            dynamic serviceListResult = JsonConvert.DeserializeObject<dynamic>(serviceListResponse.Content.ReadAsStringAsync().Result);

            string errorcode = serviceListResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 500 but actual is {errorcode}");

            string errordesc = serviceListResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value. Line: 1, Position: 329";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void ServiceList_TC2_Incorrectorder()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument ServiceListRequestXmlDocument = new XmlDocument();
            ServiceListRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = ServiceListRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = ServiceListRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = ServiceListRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = ServiceListRequestXmlDocument.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post ServiceList Result
            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(ServiceListRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the ServiceListresponse and converting to XML Document
            dynamic serviceListResult = JsonConvert.DeserializeObject<dynamic>(serviceListResponse.Content.ReadAsStringAsync().Result);

            string errorcode = serviceListResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 500 but actual is {errorcode}");

            string errordesc = serviceListResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 315";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void ServiceList_TC3_InvalidOrgID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument ServiceListRequestXmlDocument = new XmlDocument();
            ServiceListRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = ServiceListRequestXmlDocument.CreateXmlNamespaceManager();

            ServiceListRequestXmlDocument.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "ABC#$#$D";

            //Post ServiceList Result
            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(ServiceListRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.InvalidOrgId));

            //Getting the ServiceListresponse and converting to XML Document
            dynamic serviceListResult = JsonConvert.DeserializeObject<dynamic>(serviceListResponse.Content.ReadAsStringAsync().Result);

            string errorcode = serviceListResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = serviceListResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void ServiceList_TC4_ReorderOrderID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument ServiceListRequestXmlDocument = new XmlDocument();
            ServiceListRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = ServiceListRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = ServiceListRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);
            XmlNode nodeToSwap = ServiceListRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = ServiceListRequestXmlDocument.SelectSingleNode("//ct:OwnerCode", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post ServiceList Result
            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(ServiceListRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the ServiceListresponse and converting to XML Document
            dynamic serviceListResult = JsonConvert.DeserializeObject<dynamic>(serviceListResponse.Content.ReadAsStringAsync().Result);

            string errorcode = serviceListResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 500 but actual is {errorcode}");

            string errordesc = serviceListResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Order' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 619";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void TC5_InvalidOrderID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument ServiceListRequestXmlDocument = new XmlDocument();
            ServiceListRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = ServiceListRequestXmlDocument.CreateXmlNamespaceManager();

            ServiceListRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText = "VS932FCO4TM24";

            //Post ServiceList Result
            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(ServiceListRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument serviceListResponseXmlDocument = new XmlDocument();
            serviceListResponseXmlDocument.LoadXml(serviceListResult);

            Assert.True(OfferPriceValidator.ValidateErrorXmlResponse(serviceListResponseXmlDocument, "76H", "Ancillary Services not available for the request Order", out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "ServiceListRegression")]
        [Fact]
        public void ServiceList_TC6_RemoveOrderID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\ServiceListRQ.xml");

            XmlDocument ServiceListRequestXmlDocument = new XmlDocument();
            ServiceListRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = ServiceListRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = ServiceListRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            //Post ServiceList response Result
            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(ServiceListRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the ServiceListresponse and converting to XML Document
            dynamic serviceListResult = JsonConvert.DeserializeObject<dynamic>(serviceListResponse.Content.ReadAsStringAsync().Result);

            string errorcode = serviceListResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 500 but actual is {errorcode}");

            string errordesc = serviceListResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Order' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 619";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }
    }
}
