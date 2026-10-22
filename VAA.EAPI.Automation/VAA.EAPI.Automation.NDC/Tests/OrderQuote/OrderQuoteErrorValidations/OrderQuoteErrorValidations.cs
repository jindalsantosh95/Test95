using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderQuote.OrderQuoteErrorValidations
{
    public class OrderQuoteErrorValidations : IClassFixture<MemoryStoreFixture>
    {

        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgIdTravelRepublic = "91281035";
        public static string participatingOrgIdOasisTravel = "35200056";
        public static string invalidorgID = "ABCD*&^";

        public OrderQuoteErrorValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(participatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgIdOasisTravel, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgIdOasisTravel}-OcpSubID"));

        }


        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void IncorrectOrderID()
        {
            string orderQuoteRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrdeQuoteRQ.xml"));

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            XmlNamespaceManager orderQuoteRequestXmlnsManager = updatedOrderQuoteRequest.CreateXmlNamespaceManager();

            updatedOrderQuoteRequest.SelectSingleNode("//ct:OrderID", orderQuoteRequestXmlnsManager).InnerText = "!@#$";

            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            dynamic orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderQuoteResultDoc = new XmlDocument();

            orderQuoteResultDoc.LoadXml(orderQuoteResult);

            XmlNodeList code = orderQuoteResultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("129"), $"Error code expected is 129 but actual is {code}");

            XmlNodeList descText = orderQuoteResultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("No PNR or Order ID match found"), $"Error code expected is No PNR or Order ID match found but actual is {descText[0].InnerText}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void IncorrectOwnerCode()
        {
            string orderQuoteRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrdeQuoteRQ.xml"));

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            XmlNamespaceManager orderQuoteRequestXmlnsManager = updatedOrderQuoteRequest.CreateXmlNamespaceManager();

            updatedOrderQuoteRequest.SelectSingleNode("//ct:OwnerCode", orderQuoteRequestXmlnsManager).InnerText = "ABCD";

            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            dynamic orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderQuoteResultDoc = new XmlDocument();

            orderQuoteResultDoc.LoadXml(orderQuoteResult);

            XmlNodeList code = orderQuoteResultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = orderQuoteResultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Enter a valid Order ID with owner code"), $"Error code expected is Enter a valid Order ID with owner code but actual is {descText[0].InnerText}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void InvalidOfferID()
        {
            string orderQuoteRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrdeQuoteRQ.xml"));

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            XmlNamespaceManager orderQuoteRequestXmlnsManager = updatedOrderQuoteRequest.CreateXmlNamespaceManager();

            updatedOrderQuoteRequest.SelectSingleNode("//ct:OfferRefID", orderQuoteRequestXmlnsManager).InnerText = "@#$%";

            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            dynamic orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderQuoteResultDoc = new XmlDocument();

            orderQuoteResultDoc.LoadXml(orderQuoteResult);

            XmlNodeList code = orderQuoteResultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = orderQuoteResultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid Offer ID/OfferItemID for the Selected Offer"), $"Error code expected is Invalid Offer ID/OfferItemID for the Selected Offer but actual is {descText[0].InnerText}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void InvalidOfferItemID()
        {
            string orderQuoteRequest = string.Format(System.IO.File.ReadAllText("requests\\Body\\OrdeQuoteRQ.xml"));

            XmlDocument updatedOrderQuoteRequest = new XmlDocument();

            updatedOrderQuoteRequest.LoadXml(orderQuoteRequest);

            XmlNamespaceManager orderQuoteRequestXmlnsManager = updatedOrderQuoteRequest.CreateXmlNamespaceManager();

            updatedOrderQuoteRequest.SelectSingleNode("//ct:OfferItemRefID", orderQuoteRequestXmlnsManager).InnerText = "@#$%";

            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(updatedOrderQuoteRequest.OuterXml);

            dynamic orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderQuoteResultDoc = new XmlDocument();

            orderQuoteResultDoc.LoadXml(orderQuoteResult);

            XmlNodeList code = orderQuoteResultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = orderQuoteResultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid Offer ID/OfferItemID for the Selected Offer"), $"Error code expected is Invalid Offer ID/OfferItemID for the Selected Offer but actual is {descText[0].InnerText}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void OrderQuote_TC1_InvalidDataformat()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            OrderQuoteRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderQuoteRequestXmlDocument.CreateXmlNamespaceManager();

            OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post order Quote Result
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(OrderQuoteRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic orderQuoteResult = JsonConvert.DeserializeObject<dynamic>(orderQuoteResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderQuoteResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderQuoteResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void OrderQuote_TC2_Incorrectorder()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            OrderQuoteRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderQuoteRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post order Quote Result
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(OrderQuoteRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic orderQuoteResult = JsonConvert.DeserializeObject<dynamic>(orderQuoteResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderQuoteResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderQuoteResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void OrderQuote_TC3_InvalidOrgID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            OrderQuoteRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderQuoteRequestXmlDocument.CreateXmlNamespaceManager();

            OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "ABC#$#$D";

            //Post order Quote Result
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(OrderQuoteRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(invalidorgID));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic orderQuoteResult = JsonConvert.DeserializeObject<dynamic>(orderQuoteResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderQuoteResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = orderQuoteResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void TC4_ReorderPaxlist()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            OrderQuoteRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderQuoteRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:PaxID", xmlNamespaceManager);
            XmlNode nodeToSwap = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:PaxID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:PTC", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post order Quote Result
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(OrderQuoteRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic orderQuoteResult = JsonConvert.DeserializeObject<dynamic>(orderQuoteResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderQuoteResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderQuoteResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'PaxList' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'PTC' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'AgeMeasure, Birthdate, CitizenshipCountryCode, ContactInfoRefID, Employer, FOID, IdentityDoc, Individual, LangUsage, LoyaltyProgramAccount, PaxID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. ";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void TC5_ReorderSelectedOffer()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            OrderQuoteRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderQuoteRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:OfferRefID", xmlNamespaceManager);
            XmlNode nodeToSwap = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:OfferRefID", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:PaxRefID", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post order Quote Result
            HttpResponseMessage orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(OrderQuoteRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OfferPriceresponse and converting to XML Document
            dynamic orderQuoteResult = JsonConvert.DeserializeObject<dynamic>(orderQuoteResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderQuoteResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderQuoteResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedOffers' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OfferRefID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderQuoteRegression")]
        [Fact]
        public void OrderQuote_TC6_RemoveOrderID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeQuoteRQ.xml");

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            OrderQuoteRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderQuoteRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderQuoteRequestXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            //Post order Reshop Result
            HttpResponseMessage OrderQuoteResponse = NDCTransformationEndpoints.OrderQuote(OrderQuoteRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderQuoteresponse and converting to XML Document
            dynamic OrderQuoteResult = JsonConvert.DeserializeObject<dynamic>(OrderQuoteResponse.Content.ReadAsStringAsync().Result);

            string errorcode = OrderQuoteResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = OrderQuoteResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'ExistingOrder' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'AffectedOrderItem, OrderID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }
    }
}
