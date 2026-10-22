using System.Globalization;
using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderChange.ErrorMessageValidation
{
    public class OrderChangeMandatoryFieldValidationV2 : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        

        public OrderChangeMandatoryFieldValidationV2(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOrdinalIsMissingInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create XmlNamespaceManager after loading the XML document
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            orderChangeRequestDoc.SelectSingleNode("//def:Ordinal", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            // Directly access the first "Code" and "DescText" nodes
            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            // Expected response based on the XML error message you provided
            string expectedResponse = "Ordinal/OrgRole/Participant Name is mandatory in the request";

            // Validate error message and error code
            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected {expectedResponse} but was {responseText}");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected 912 but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOrdinalValueIsNullInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create XmlNamespaceManager after loading the XML document
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Set the Ordinal value to an empty string to simulate the invalid condition
            orderChangeRequestDoc.SelectSingleNode("//def:Ordinal", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            // Directly access the first "Code" and "DescText" nodes
            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            // Expected response based on the XML error message you provided
            string expectedResponse = "Ordinal/OrgRole/Participant Name is mandatory in the request";

            // Validate error message and error code
            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected {expectedResponse} but was {responseText}");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected 912 but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOrgRoleIsNullInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create XmlNamespaceManager after loading the XML document
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Set the OrgRole value to an empty string to simulate the invalid condition
            orderChangeRequestDoc.SelectSingleNode("//def:OrgRole", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            // Directly access the first "Code" and "DescText" nodes
            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            // Expected response based on the XML error message you provided
            string expectedResponse = "Please complete all mandatory fields in request as per given specifications";

            // Validate error message and error code
            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected {expectedResponse} but was {responseText}");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected 912 but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenParticipantOrgNameIsMissingInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create XmlNamespaceManager after loading the XML document
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Set the OrgRole value to an empty string to simulate the invalid condition
            orderChangeRequestDoc.SelectSingleNode("//def:OrgRole", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            // Directly access the first "Code" and "DescText" nodes
            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            // Expected response based on the XML error message you provided
            string expectedResponse = "Ordinal/OrgRole/Participant Name is mandatory in the request";

            // Validate error message and error code
            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected {expectedResponse} but was {responseText}");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected 912 but was {code}");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenParticipantOrgNameIsNullInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create XmlNamespaceManager after loading the XML document
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Set the Participant Org Name to an empty string to simulate the invalid condition
            orderChangeRequestDoc.SelectSingleNode("//def:ParticipatingOrg//def:Name", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            // Directly access the first "Code" and "DescText" nodes
            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            // Expected response based on the XML error message you provided
            string expectedResponse = "Ordinal/OrgRole/Participant Name is mandatory in the request";

            // Validate error message and error code
            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOfferRefIdIsMissingInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create XmlNamespaceManager after loading the XML document
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Set the PaxRefID value to an empty string to simulate the invalid condition
            orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:PaxRefID", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            // Directly access the first "Code" and "DescText" nodes
            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            // Expected response based on the XML error message you provided
            string expectedResponse = "OfferRefID is mandatory in the request";

            // Validate error message and error code
            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOfferRefIdIsNullInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();
            
            // Set the PaxRefID to an empty string to simulate the invalid condition
            orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:PaxRefID", orderChangeRequestnsManager).InnerText = "";

            // Send the request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "OfferRefID is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOfferRefIdIsExpiredInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();
            
            // Set the PaxRefID to an invalid value to simulate the expired condition
            orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:PaxRefID", orderChangeRequestnsManager).InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderChangeRQ)";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("913"), $"Error Code for Order change error message is not as expected. Expected '913' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOwnerCodeUnderOrderBlockIsMissingInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            XmlNode ownerCodeNode = orderChangeRequestDoc.SelectSingleNode("//def:OrderBlock//def:OwnerCode", orderChangeRequestnsManager);
            if (ownerCodeNode != null) ownerCodeNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;
            string expectedResponse = "Owner Code is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOwnerCodeUnderOrderBlockIsNullInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            XmlNode ownerCodeNode = orderChangeRequestDoc.SelectSingleNode("//def:OrderBlock//def:OwnerCode", orderChangeRequestnsManager);
            if (ownerCodeNode != null) ownerCodeNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "Owner Code is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOfferItemRefIdIsMissingInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            XmlNode offerItemRefIdNode = orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:OfferItemRefID", orderChangeRequestnsManager);
            if (offerItemRefIdNode != null) offerItemRefIdNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "OfferItemRefID is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOfferItemRefIdIsNullInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            XmlNode offerItemRefIdNode = orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:OfferItemRefID", orderChangeRequestnsManager);
            if (offerItemRefIdNode != null) offerItemRefIdNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "OfferItemRefID is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenPaxRefIdIsMissingInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            XmlNode paxRefIdNode = orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:PaxRefID", orderChangeRequestnsManager);
            if (paxRefIdNode != null) paxRefIdNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "PaxList is mandatory in the request.";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenPaxRefIdIsNullInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Simulate PaxRefID being null
            XmlNode paxRefIdNode = orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:PaxRefID", orderChangeRequestnsManager);
            if (paxRefIdNode != null) paxRefIdNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "PaxList is mandatory in the request.";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOrderIdIsMissingInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Simulate OrderID being missing
            XmlNode orderIdNode = orderChangeRequestDoc.SelectSingleNode("//def:OrderID", orderChangeRequestnsManager);
            if (orderIdNode != null) orderIdNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "OrderID is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOrderIdIsNullInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Simulate OrderID being null
            XmlNode orderIdNode = orderChangeRequestDoc.SelectSingleNode("//def:OrderID", orderChangeRequestnsManager);
            if (orderIdNode != null) orderIdNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "OrderID is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOwnerCodeUnderSelectedPricedOfferBlockIsNullInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Simulate Owner Code being null
            XmlNode ownerCodeNode = orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:OwnerCode", orderChangeRequestnsManager);
            if (ownerCodeNode != null) ownerCodeNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "Owner Code is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenOwnerCodeUnderSelectedPricedOfferBlockIsMissingInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);

            // Create and configure XmlNamespaceManager
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Simulate Owner Code being missing
            XmlNode ownerCodeNode = orderChangeRequestDoc.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer//def:SelectedOfferItem//def:OwnerCode", orderChangeRequestnsManager);
            if (ownerCodeNode != null) ownerCodeNode.InnerText = "";

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeResponseDoc = new XmlDocument();
            orderChangeResponseDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeResponseDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeResponseDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "Owner Code is mandatory in the request";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");
            Assert.True(code.Equals("912"), $"Error Code for Order change error message is not as expected. Expected '912' but was '{code}'");
        }

        // pax validation for v2

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenPaxlistIsNotPresentInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaxList", orderChangeRequestnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeRequestDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeRequestDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "PaxList is mandatory in the request.";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order Change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");

            Assert.True(code.Equals("912"), $"Error Code for Order Change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenPaxlistIsNotProperlyDefinedInDatalist()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:PaxList//def:Pax[2]", orderChangeRequestnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeRequestDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeRequestDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "Invalid PaxRefID for the SelectedOffer/OfferID (Call again OrderChangeRQ)";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order Change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");

            Assert.True(code.Equals("913"), $"Error Code for Order Change error message is not as expected. Expected '913' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenPaxlistIsNotProperlyDefinedInSelectedOfferItem()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderChangeRequest.SelectSingleNode("//def:SelectedPricedOffer//def:SelectedOfferItem[2]/def:PaxRefID", orderChangeRequestnsManager);
            nodeToRemove?.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeRequestDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeRequestDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "PaxList is mandatory in the request.";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order Change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");

            Assert.True(code.Equals("912"), $"Error Code for Order Change error message is not as expected. Expected '912' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageWhenPaxlistIsMismatchWithSelectedOfferItemInOrderChangeRequest()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            string paxRefId_1 = updatedOrderChangeRequest.SelectSingleNode("//def:SelectedPricedOffer/def:SelectedOfferItem[2]/def:PaxRefID", orderChangeRequestnsManager).InnerText;
            string paxRefId_2 = updatedOrderChangeRequest.SelectSingleNode("//def:SelectedPricedOffer/def:SelectedOfferItem[3]/def:PaxRefID", orderChangeRequestnsManager).InnerText;

            // Swap the values
            updatedOrderChangeRequest.SelectSingleNode("//def:SelectedPricedOffer/def:SelectedOfferItem[3]/def:PaxRefID", orderChangeRequestnsManager).InnerText = paxRefId_1;
            updatedOrderChangeRequest.SelectSingleNode("//def:SelectedPricedOffer/def:SelectedOfferItem[2]/def:PaxRefID", orderChangeRequestnsManager).InnerText = paxRefId_2;

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeResult.ToString());

            string code = orderChangeRequestDoc.GetElementsByTagName("Code")[0]?.InnerText;
            string responseText = orderChangeRequestDoc.GetElementsByTagName("DescText")[0]?.InnerText;

            string expectedResponse = "Invalid PaxRefID for the SelectedOffer/OfferID (Call again OrderChangeRQ)";

            Assert.True(responseText.Has(expectedResponse), $"Text value for Order Change error message is not as expected. Expected '{expectedResponse}' but was '{responseText}'");

            Assert.True(code.Equals("913"), $"Error Code for Order Change error message is not as expected. Expected '913' but was '{code}'");
        }

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateSuccessResponseWhenPtcAndIndividualRemovedFromDatalist()
        {
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument updatedOrderChangeRequest = new XmlDocument();
            updatedOrderChangeRequest.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestnsManager = updatedOrderChangeRequest.CreateXmlNamespaceManager();

            XmlNodeList nodesToRemove_1 = updatedOrderChangeRequest.SelectNodes("//def:PaxList//def:Pax//def:PTC", orderChangeRequestnsManager);
            foreach (XmlNode nodeToRemove in nodesToRemove_1)
            {
                XmlNode parent = nodeToRemove.ParentNode;
                parent.RemoveChild(nodeToRemove);
            }

            XmlNodeList nodesToRemove_2 = updatedOrderChangeRequest.SelectNodes("//def:PaxList//def:Pax//def:Individual", orderChangeRequestnsManager);
            foreach (XmlNode nodeToRemove in nodesToRemove_2)
            {
                XmlNode parent = nodeToRemove.ParentNode;
                parent.RemoveChild(nodeToRemove);
            }

            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);
            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Change - response was {orderChangeResult}");

            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            XmlNamespaceManager orderChangeResponseManager = orderChangeResponseXmlDocument.CreateXmlNamespaceManager();
            orderChangeResponseXmlDocument.LoadXml(orderChangeResult);

            string differentialTypeCode = orderChangeResponseXmlDocument.SelectSingleNode("//def:Order/def:OrderItem[1]/def:PriceDifferential/def:DifferentialTypeCode", orderChangeResponseManager)?.InnerText;

            Assert.True(differentialTypeCode.Equals("EvenExchange"), $"DifferentialTypeCode is not as expected in Order Change Response. Expected 'EvenExchange' but was '{differentialTypeCode}'");
        }

        //APIM Validations 

        [Trait("Category", "OrderChangeAllRegression")]
        [Fact]
        public void ValidateErrorMessageForAPIMValidationInOrderChangeRequest()
        {
            // Load the OrderChangeRQ XML from the file or method
            string orderChangeRequest = NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney();

            XmlDocument orderChangeRequestDoc = new XmlDocument();
            orderChangeRequestDoc.LoadXml(orderChangeRequest);
            XmlNamespaceManager orderChangeRequestnsManager = orderChangeRequestDoc.CreateXmlNamespaceManager();

            // Locate the Ordinal node and clone it
            XmlNode nodeToRemove = orderChangeRequestDoc.SelectSingleNode("//def:Ordinal", orderChangeRequestnsManager);
            XmlNode nodeToSwap = nodeToRemove.CloneNode(true);

            // Remove the original Ordinal node
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            // Locate the OrgRole node
            XmlNode selectNode = orderChangeRequestDoc.SelectSingleNode("//def:OrgRole", orderChangeRequestnsManager);

            // Append the cloned Ordinal node after the OrgRole node
            selectNode.ParentNode.AppendChild(nodeToSwap);

            // Send the modified request
            HttpResponseMessage orderChangeResponse = NDCTransformationEndpoints.OrderChange(orderChangeRequestDoc.OuterXml);
            //dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            dynamic orderChangeResult = JsonConvert.DeserializeObject<dynamic>(orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string errorcode = orderChangeResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderChangeResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        private string NDC_OrderCreateAndGetTheOrderChangeRequestSingleJourney()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 50;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, deptDate: journey1Date,
                cabinTypeCodeOne: "5", numberOfadults: 1);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestorderChangeRequestnsManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            string originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria//def:Date", airShoppingRequestorderChangeRequestnsManager).InnerText;

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelightFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseorderChangeRequestnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseorderChangeRequestnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            XmlNamespaceManager orderCreateRequestorderChangeRequestnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestorderChangeRequestnsManager).InnerText = "testemail@fly.virgin.com";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            XmlNamespaceManager orderCreateorderChangeRequestnsManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string bookingId = updatedOrderCreateResponse.SelectSingleNode("//def:BookingID", orderCreateorderChangeRequestnsManager).InnerText;

            //Order Retrieve 
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();
            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from orderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve PNR sent was {bookingId}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);
            
            //Adding namespace manager
            XmlNamespaceManager orderRetrieveResponseorderChangeRequestnsManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//def:OrderID", orderRetrieveResponseorderChangeRequestnsManager).InnerText;

            int countOrderItemID = 1;

            int counter = 0;
            string orderReshopRequest = "";
            string orderReshopResult = "";
            IDictionary<string, string> reshopOfferDetails = new Dictionary<string, string>();

            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            do
            {

                DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime newDate = date.AddDays(counter);

                string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");

                //Generating the Order Reshop Request

                orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, bookingId, countOrderItemID, JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.ATL, newCabinTypeCodeOne: "5", deptDate: reshopJourney1Date, flighttype: "Direct");

                //Post Order Reshop request
                HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                Assert.True(orderReshopResponse.IsSuccessStatusCode, $"Success status code was not returned from orderReshop Response, returned: {orderReshopResponse.StatusCode} with message: {orderReshopResponse.ReadResponseAsString()}" + $"OrderReshop Request sent was {orderReshopRequest}");

                //Getting the Order Reshop response and converting to XML Document
                orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                //Find offer based on Even Exchange
                reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, "EvenExchange");

                counter++;

                if (reshopOfferDetails.Count > 0)
                {
                    break;
                }

            } while (counter < 6);
            Assert.True(reshopOfferDetails.Count > 0, "ReshopOfferDetails did not return any Offers with Even Exchange");
            string reshopOfferRefId = reshopOfferDetails["OfferRefID"];
            string reshopTotalAmount = reshopOfferDetails["TotalAmount"];

            string orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            //Post Order Quote request
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest);

            Assert.True(orderQuoteResponse.IsSuccessStatusCode, $"Success status code was not returned from orderQuote Response, returned: {orderQuoteResponse.StatusCode} with message: {orderQuoteResponse.ReadResponseAsString()}" + $"OrderQuote Request sent was {orderQuoteRequest}");

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            string orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, bookingId, orderId);

            //Post Order Create request
            return orderChangeRequest;
        }

    }
}
