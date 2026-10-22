using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class SeatAvailabilityPrimeSaleRequestValidations
    {


        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_IncorrectAgent()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            updatedSeatAvailabilityRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ParticipatingOrg//ct:OrgID", updatedSeatResponseXmlnsManager).InnerText = "abcd8967";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the Seatavailability Result
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_SeatAvailabilityIncorrectOfferID()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            updatedSeatAvailabilityRequest.SelectSingleNode("//ct:SeatAvailCoreRequest//ct:OfferRequest//ct:Offer//ct:OfferID", updatedSeatResponseXmlnsManager).InnerText = "TESTOFFERID";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatAvailabilityResponse = new XmlDocument();

            updatedSeatAvailabilityResponse.LoadXml(seatAvailabilityResult);

            XmlNodeList code = updatedSeatAvailabilityResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = updatedSeatAvailabilityResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid or Timed-out OfferID entered"), $"Error code expected is Invalid or Timed-out OfferID entered but actual is {descText}");

            //Xunit.Sdk.TrueException: 'Error code expected is Invalid OfferItemID for the SelectedOffer/OfferID but actual is System.Xml.XmlElementList'

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingAgentName()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedSeatAvailabilityRequest.SelectSingleNode("//ct:DistributionChainLink//ct:ParticipatingOrg//ct:Name", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            string SeatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSeatAvailabilityResponse = new XmlDocument();

            updatedSeatAvailabilityResponse.LoadXml(SeatAvailabilityResult);

            XmlNodeList code = updatedSeatAvailabilityResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = updatedSeatAvailabilityResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid or Timed-out OfferID entered"), $"Error code expected is Invalid or Timed-out OfferID entered but actual is {descText}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOrgRole()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedSeatAvailabilityRequest.SelectSingleNode("//ct:DistributionChainLink//ct:OrgRole", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the seat availability result
            dynamic SeatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = SeatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = SeatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'ParticipatingOrg' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 340";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_InvalidOrgRole()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            updatedSeatAvailabilityRequest.SelectSingleNode("//ct:DistributionChainLink//ct:OrgRole", updatedSeatResponseXmlnsManager).InnerText = "acbcjk&";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the Seatavailability Result
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrgRole' element is invalid - The value 'acbcjk&' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:DistributionChainRoleCodeType' - The Enumeration constraint failed. Line: 1, Position: 361";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_InvalidOrdinal()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            updatedSeatAvailabilityRequest.SelectSingleNode("//ct:DistributionChainLink//ct:Ordinal", updatedSeatResponseXmlnsManager).InnerText = "$#ABC7890";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the Seatavailability Result
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value '$#ABC7890' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string '$#ABC7890' is not a valid Integer value. Line: 1, Position: 339";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOrdinal()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedSeatAvailabilityRequest.SelectSingleNode("//ct:DistributionChainLink//ct:Ordinal", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the SeatAvailabilityResult
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'ContactInfo, Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 320";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_InvalidOwnerCode()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            updatedSeatAvailabilityRequest.SelectSingleNode("//ct:SeatAvailCoreRequest//ct:OfferRequest//ct:Offer//ct:OwnerCode", updatedSeatResponseXmlnsManager).InnerText = "TESTOWNERCODE";

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updateSeatAvailabilityResponse = new XmlDocument();

            updateSeatAvailabilityResponse.LoadXml(seatAvailabilityResult);

            XmlNodeList code = updateSeatAvailabilityResponse.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = updateSeatAvailabilityResponse.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid or Timed-out OfferID entered"), $"Error code expected is Invalid or Timed-out OfferID entered but actual is {descText}");

            //{System.Xml.XmlElementList}

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOfferID()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedSeatAvailabilityRequest.SelectSingleNode("//ct:SeatAvailCoreRequest//ct:OfferRequest//ct:Offer", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the seatavailability Result
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'OfferRequest' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has incomplete content. List of possible elements expected: 'Offer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 616";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", "SeatAvailabilityAllRegression")]
        [Fact]
        public void SeatAvailability_PSSP_MissingOwnerCode()
        {
            string seatAvailabilityRequest = Core.Extensions.FileExtensions.ReadFile("SeatAvailabilityOfferRQ", "xml");

            XmlDocument updatedSeatAvailabilityRequest = new XmlDocument();

            updatedSeatAvailabilityRequest.LoadXml(seatAvailabilityRequest);

            XmlNamespaceManager updatedSeatResponseXmlnsManager = updatedSeatAvailabilityRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedSeatAvailabilityRequest.SelectSingleNode("//ct:SeatAvailCoreRequest//ct:OfferRequest//ct:Offer//ct:OwnerCode", updatedSeatResponseXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(updatedSeatAvailabilityRequest.OuterXml);

            //Getting the Seat AvailabilityResponse 
            dynamic seatAvailabilityResult = JsonConvert.DeserializeObject<dynamic>(seatAvailabilityResponse.Content.ReadAsStringAsync().Result);

            string errorcode = seatAvailabilityResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = seatAvailabilityResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'Offer' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has incomplete content. List of possible elements expected: 'OfferItem, OwnerCode' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. Line: 1, Position: 704";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }
    }
}

