using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.AirShopping;

namespace VAA.EAPI.Automation.NDC.Tests.Airshopping
{
    public class AirShoppingDepTimeSearch : IClassFixture<MemoryStoreFixture>
    {
        private const string IataCommonTypesNamespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes";
        private const string ParticipatingOrgOrgIdTravelRepublic = "91281035";
        private readonly MemoryStoreFixture memoryStoreFixture;

        public AirShoppingDepTimeSearch(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(ParticipatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{ParticipatingOrgOrgIdTravelRepublic}-OcpSubID"));
        }

        [Fact]
        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchOneWayEconomy() => RunDepartureTimeSearch(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.LHR, LocationType.JFK, "5", "5", 3, 1, 2, 7, "10:00:00", 12, 8, "PT2H", "PT2H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchOneWayPremium() => RunDepartureTimeSearch(JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.LHR, LocationType.BOS, "4", "4", 3, 1, 2, 7, "11:00:00", 20, 2, "PT9H", "PT9H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchOneWayUpper() => RunDepartureTimeSearch(JourneyType.OneWay, LocationType.LHR, LocationType.LAS, LocationType.LHR, LocationType.LAS, "2", "2", 3, 1, 2, 7, "08:00:00", 14, 4, "PT6H", "PT4H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchReturnEconomy() => RunDepartureTimeSearch(JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, "5", "5", 3, 1, 2, 7, "14:00:00", 18, 10, "PT4H", "PT4H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchReturnPremium() => RunDepartureTimeSearch(JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, "4", "4", 1, 1, 2, 7, "10:00:00", 17, 3, "PT7H", "PT7H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchReturnUpper() => RunDepartureTimeSearch(JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, "2", "2", 1, 1, 2, 7, "15:00:00", 21, 11, "PT6H", "PT4H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchMulticityEconomy() => RunDepartureTimeSearch(JourneyType.Return, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.JFK, "5", "5", 2, 0, 0, 7, "12:00:00", 21, 3, "PT9H", "PT9H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchMulticityPremium() => RunDepartureTimeSearch(JourneyType.Return, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.JFK, "4", "4", 3, 1, 2, 7, "12:00:00", 18, 6, "PT6H", "PT6H");

        [Fact]
        [Trait("Category", "NDC-AirshoppingAllRegression")]
        public Task AirShopping_DeptTimeSearchMulticityUpper() => RunDepartureTimeSearch(JourneyType.Return, LocationType.ATL, LocationType.LHR, LocationType.LHR, LocationType.JFK, "2", "2", 1, 1, 2, 48, "12:00:00", 20, 4, "PT8H", "PT8H");

        private async Task RunDepartureTimeSearch(JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, string cabinTypeCodeOne, string cabinTypeCodeTwo, int numberOfAdults, int numberOfChildren, int numberOfYouths, int departureDaysFromNow, string departureTime, int expectedTimeAfterHours, int expectedTimeBeforeHours, string timeAfterDuration, string timeBeforeDuration)
        {
            string departureDate = DateTime.UtcNow.AddDays(departureDaysFromNow).ToString("yyyy-MM-dd");
            string airShoppingRequest = journeyType == JourneyType.OneWay
                ? RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: ParticipatingOrgOrgIdTravelRepublic, journeyType, locationOne: locationOne, locationTwo: locationTwo, cabinTypeCodeOne: cabinTypeCodeOne, numberOfadults: numberOfAdults, numberOfchildren: numberOfChildren, numberOfyouths: numberOfYouths, deptDate: departureDate)
                : RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: ParticipatingOrgOrgIdTravelRepublic, journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, cabinTypeCodeOne: cabinTypeCodeOne, cabinTypeCodeTwo: cabinTypeCodeTwo, numberOfadults: numberOfAdults, numberOfchildren: numberOfChildren, numberOfyouths: numberOfYouths, deptDate: departureDate);

            XmlDocument updatedAirShoppingRequest = new XmlDocument();
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(updatedAirShoppingRequest.NameTable);
            namespaceManager.AddNamespace("def", IataCommonTypesNamespace);
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);
            AddDepartureTimeCriteria(updatedAirShoppingRequest, namespaceManager, "//def:OriginDestCriteria[1]//def:OriginDepCriteria", departureTime, timeAfterDuration, timeBeforeDuration);

            if (journeyType != JourneyType.OneWay)
            {
                AddDepartureTimeCriteria(updatedAirShoppingRequest, namespaceManager, "//def:OriginDestCriteria[2]//def:OriginDepCriteria", departureTime, timeAfterDuration, timeBeforeDuration);
            }

            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml, memoryStoreFixture.Get<string>(ParticipatingOrgOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();
            string airShoppingResult = await airShoppingResponse.Content.ReadAsStringAsync();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            Assert.True(AirshoppingValidator.ValidateDepartureTimeSearch(updatedAirShoppingRequest, airShoppingResponseXmlDocument, expectedTimeAfterHours, expectedTimeBeforeHours, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
        }

        private static void AddDepartureTimeCriteria(XmlDocument request, XmlNamespaceManager namespaceManager, string xpath, string departureTime, string timeAfterDuration, string timeBeforeDuration)
        {
            XmlNode departureCriteria = request.SelectSingleNode(xpath, namespaceManager)
                ?? throw new InvalidOperationException($"Expected node not found for xpath '{xpath}'.");
            departureCriteria.AppendChild(CreateElement(request, "Time", departureTime));
            departureCriteria.AppendChild(CreateElement(request, "TimeAfterDuration", timeAfterDuration));
            departureCriteria.AppendChild(CreateElement(request, "TimeBeforeDuration", timeBeforeDuration));
        }

        private static XmlElement CreateElement(XmlDocument document, string name, string value)
        {
            XmlElement element = document.CreateElement(name, IataCommonTypesNamespace);
            element.InnerText = value;
            return element;
        }
    }
}
