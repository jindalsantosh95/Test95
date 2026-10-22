using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OrderReshop
{
    public class OrderReshopCancelResponseValidations : IClassFixture<MemoryStoreFixture>
    {
        

        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderReshopCancelResponseValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdDanishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdDanishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSwedishAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSwedishAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdSaudiAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdSaudiAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCanadianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCanadianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCarrebianAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCarrebianAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCaribjetAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCaribjetAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdEuropeanAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdEuropeanAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdNorwegiansAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdNorwegiansAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderReshopCancel)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDVisa")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin, JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDMaster")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "UnsecureUntokenized")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 1, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDMaster")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDMaster")]
        [InlineData(6, 100, 200, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 2, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.MBJ, LocationType.MBJ, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDMaster")]
        [InlineData(7, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, SeatPriceType.None, null, "InstantPurchase", "CPDVisa")]
        [InlineData(8, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.Paid, null, "InstantPurchase", "CPDVisa")]
        [InlineData(9, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.Free, null, "InstantPurchase", "CPDVisa")]
        [InlineData(10, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDVisa")]
        [InlineData(11, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 1, 1, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.YYZ, LocationType.YYZ, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "InstantPurchase", "CPDMaster")]
        [InlineData(12, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, SeatPriceType.None, null, "HoldBooking", "CPDMaster")]
        [InlineData(13, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 0, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "HoldBooking", "CPDVisa")]
        [InlineData(14, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, SeatPriceType.None, null, "HoldBooking", "CPDMaster")]
        [InlineData(15, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, SeatPriceType.None, null, "HoldBooking", "CPDVisa")]
        public async Task OrderReshopCancelRSTests(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, SeatPriceType seatType, string corporate, string bookingType, string paymentType)
        {
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: seatType, corporate: corporate, bookingType: "InstantPurchase", paymentType: paymentType, apiResult: ApiResult.OrderReshopCancelRS,flow:"cancel");

            string orderCreateResult = result1["orderCreateResult"];

            string bookingId = result1["pnr"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string orderID = updatedOrderCreateResponse.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult,orderID, flow: "cancel");

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult =await orderReshopResponse.Content.ReadAsStringAsync();
            XmlDocument orderReshopResponseXmlDocument = new();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

        }

    }
}
