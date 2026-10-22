using System.Xml;
using Microsoft.Graph.Models;
using Microsoft.Web.Services3.Addressing;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.OrderReshop.OrderReshopErrorValidations
{
    public class OrderReshopErrorValidations : IClassFixture<MemoryStoreFixture>
    {

        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgIdTravelRepublic = "91281035";
        public static string participatingOrgIdOasisTravel = "35200056";
        public static string invalidorgID = "ABCD*&^";

        public OrderReshopErrorValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(participatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgIdOasisTravel, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgIdOasisTravel}-OcpSubID"));

        }

        [Trait("Category", "NDC-ProdSanity")]
        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void Reshop_OrdinalValueZero()
        {
            string request = string.Format(System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml"));

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "912", "Invalid ordinal number - should start from 1", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void Reshop_InvalidSubscriptionkey()
        {
            string request = string.Format(System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml"));

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:ParticipatingOrg//ct:OrgID", xmlNamespaceManager).InnerText = "1234567";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();
            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");
            
            string errordesc = orderReshopResult.message?.ToString();
            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";
            
            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void Reshop_InvalidBookingID()
        {
            string request = string.Format(System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml"));

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = updatedRequest.CreateXmlNamespaceManager();

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("129"), $"Error code expected is 129 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("No PNR match found"), $"Error code expected is No PNR match found but actual is {descText}");

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "129", "No PNR match found", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }

        [Trait("Category", "OrderReshopRegression")]
        [Fact]
        public void Reshop_InCorrectBookingID()
        {
            string request = string.Format(System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml"));

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = updatedRequest.CreateXmlNamespaceManager();

            updatedRequest.SelectSingleNode("//ct:BookingRef//ct:BookingID", xmlNamespaceManager).InnerText = "1234";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderReshop(updatedRequest.OuterXml);

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(resultDoc, "129", "No PNR or Order ID match found", out ValidationResultHelper result1), "Validation failed. Reasons: " + string.Join(", ", result1.Message));
        }
       
        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderReshop_TC1_InvalidDataformat()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            OrderReshopRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager).InnerText = "ABCD";

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABCD' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABCD' is not a valid Integer value";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderReshop_TC2_Incorrectorder()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            XmlNode nodeToSwap = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:Ordinal", xmlNamespaceManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:OrgRole", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderReshop_TC3_InvalidOrgID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            OrderReshopRequestXmlDocument.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "ABC#$#$D";

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(invalidorgID));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void TC4_ReorderOriginDep()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:OriginDepCriteria", xmlNamespaceManager);
            XmlNode nodeToSwap = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:OriginDepCriteria", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:DestArrivalCriteria", xmlNamespaceManager);
            selectNode.ParentNode.AppendChild(nodeToSwap);

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'BookingRef' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'PaxJourneyList, PayloadRestrictions, PaymentFunctions, ResponseParameters, UpdateOrder' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void TC5_InvalidCabintype()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            OrderReshopRequestXmlDocument.SelectSingleNode("//ct:CabinTypeCode", xmlNamespaceManager).InnerText = "A70B^CD";

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'BookingRef' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'PaxJourneyList, PayloadRestrictions, PaymentFunctions, ResponseParameters, UpdateOrder' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void TC6_InvalidPrefLevelCode()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            OrderReshopRequestXmlDocument.SelectSingleNode("//ct:PrefLevelCode", xmlNamespaceManager).InnerText = "1289^0";

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'BookingRef' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'PaxJourneyList, PayloadRestrictions, PaymentFunctions, ResponseParameters, UpdateOrder' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void TC7_RemoveBookingID()
        {
            string request = System.IO.File.ReadAllText("Requests\\Body\\OrdeReshopRQ.xml");

            XmlDocument OrderReshopRequestXmlDocument = new XmlDocument();
            OrderReshopRequestXmlDocument.LoadXml(request);

            XmlNamespaceManager xmlNamespaceManager = OrderReshopRequestXmlDocument.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = OrderReshopRequestXmlDocument.SelectSingleNode("//ct:BookingID", xmlNamespaceManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            //Post order Reshop Result
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(OrderReshopRequestXmlDocument.OuterXml, memoryStoreFixture.Get<string>(participatingOrgIdTravelRepublic));

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Request' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage' has invalid child element 'BookingRef' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'PaxJourneyList, PayloadRestrictions, PaymentFunctions, ResponseParameters, UpdateOrder' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }
    }
}
