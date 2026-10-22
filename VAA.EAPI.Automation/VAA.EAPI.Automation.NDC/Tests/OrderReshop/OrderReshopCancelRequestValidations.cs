using System.Xml;
using Newtonsoft.Json;
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
using VAA.EAPI.Automation.NDC.Tests.AirShopping;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OrderReshop
{
    public class OrderReshopCancelRequestValidations : IClassFixture<MemoryStoreFixture>
    {


        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderReshopCancelRequestValidations(MemoryStoreFixture memoryStoreFixture)
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

        [Fact]
        [Trait("Category", Traits.NDCOrderReshopCancel)]
        public async Task OrderReshopCancel_IncorrectOrderRefID()
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(240, 60, AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyLight, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.None, locationFour: LocationType.None, serviceCount: 0, seatType: SeatPriceType.None, corporate: null, bookingType: "InstantPurchase", paymentType: "Untokenized");

            string orderCreateResult = result1["orderCreateResult"];

            string bookingId = result1["pnr"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string orderID = updatedOrderCreateResponse.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderID, flow: "cancel");

            XmlDocument updatedReshopRequest = new XmlDocument();

            updatedReshopRequest.LoadXml(orderReshopRequest);

            updatedReshopRequest.SelectSingleNode("//ct:OrderRefID", xmlNamespaceManager).InnerText = "VS932ECD719";

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(updatedReshopRequest.OuterXml);

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = await orderReshopResponse.Content.ReadAsStringAsync();
            XmlDocument orderReshopResponsexml = new();
            orderReshopResponsexml.LoadXml(orderReshopResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(orderReshopResponsexml, "914", "OrderID mismatch in the request", out ValidationResultHelper result2), "Validation failed. Reasons: " + string.Join(", ", result2.Message));

        }

        [Fact]
        [Trait("Category", Traits.NDCOrderReshopCancel)]
        public async Task OrderReshopCancel_NullOrderRefID()
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(240, 60, AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyLight, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.None, locationFour: LocationType.None, serviceCount: 0, seatType: SeatPriceType.None, corporate: null, bookingType: "InstantPurchase", paymentType: "Untokenized");

            string orderCreateResult = result1["orderCreateResult"];

            string bookingId = result1["pnr"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string orderID = updatedOrderCreateResponse.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, flow: "cancel");

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = await orderReshopResponse.Content.ReadAsStringAsync();
            XmlDocument orderReshopResponseXmlDocument = new();
            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(orderReshopResponseXmlDocument, "914", "OrderID is mandatory in the request", out ValidationResultHelper result2), "Validation failed. Reasons: " + string.Join(", ", result2.Message));

        }

        [Fact]
        [Trait("Category", Traits.NDCOrderReshopCancel)]
        public async Task OrderReshopCancel_IncorrectAgent()
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(240, 60, AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyLight, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.None, locationFour: LocationType.None, serviceCount: 0, seatType: SeatPriceType.None, corporate: null, bookingType: "InstantPurchase", paymentType: "Untokenized");

            string orderCreateResult = result1["orderCreateResult"];

            string bookingId = result1["pnr"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string orderID = updatedOrderCreateResponse.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderID, flow: "cancel");

            XmlDocument updatedReshopRequest = new XmlDocument();

            updatedReshopRequest.LoadXml(orderReshopRequest);

            updatedReshopRequest.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "912835";

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(updatedReshopRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = await orderReshopResponse.Content.ReadAsStringAsync();
            XmlDocument orderReshopResponsexml = new();
            orderReshopResponsexml.LoadXml(orderReshopResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(orderReshopResponsexml, "368", "The requested seller is not authorised to access the service", out ValidationResultHelper result2), "Validation failed. Reasons: " + string.Join(", ", result2.Message));
        }

        [Fact]
        [Trait("Category", Traits.NDCOrderReshopCancel)]
        public void OrderReshopCancel_InvalidSubKEY()
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(240, 60, AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyLight, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.None, locationFour: LocationType.None, serviceCount: 0, seatType: SeatPriceType.None, corporate: null, bookingType: "InstantPurchase", paymentType: "Untokenized");

            string orderCreateResult = result1["orderCreateResult"];

            string bookingId = result1["pnr"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string orderID = updatedOrderCreateResponse.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderID, flow: "cancel");

            XmlDocument updatedReshopRequest = new XmlDocument();

            updatedReshopRequest.LoadXml(orderReshopRequest);

            updatedReshopRequest.SelectSingleNode("//ct:OrgID", xmlNamespaceManager).InnerText = "1234567";

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(updatedReshopRequest.OuterXml);

            //Getting the OrderReshopresponse and converting to XML Document
            dynamic orderReshopResult = JsonConvert.DeserializeObject<dynamic>(orderReshopResponse.Content.ReadAsStringAsync().Result);

            string errorcode = orderReshopResult.statusCode?.ToString();
            Assert.True(errorcode.Equals("401"), $"Error code expected is 401 but actual is {errorcode}");

            string errordesc = orderReshopResult.message?.ToString();
            string expectedDesc = "Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Fact]
        [Trait("Category", Traits.NDCOrderReshopCancel)]
        public async Task OrderReshopCancel_InvalidOrderRefID1()
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(240, 60, AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyLight, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, LocationType.LHR, locationTwo: LocationType.JNB, locationThree: LocationType.None, locationFour: LocationType.None, serviceCount: 0, seatType: SeatPriceType.None, corporate: null, bookingType: "InstantPurchase", paymentType: "Untokenized");

            string orderCreateResult = result1["orderCreateResult"];

            string bookingId = result1["pnr"];

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderCreateResponse.CreateXmlNamespaceManager();

            string orderID = updatedOrderCreateResponse.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderID, flow: "cancel");

            XmlDocument updatedReshopRequest = new XmlDocument();

            updatedReshopRequest.LoadXml(orderReshopRequest);

            updatedReshopRequest.SelectSingleNode("//ct:CancelOrderRef//ct:OrderRefID", xmlNamespaceManager).InnerText = "1234@#567";

            //Post Order Reshop request
            HttpResponseMessage orderReshopResponse = NDCTransformationEndpoints.OrderReshop(updatedReshopRequest.OuterXml);

            //Getting the Order Reshop response and converting to XML Document
            string orderReshopResult = await orderReshopResponse.Content.ReadAsStringAsync();
            XmlDocument orderReshopResponsexml = new();
            orderReshopResponsexml.LoadXml(orderReshopResult);

            Assert.True(AirshoppingValidator.ValidateErrorXmlResponse(orderReshopResponsexml, "914", "OrderID mismatch in the request", out ValidationResultHelper result2), "Validation failed. Reasons: " + string.Join(", ", result2.Message));

        }

    }
}
