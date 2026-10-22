using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderChange;

namespace VAA.EAPI.Automation.NDC.Tests.OrderChange.ErrorMessageValidation
{
    public class OrderChangeMandatoryFieldValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderChangeMandatoryFieldValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOrdinalIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:Ordinal", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderReshop(updatedOrderChangeRequest.OuterXml);
            
            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());
            
            string errorcode = updatedorderChangeResult.statusCode?.ToString();
            
            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();
            
            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage:IATA_OrderChangeRQ' element is not declared. Line: 1, Position: 40";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOrdinalValueIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:Ordinal", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value '' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string '' is not a valid Integer value. Line: 1, Position: 363";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOrgRoleIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:OrgRole", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrgRole' element is invalid - The value '' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:DistributionChainRoleCodeType' - The Enumeration constraint failed. Line: 1, Position: 383";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenParticipantOrgNameIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:OrgRole", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrgRole' element is invalid - The value '' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:DistributionChainRoleCodeType' - The Enumeration constraint failed. Line: 1, Position: 383";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");


        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenParticipantOrgNameIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:ParticipatingOrg//ct:Name", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);


            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1633";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOfferRefIdIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOfferRefIdIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOfferRefIdIsExpiredInOrderChangeRequest()
        {
            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOwnerCodeUnderOrderBlockIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOwnerCodeUnderOrderBlockIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOfferItemRefIdIsMissingInOrderChangeRequest()
        {
            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOfferItemRefIdIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenPaxRefIdIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenPaxRefIdIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOrderIdIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOrderIdIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOwnerCodeUnderSelectedPricedOfferBlockIsNullInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public async Task ValidateErrorMessageWhenOwnerCodeUnderSelectedPricedOfferBlockIsMissingInOrderChangeRequest()
        {

            string myXmlRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrderChangeRQ.xml"));

            XmlDocument updatedOrderChangeRequest = new XmlDocument();

             XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            updatedOrderChangeRequest.LoadXml(myXmlRequest);

            updatedOrderChangeRequest.SelectSingleNode("//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer//ct:SelectedOfferItem//ct:PaxRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            dynamic updatedorderChangeResult = JsonConvert.DeserializeObject<dynamic>(await orderChangeResponse.Content.ReadAsStringAsync());

            string errorcode = updatedorderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = updatedorderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'DataLists' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrderChangeParameters, PaxGroup, PaymentFunctions' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 1644";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        private static void PaymentFunctionsDataAddition(IATA_OrderChangeRQ instance)
        {
            //Adding PaymentFunctions blocks
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.CurCode = "GBP";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.Text = "0.00";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = "42639X1LFFPQ1307";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardSecurityCode = "123";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = "VI";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.ExpirationDate = "1223";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = "ODQzNjgwNjU0ZjM3N2JmYTg0NTM";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = "Y";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = "02";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = "c272b04f-6e7b-43a2-bb78-90f4fb94aa25";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = "CardEnrolled3DS2.1.0";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.StreetText.AddRange(new string[] { "8", "Cobbles Crescent" });
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.PostalCode = "RH108HA";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CityName = "Crawley";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CountryCode = "GB";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.GivenName = "David";
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.Surname = "Smith";

        }

        private static void DataListsDataAddition(IATA_OrderChangeRQ instance)
        {
            //Adding DataLists blocks
            instance.Request.DataLists.ContactInfoList.ContactInfo.ContactInfoID = "CID_1";
            instance.Request.DataLists.ContactInfoList.ContactInfo.EmailAddress.EmailAddressText = "test@virgin.com";
            instance.Request.DataLists.ContactInfoList.ContactInfo.Phone.ContactTypeText = "HOME";
            instance.Request.DataLists.ContactInfoList.ContactInfo.Phone.CountryDialingCode = "GB";
            instance.Request.DataLists.ContactInfoList.ContactInfo.Phone.PhoneNumber = "7890654321";
            instance.Request.DataLists.PaxList.Pax.Add(new Pax()
            {
                ContactInfoRefID = "CID_1",
                Individual = new Individual()
                {
                    Birthdate = "1995-05-18",
                    GenderCode = "M",
                    GivenName = "David",
                    IndividualID = "IND1",
                    Surname = "Smith",
                    TitleName = "Mr"
                },
                PaxID = "ADULT_1",
                PTC = "ADT"
            });
        }
    }
}
