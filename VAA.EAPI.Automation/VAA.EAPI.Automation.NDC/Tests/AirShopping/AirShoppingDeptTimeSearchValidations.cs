using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingDeptTimeSearchValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public static string participatingOrgOrgIdTravelRepublic = "91281035";

        public AirShoppingDeptTimeSearchValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_RemoveTimeAfterDurationField()
        {
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            itemNode1.AppendChild(newElement1);

            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement3.InnerText = "PT2H";

            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");

            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_RemoveTimeBeforeDurationField()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            itemNode1.AppendChild(newElement1);

            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement2.InnerText = "PT2H";

            itemNode1.AppendChild(newElement2);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            Console.WriteLine(code[0].InnerText + " Code");

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Console.WriteLine(descText[0].InnerText + " descText");

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_RemoveTimeBeforeAndAfterDurationFields()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            Console.WriteLine("afterDepTimeHours" + afterDepTimeHours);

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            Console.WriteLine("beforeDeptTimeHours" + beforeDeptTimeHours);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            Console.WriteLine("depatureTime" + DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss"));

            itemNode1.AppendChild(newElement1);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            Console.WriteLine(code[0].InnerText + " Code");

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Console.WriteLine(descText[0].InnerText + " descText");

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_ZeroTimeAfterDurationField()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            Console.WriteLine("afterDepTimeHours" + afterDepTimeHours);

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            Console.WriteLine("beforeDeptTimeHours" + beforeDeptTimeHours);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            Console.WriteLine("depatureTime" + DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss"));

            itemNode1.AppendChild(newElement1);

            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement2.InnerText = "PT0H";

            itemNode1.AppendChild(newElement2);

            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement3.InnerText = "PT2H";

            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            Console.WriteLine(code[0].InnerText + " Code");

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Console.WriteLine(descText[0].InnerText + " descText");

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_ZeroTimeBeforeDurationField()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            Console.WriteLine("afterDepTimeHours" + afterDepTimeHours);

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            Console.WriteLine("beforeDeptTimeHours" + beforeDeptTimeHours);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            Console.WriteLine("depatureTime" + DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss"));

            itemNode1.AppendChild(newElement1);

            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement2.InnerText = "PT2H";

            itemNode1.AppendChild(newElement2);

            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement3.InnerText = "PT0H";

            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            Console.WriteLine(code[0].InnerText + " Code");

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Console.WriteLine(descText[0].InnerText + " descText");

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_ZeroTimeBeforeAndAfterDurationField()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            Console.WriteLine("afterDepTimeHours" + afterDepTimeHours);

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            Console.WriteLine("beforeDeptTimeHours" + beforeDeptTimeHours);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            Console.WriteLine("depatureTime" + DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss"));

            itemNode1.AppendChild(newElement1);

            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement2.InnerText = "PT0H";

            itemNode1.AppendChild(newElement2);

            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement3.InnerText = "PT0H";

            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            Console.WriteLine(code[0].InnerText + " Code");

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Console.WriteLine(descText[0].InnerText + " descText");

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_TimeRequestedAfterDepatureDay()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(5).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            Console.WriteLine("afterDepTimeHours" + afterDepTimeHours);

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            Console.WriteLine("beforeDeptTimeHours" + beforeDeptTimeHours);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = "16:00:00";

            Console.WriteLine("depatureTime" + DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss"));

            itemNode1.AppendChild(newElement1);

            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement2.InnerText = "PT9H";

            itemNode1.AppendChild(newElement2);

            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement3.InnerText = "PT1H";

            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            Console.WriteLine(code[0].InnerText + " Code");

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Console.WriteLine(descText[0].InnerText + " descText");

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_DepatureTime_TimeRequestedBeforeDepatureDay()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string addfourhours = DateTime.UtcNow.AddHours(5).ToString("HH:mm:ss");

            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
            JourneyType.OneWay,
            locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
            cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            string afterDeptTime = DateTime.UtcNow.AddHours(6).ToString("HH:mm:ss");

            int afterDepTimeHours = int.Parse(afterDeptTime.Substring(0, 2));

            Console.WriteLine("afterDepTimeHours" + afterDepTimeHours);

            string beforeDeptTime = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");

            int beforeDeptTimeHours = int.Parse(beforeDeptTime.Substring(0, 2));

            Console.WriteLine("beforeDeptTimeHours" + beforeDeptTimeHours);

            XmlNode itemNode1 = updatedAirShoppingRequest.SelectSingleNode("//def:OriginDestCriteria[1]//def:OriginDepCriteria", airShoppingRequestXmlnsManager);

            XmlElement newElement1 = updatedAirShoppingRequest.CreateElement("Time", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement1.InnerText = "08:00:00";

            Console.WriteLine("depatureTime" + DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss"));

            itemNode1.AppendChild(newElement1);

            XmlElement newElement2 = updatedAirShoppingRequest.CreateElement("TimeAfterDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement2.InnerText = "PT9H";

            itemNode1.AppendChild(newElement2);

            XmlElement newElement3 = updatedAirShoppingRequest.CreateElement("TimeBeforeDuration", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            newElement3.InnerText = "PT9H";

            itemNode1.AppendChild(newElement3);

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml);

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

            //Getting deptarure timings of every flight from response
            XmlNodeList code = airShoppingResponseXmlDocument.GetElementsByTagName("Code");
            Assert.NotNull(code);

            XmlNodeList descText = airShoppingResponseXmlDocument.GetElementsByTagName("DescText");
            Assert.NotNull(descText);

            Assert.True((code[0].InnerText).Equals("103"),
           $"Text value for Order create error message is not as expected. Expected 107 " +
           $"but was {code[0].InnerText}");

            Assert.True((descText[0].InnerText).Equals("Invalid/ Missing Departure Time."),
            $"Error Code for Order create error message is not as expected. Expected Invalid/ Missing Departure Time. " +
            $"but was {descText[0].InnerText}");
        }
    }
}
