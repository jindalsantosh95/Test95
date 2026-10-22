using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingOffersValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";

        public AirShoppingOffersValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersOneWayEconomy()
        {
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");


            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersOneWayPremium()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "4", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");


            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersOneWayUpper()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK,
                cabinTypeCodeOne: "2", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {
                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersReturntripEconomy()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");


            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersReturntripPremium()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersReturntripUpper()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersMultiCityEconomy()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 3, numberOfchildren: 1, numberOfyouths: 2, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting deptarure timings of every flight from response

            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("ATL"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersMultiCityPremium()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.BOS,
                cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("LHR"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }

        [Trait("Category", "NDC-AirshoppingAllRegression")]
        [Fact]
        public async Task AirShopping_ReturnOffersMultiCityUpper()
        {
            string deptDateTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string addfourhours = DateTime.UtcNow.AddHours(4).ToString("HH:mm:ss");
            string todaysdate = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: participatingOrgOrgIdTravelRepublic,
                JourneyType.OneWay,
                locationOne: LocationType.ATL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.JFK,
                cabinTypeCodeOne: "2", cabinTypeCodeTwo: "2", numberOfadults: 2, numberOfchildren: 0, numberOfyouths: 0, deptDate: todaysdate);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest, memoryStoreFixture.Get<string>(participatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            airShoppingResponseXmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            airShoppingResponseXmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Getting deptarure timings of every flight from response
            int dmsCount = airShoppingResponseXmlDocument.SelectNodes("//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment", airShoppingResponseXmlnsManager).Count;

            for (int i = 1; i <= dmsCount; i++)
            {
                string aircraftTimeAndDate = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:AircraftScheduledDateTime", airShoppingResponseXmlnsManager).InnerText;

                string flightTime = (aircraftTimeAndDate).Substring(11, 8);

                Assert.NotNull(flightTime);

                string flightDate = (aircraftTimeAndDate).Substring(0, 10);

                string originCode = airShoppingResponseXmlDocument.SelectSingleNode($"//rs:DatedMarketingSegmentList//rs:DatedMarketingSegment[{i}]//rs:Dep//rs:IATA_LocationCode", airShoppingResponseXmlnsManager).InnerText;

                //Comparing the date of the flight with current date
                if (flightDate.Equals(todaysdate) && originCode.Equals("ATL"))
                {

                    int responseHours = int.Parse(flightTime.Substring(0, 2));

                    Assert.NotNull(responseHours);

                    int requestHours = int.Parse(addfourhours.Substring(0, 2));

                    Assert.NotNull(requestHours);

                    int responseMinutes = int.Parse(flightTime.Substring(3, 2));

                    Assert.NotNull(responseMinutes);

                    int requestminutes = int.Parse(addfourhours.Substring(3, 2));

                    Assert.NotNull(requestminutes);

                    if (responseHours.Equals(requestHours))
                        Assert.True(responseMinutes > requestminutes);

                    Assert.True(responseHours >= requestHours);
                }
            }
        }
    }
}
