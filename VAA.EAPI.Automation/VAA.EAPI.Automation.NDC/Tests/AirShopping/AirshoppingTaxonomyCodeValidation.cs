using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.AirShopping
{
    public class AirshoppingTaxonomyCodeValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";

        public AirshoppingTaxonomyCodeValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC01AirShopping_Taxonomy_13EC_true_189C_true()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            // Explicitly remove FareCriteria from the request so this taxonomy test continues
            // to exercise the intended offer filtering behavior when the request template evolves.
            XmlNodeList fareCriteriaNodes = updatedAirShoppingRequest.SelectNodes("//def:OfferCriteria//def:FareCriteria", airShoppingRequestXmlnsManager);
            if (fareCriteriaNodes != null)
            {
                foreach (XmlNode fareCriteriaNode in fareCriteriaNodes)
                {
                    fareCriteriaNode.ParentNode?.RemoveChild(fareCriteriaNode);
                }
            }
            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            Assert.True(AirshoppingValidator.ValidateTaxonomyCode(airShoppingRequestDocument, airShoppingResponseXmlDocument, true, true, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC02AirShopping_Taxonomy_13EC_false_189C_false()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria//def:IncludeInd", airShoppingRequestXmlnsManager).InnerText = "false";

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            Assert.True(AirshoppingValidator.ValidateTaxonomyCode(airShoppingRequestDocument, airShoppingResponseXmlDocument, false, false, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC03AirShopping_Taxonomy_13EC_true()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemnode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria//def:TaxonomyCode[1]", airShoppingRequestXmlnsManager);

            itemnode2.ParentNode.RemoveChild(itemnode2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            Assert.True(AirshoppingValidator.ValidateTaxonomyCode(airShoppingRequestDocument, airShoppingResponseXmlDocument, true, false, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC04AirShopping_Taxonomy_189C_true()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemnode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria//def:TaxonomyCode[2]", airShoppingRequestXmlnsManager);

            itemnode2.ParentNode.RemoveChild(itemnode2);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //validate xsd against response
            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            Assert.True(AirshoppingValidator.ValidateTaxonomyCode(airShoppingRequestDocument, airShoppingResponseXmlDocument, false, true, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC05AirShopping_Taxonomy_189C_true_13EC_false()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemnode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria//def:TaxonomyCode[2]", airShoppingRequestXmlnsManager);

            itemnode2.ParentNode.RemoveChild(itemnode2);

            //Node to add service criteria and Include ind false
            XmlNode itemNode3 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria", airShoppingRequestXmlnsManager);
            //Adding Service criteria in the Offer criteria
            XmlElement newElementServCriteria = updatedAirShoppingRequest.CreateElement("ServiceCriteria", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            itemNode3.AppendChild(newElementServCriteria);

            XmlNode itemNode4 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria[2]", airShoppingRequestXmlnsManager);
            //Adding Service criteria in the Offer criteria
            XmlElement newElementIncludeInd = updatedAirShoppingRequest.CreateElement("IncludeInd", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElementIncludeInd.InnerText = "false";
            itemNode4.AppendChild(newElementIncludeInd);

            //Adding taxonomy code
            XmlNode itemNode5 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria[2]", airShoppingRequestXmlnsManager);
            //Adding Service criteria in the Offer criteria
            XmlElement newElementTaxoCode = updatedAirShoppingRequest.CreateElement("TaxonomyCode", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElementTaxoCode.InnerText = "13EC";
            itemNode5.AppendChild(newElementTaxoCode);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic));

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");

            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("496"),
           $"Text value for air shopping error message is not as expected. Expected 496 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("No Offers found"),
            $"Error Code for air shopping error message is not as expected. Expected No Offers found " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task TC06AirShopping_Taxonomy_189C_false_13EC_true()
        {
            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "2", numberOfadults: 1, numberOfinfants: 1, numberOfchildren: 1, numberOfyouths: 1, taxonomy: "taxonomy");

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemnode2 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria//def:TaxonomyCode[1]", airShoppingRequestXmlnsManager);

            itemnode2.ParentNode.RemoveChild(itemnode2);

            //Node to add service criteria and Include ind false
            XmlNode itemNode3 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria", airShoppingRequestXmlnsManager);
            //Adding Service criteria in the Offer criteria
            XmlElement newElementServCriteria = updatedAirShoppingRequest.CreateElement("ServiceCriteria", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            itemNode3.AppendChild(newElementServCriteria);

            XmlNode itemNode4 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria[2]", airShoppingRequestXmlnsManager);
            //Adding Service criteria in the Offer criteria
            XmlElement newElementIncludeInd = updatedAirShoppingRequest.CreateElement("IncludeInd", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElementIncludeInd.InnerText = "false";
            itemNode4.AppendChild(newElementIncludeInd);

            //Adding taxonomy code
            XmlNode itemNode5 = updatedAirShoppingRequest.SelectSingleNode("//def:OfferCriteria//def:ServiceCriteria[2]", airShoppingRequestXmlnsManager);
            //Adding Service criteria in the Offer criteria
            XmlElement newElementTaxoCode = updatedAirShoppingRequest.CreateElement("TaxonomyCode", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            newElementTaxoCode.InnerText = "189C";
            itemNode5.AppendChild(newElementTaxoCode);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument airShoppingRequestDocument = new XmlDocument();
            airShoppingRequestDocument.LoadXml(airShoppingRequest);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(XsdValidator.ValidateAirShoppingResponse(airShoppingResult, out string[] airShoppingResponseExceptions), string.Join(Environment.NewLine, airShoppingResponseExceptions));

            Assert.True(AirshoppingValidator.ValidateTaxonomyCode(airShoppingRequestDocument, airShoppingResponseXmlDocument, false, true, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }
    }
}
