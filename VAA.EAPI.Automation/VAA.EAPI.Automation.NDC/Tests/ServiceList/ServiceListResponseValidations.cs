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
using VAA.EAPI.Automation.NDC.Tests.AirShopping;
using VAA.EAPI.Automation.NDC.Tests.OrderChange;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.ServiceList
{
    public class ServiceList_ResponseValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public ServiceList_ResponseValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", "ServiceListPrimeSaleRegression")]
        [InlineData(1, 200, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(2, 120, 60, AgentID.ParticipatingOrgIdIndiaTestAgent, 2, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassic, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(4, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.IAD, LocationType.IAD, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(5, 100, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(6, 100, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(7, 100, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClass, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(8, 100, 150, AgentID.ParticipatingOrgIdIndiaTestAgent, 1, 1, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.EWR, LocationType.EWR, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(9, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, "Direct", null)]
        [InlineData(10, 100, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(11, 100, 150, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(12, 100, 200, AgentID.ParticipatingOrgIdKayakUsa, 1, 0, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.BOM, LocationType.ATL, LocationType.ATL, LocationType.BOM, LocationType.None, LocationType.None, "Direct", null)]
        public void ServicePrimeSale(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2)
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, serviceCount: 1, apiResult: RequestHelper.ApiResult.ServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2);

            string airShoppingRequest = result1["airShoppingRQ"];

            string offerPriceResult = result1["offerPriceResult"];

            string serviceListResult = result1["serviceListResult"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //serviceListResult assertion

            softAssert.True(ServiceListAssertions.ValidateOffers(serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(AirshoppingValidator.ValidateNullValues(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidateDatedMarketingAndOperatingList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidatePaxList(serviceListResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidatePaxSegmentList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidateServiceDefnitionList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(ServiceListAssertions.ValidateAlcarteOffers(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCSanityV2)]
        [InlineData(1, 250, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(2, 100, 160, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null)]
        [InlineData(3, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, "Direct", null)]
        public void ServiceListPrimeSale(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2)
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, serviceCount: 1, apiResult: RequestHelper.ApiResult.ServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, agentMailId: "Bhaskar.Singh@fly.virgin.com");

                string airShoppingRequest = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceResult"];

                string serviceListResult = result1["serviceListResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                //serviceListResult assertion

                softAssert.True(ServiceListAssertions.ValidateOffers(serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + test + string.Join(", ", result.Message));

                softAssert.True(AirshoppingValidator.ValidateNullValues(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(ServiceListAssertions.ValidateDatedMarketingAndOperatingList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(ServiceListAssertions.ValidatePaxList(serviceListResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(ServiceListAssertions.ValidatePaxSegmentList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(ServiceListAssertions.ValidateServiceDefnitionList(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(ServiceListAssertions.ValidateAlcarteOffers(serviceListResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }


        [Theory]
        [Trait("Category", "ServiceListPostSaleRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JNB, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDVisa", "AVML")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","FPML")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDVisa","AVML")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","AVML")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 1, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.DEL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","CHML")]
        public void ServiceListPostSaleOneWay(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType, string selectedService = "")
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, apiResult: ApiResult.PostSaleServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string serviceListResult = result1["postSaleServiceListRS"];

                string airShoppingRQ = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRQ = result1["orderCreateRQ"];

                string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRQ, serviceListResult, "GBP", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), serviceCount: 1, selectedService: selectedService);

                XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

                updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

                HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureSuccessStatusCode();

                string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

                updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlNamespaceManager xmlnamespacemanager = updatedOrderChangeServiceResponse.CreateXmlNamespaceManager();
                int serviceOrderItemCount = updatedOrderChangeServiceResponse.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlnamespacemanager).Count;

                int warning = updatedOrderChangeServiceResponse.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlnamespacemanager).Count;

                Assert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRQ, orderCreateResult);

                string orderRetrieveResult = "";

                HttpResponseMessage orderRetrieveResponse = null;

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                    orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderRetrieveResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                }, 5);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string orderHistoryRQ = RequestHelper.GenerateOrderHistoryRQ(airShoppingRQ, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "ServiceListPostSaleRegression")]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa","AVML")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","FPML")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 0, 0, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa","CHML")]
        [InlineData(4, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.BOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","AVML")]
        [InlineData(5, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","FPML")]
        public void ServiceListPostSaleReturnTrip(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType,string selectedService="")
        {
            RetryHelper.RunWithRetry(() =>
            {

                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, apiResult: ApiResult.PostSaleServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string serviceListResult = result1["postSaleServiceListRS"];

                string airShoppingRQ = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRQ = result1["orderCreateRQ"];

                string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRQ, serviceListResult, "GBP", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), "VS932"+ FindAndReturnDataHelper.ReturnBookingId(orderCreateResult)+ FindAndReturnDataHelper.GetYearSuffix(), serviceCount: 1,selectedService:selectedService);

                XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

                updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

                HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureSuccessStatusCode();

                string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

                updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlNamespaceManager xmlnamespacemanager = updatedOrderChangeServiceResponse.CreateXmlNamespaceManager();
                int serviceOrderItemCount = updatedOrderChangeServiceResponse.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlnamespacemanager).Count;

                int warning = updatedOrderChangeServiceResponse.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlnamespacemanager).Count;

                Assert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRQ, orderCreateResult);

                string orderRetrieveResult = "";

                HttpResponseMessage orderRetrieveResponse = null;

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                    orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderRetrieveResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                }, 5);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string orderHistoryRQ = RequestHelper.GenerateOrderHistoryRQ(airShoppingRQ, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category", "ServiceListPostSaleRegression")]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa","AVML")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 2, 1, 0, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.LOS, LocationType.LOS, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","AVML")]
        [InlineData(3, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 2, 0, 0, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDMaster","AVML")]
        public void ServiceListPostSaleCodeShare(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType, string selectedService = "")
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, apiResult: ApiResult.PostSaleServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string serviceListResult = result1["postSaleServiceListRS"];

                string airShoppingRQ = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRQ = result1["orderCreateRQ"];

                string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRQ, serviceListResult, "GBP", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), serviceCount: 1, selectedService: selectedService);

                XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

                updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

                HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureSuccessStatusCode();

                string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

                updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlNamespaceManager xmlnamespacemanager = updatedOrderChangeServiceResponse.CreateXmlNamespaceManager();
                int serviceOrderItemCount = updatedOrderChangeServiceResponse.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlnamespacemanager).Count;

                int warning = updatedOrderChangeServiceResponse.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlnamespacemanager).Count;

                Assert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRQ, orderCreateResult);

                string orderRetrieveResult = "";

                HttpResponseMessage orderRetrieveResponse = null;

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                    orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderRetrieveResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                }, 5);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string orderHistoryRQ = RequestHelper.GenerateOrderHistoryRQ(airShoppingRQ, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

        [Theory]
        [Trait("Category",Traits.NDCServiceListPostSaleRegression)]
        [InlineData(1, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.Return, LocationType.LHR, LocationType.YYZ, LocationType.YYZ, LocationType.YVR, LocationType.None, LocationType.None, null, "Direct", null, "CPDVisa","AVML")]
        [InlineData(2, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.VirginAndNonVirgin, JourneyType.MultiCity, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.BOM, LocationType.BOM, LocationType.LHR, null, "Direct", null, "CPDMaster","FPML")]
        [InlineData(3, 240, 60, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.MultiCity, LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.DEL, LocationType.BOM, LocationType.LHR, null, "Direct", null, "CPDVisa","AVML")]
        public void ServiceListPostSaleMultiCity(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string corporate, string agentType, string participatingOrgOrgID2, string paymentType, string selectedService = "")
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, apiResult: ApiResult.PostSaleServiceListRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "InstantPurchase", paymentType: paymentType);

                string serviceListResult = result1["postSaleServiceListRS"];

                string airShoppingRQ = result1["airShoppingRQ"];

                string offerPriceResult = result1["offerPriceRS"];

                string orderCreateResult = result1["orderCreateResult"];

                string orderCreateRQ = result1["orderCreateRQ"];

                string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRQ, serviceListResult, "GBP", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), serviceCount: 1, selectedService: selectedService);

                XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

                updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

                HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureSuccessStatusCode();

                string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

                updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlNamespaceManager xmlnamespacemanager = updatedOrderChangeServiceResponse.CreateXmlNamespaceManager();
                int serviceOrderItemCount = updatedOrderChangeServiceResponse.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlnamespacemanager).Count;

                int warning = updatedOrderChangeServiceResponse.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlnamespacemanager).Count;

                Assert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRQ, orderCreateResult);

                string orderRetrieveResult = "";

                HttpResponseMessage orderRetrieveResponse = null;

                RetryHelper.RunWithRetry(() =>
                {
                    orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();

                    orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.True(OrderChangeValidators.ValidateServices(updatedorderChangeServiceRequest.OuterXml, orderRetrieveResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                }, 5);

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                string orderHistoryRQ = RequestHelper.GenerateOrderHistoryRQ(airShoppingRQ, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();

                softAssert.AssertAll();

                Assert.True(true);
            }, 2);
        }

    }
}
