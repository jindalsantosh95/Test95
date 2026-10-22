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
using VAA.EAPI.Automation.NDC.Tests.OrderChange;
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using VAA.EAPI.Automation.NDC.Tests.OrderQuote;
using VAA.EAPI.Automation.NDC.Tests.OrderReshop;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;
using LocationType = VAA.EAPI.Automation.Core.Helpers.Enums.LocationType;

namespace VAA.EAPI.Automation.NDC.Tests.EndToEnd
{
    public class SanityE2E : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public SanityE2E(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", Traits.NDCE2EFlowTests)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.BGI, LocationType.None, LocationType.None, LocationType.None, LocationType.None, "Direct", null, "CPDVisa", "EvenExchange", SeatPriceType.Paid)]
        [InlineData(2, 200, 100, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.BGI, LocationType.BGI, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa", "AddCol", SeatPriceType.Free)]
        public void ConfirmHeldBooking_OrderChangeEvenExchange_OrderChangeSeat(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType, string exchangeType, SeatPriceType seatType)
        {
            #region confirm held booking and retrieve pnr
            Dictionary<string, string> result1 = OrderChangeHelper.ChangeOrder(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, seatType: SeatPriceType.None, apiResult: ApiResult.OrderChangeConifrmHeldBookingRS, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, bookingType: "HoldBooking", paymentType: paymentType);

            string airShoppingRequest = result1["airShoppingRQ"];

            string airShoppingResult = result1["airShoppingRS"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string offerPriceResult = result1["offerPriceRS"];

            string orderRetrieveHoldBookingResult = result1["orderRetrieveRS"];

            string orderChangeConfirmHoldBookingResult = result1["OrderChangeConifrmHeldBookingRS"];

            SoftAssertHelper softAssert = new SoftAssertHelper();

            //validate order create hold booking response
            softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "HoldBooking"), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "create hold booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            //validate order change confirm hold booking response
            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result, flow: "InstantPurchase"), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeConfirmHoldBookingResult, out result), "Confirm Held booking Validation failed. Reasons: " + string.Join(", ", result.Message));

            #endregion

            #region add seat to pnr and retrieve pnr

            Dictionary<string, string> result3 = UpdatePNRDetails.AddSeatToPNR(airShoppingRequest, orderCreateResult, seatType: SeatPriceType.Paid, apiResult: ApiResult.OrderChangeSeatRS, orderChangePayment: "CPDVisa");

            string orderChangeSeatRQ = result3["orderChangeSeatRQ"];

            string orderChangeSeatRS = result3["orderChangeSeatRS"];

            //validate order change seat response

            softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatRS, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderChangeSeatPrice(orderChangeSeatRQ, orderCreateResult, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //validate change seat retrieve response

            string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "InstantPurchase"), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderChangeSeatPrice(orderChangeSeatRQ, orderCreateResult, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOrderChangeSeatTicketDocument(orderChangeSeatRQ, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            #endregion

            #region add SSR to pnr and retrieve pnr

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderRetrieveResult) + FindAndReturnDataHelper.GetYearSuffix());

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml).EnsureIsSuccessStatusCode();

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

            string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRequest, serviceListResult, "GBP", FindAndReturnDataHelper.ReturnBookingId(orderChangeConfirmHoldBookingResult), "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderChangeConfirmHoldBookingResult) + FindAndReturnDataHelper.GetYearSuffix(), serviceCount: 1,selectedService:"AVML");

            updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

            HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureIsSuccessStatusCode();

            string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

            updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

            XmlNamespaceManager xmlNamespaceManager = updatedOrderChangeServiceResponse.CreateXmlNamespaceManager();

            int warning = updatedOrderChangeServiceResponse.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlNamespaceManager).Count;

            int serviceOrderItemCount = updatedOrderChangeServiceResponse.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlNamespaceManager).Count;

            softAssert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

            //retrieve the pnr
            orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();
            orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            #endregion

            #region adult name correction and retrieve pnr

            string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

            HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

            string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

            HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

            string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //validate Adult_1 name in retrieve response

            orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            orderRetrieveResponseXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

            string adult1UpdatedGivenName = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

            string adult1UpdatedSurName = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

            string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

            string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

            if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
            {
                result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
            }

            #endregion

            #region amend Adult APIs
            string orderChangeAmendAPISRequestAdult = RequestHelper.GenerateOrderChangeAmendAPIsRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderRetrieveResult, "ADULT_1", "ADT", passport: "passport", visa: "visa");

            HttpResponseMessage orderChangeAmendAPIsRSAdult = NDCTransformationEndpoints.OrderChange(orderChangeAmendAPISRequestAdult).EnsureIsSuccessStatusCode();

            dynamic orderChangeAmendAPIsResultAdult = orderChangeAmendAPIsRSAdult.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeAmendAPIsResultAdult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.OrderChangeRQValidateAPIs(orderChangeAmendAPISRequestAdult, orderChangeAmendAPIsResultAdult, out result, passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            orderRetrieveResponseXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);

            softAssert.True(OrderCreateAssertions.OrderChangeRQValidateAPIs(orderChangeAmendAPISRequestAdult, orderRetrieveResult, out result, passport: "passport", visa: "visa", knowntraveller: "knowntraveller", redress: "redress"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            #endregion

            #region change travel date and retrieve pnr
            Dictionary<string, string> result2 = UpdatePNRDetails.ChangeTravelDate(airShoppingRequest, orderCreateResult, cabinCode: cabinCode, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, exchangeType: exchangeType, changeOrder: ChangeOrder.CompleteJourney, apiResult: ApiResult.OrderChangeRS, orderChangePayment: "CPDVisa", passport: "passport", visa: "visa");

            string orderRetrieveConfirmHoldBookingResult = result2["orderRetrieveRS"];

            string orderChangeResult = result2["OrderChangeResult"];

            string orderChangeRequest = result2["orderChangeRQ"];

            string orderReshopResult = result2["orderReshopRS"];

            string orderReshopRequest = result2["orderReshopRQ"];

            string orderQuoteResult = result2["orderQuoteRS"];

            string orderQuoteRequest = result2["orderQuoteRQ"];

            //Validate order reshop response

            softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePricingDetails(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + test + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateOperatingLeg(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderReshopRequest, orderReshopResult, out result), "Order Reshop Validation failed. Reasons: " + string.Join(", ", result.Message));

            //Validate order quote response

            softAssert.True(OrderReshopValidator.ValidateOffersReturned(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateRequotePricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateUpsellOffersPricingDetails(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateCabinTypeFromOrderReshopRequest(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateDatedMarketingAndOperatingList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateOriginDestList(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePaxJourneylist(airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePaxSegmentList(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidatePriceClassList(orderReshopRequest, airShoppingRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateBaggageAllowancelist(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateBaggageAssociations(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateFareComponent(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateBaggageAssociationsRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateFareComponentRequotedOffer(orderReshopRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateExpirationTTL(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateBaggaeAllowancelist(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateServiceDefinitionList(orderCreateRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateFareDetailOtherOffers(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateFareDetailRequotedOffer(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderQuoteValidator.ValidateOperatingLeg(orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidatePriceClassListCabinType(orderQuoteRequest, orderQuoteResult, out result), "Order Quote Validation failed. Reasons: " + string.Join(", ", result.Message));

            //validate order change exchange response

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Order Change Validation failed. Reasons: " + string.Join(", ", result.Message));

            //validate retrieve order change response

            orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            #endregion

            #region cancel pnr and retrieve pnr
            Dictionary<string, string> result4 = UpdatePNRDetails.CancelPNR(airShoppingRequest, orderCreateResult, apiResult: ApiResult.OrderChangeRS);

            string orderChangeCancelRQ = result4["OrderChangeCancelRQ"];

            string orderChangeCancelRS = result4["OrderChangeCancelRS"];

            string orderReshopCancelRQ = result4["orderReshopCancelRQ"];

            string orderReshopCancelRS = result4["orderReshopCancelRS"];

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlDocument orderReshopCancelRSDoc = new XmlDocument();

            orderReshopCancelRSDoc.LoadXml(orderReshopCancelRS);

            orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            softAssert.True(OrderChangeValidators.ValidateRfcOrderViewRS(orderCreateResult, orderChangeCancelRS, out result, flow: "NonRfc"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderReshopValidator.ValidateReshopCancelRS(airShoppingRequest, orderReshopCancelRS, out result, flow: "NonRfc"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderCreateResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve  Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve   Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Order Retrieve     Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderRetrieveResult, out result, flow: "Cancel"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            #endregion

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.NDCE2EFlowTests)]
        [InlineData(1, 100, 200, AgentID.ParticipatingOrgIdTravelRepublic, 1, 1, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, LocationType.None, LocationType.None, "Direct", null, "CPDVisa", "AddCol", SeatPriceType.Paid)]
        public void PnrWithSeatAndSSr_OrderChangeExchange_OrderCancel(int test, int addDays, int maximumDays, string participatingOrgOrgID, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, string agentType, string participatingOrgOrgID2, string paymentType, string exchangeType, SeatPriceType seatType)
        {
            Dictionary<string, string> result1 = OrderCreateHelper.CreatePnr(addDays, maximumDays, participatingOrgOrgID: participatingOrgOrgID, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, cabinCode: cabinCode, cabinName: cabinName, flight: flight, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, bookingType: "InstantPurchase", seatType: seatType, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, paymentType: paymentType, serviceCount: 1);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            string airShoppingRequest = result1["airShoppingRQ"];

            string orderCreateResult = result1["orderCreateResult"];

            string orderCreateRequest = result1["orderCreateRQ"];

            string serviceListResult = result1["serviceListResult"];

            softAssert.True(OrderCreateAssertions.ValidateServices(orderCreateRequest, orderCreateResult, serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            #region retrieve pnr

            string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest);

            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            #endregion

            #region perform date change

            Dictionary<string, string> result2 = UpdatePNRDetails.ChangeTravelDate(airShoppingRequest, orderCreateResult, cabinCode: cabinCode, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, exchangeType: exchangeType, changeOrder: ChangeOrder.CompleteJourney, apiResult: ApiResult.OrderChangeRS, orderChangePayment: "CPDVisa");

            string orderChangeResult = result2["OrderChangeResult"];

            #endregion

            #region cancel pnr

            Dictionary<string, string> result4 = UpdatePNRDetails.CancelPNR(airShoppingRequest, orderCreateResult, apiResult: ApiResult.OrderChangeRS);

            string orderChangeCancelRQ = result4["OrderChangeCancelRQ"];

            string orderChangeCancelRS = result4["OrderChangeCancelRS"];

            #endregion

            softAssert.AssertAll();
        }

        [Fact]
        [Trait("Category", Traits.NDCE2EFlowTests)]
        public void OrderChangeDifferentCradPaymentAndCancel()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderChangeHelperResult = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "4", cabinName: CabinType.PremiumFlex, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.SEA, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "CPDMaster", agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderChangeResult = orderChangeHelperResult["OrderChangeResult"];

                string orderChangeRequest = orderChangeHelperResult["orderChangeRQ"];

                string orderCreateResult = orderChangeHelperResult["orderCreateResult"];

                string orderCreateRequest = orderChangeHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderChangeHelperResult["airShoppingRQ"];

                string offerPriceResult = orderChangeHelperResult["offerPriceRS"];

                string orderQuoteResult = orderChangeHelperResult["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //OrderCancel
                Dictionary<string, string> cancelPnrHelperResult = UpdatePNRDetails.CancelPNR(airShoppingRequest, orderCreateResult, apiResult: ApiResult.OrderChangeRS);

                string orderChangeCancelRQ = cancelPnrHelperResult["OrderChangeCancelRQ"];

                string orderChangeCancelRS = cancelPnrHelperResult["OrderChangeCancelRS"];

                XmlDocument orderCancelRSDocument = new XmlDocument();

                orderCancelRSDocument.LoadXml(orderChangeCancelRS);

                XmlNamespaceManager xmlnsManager = orderCancelRSDocument.CreateXmlNamespaceManager();

                int paymentFunctionCount = orderCancelRSDocument.SelectNodes("//msg:PaymentFunctions", xmlnsManager).Count;

                if(paymentFunctionCount!= 4)
                {
                    result.Message.Add($"Expected Payment functions count 4 but actual is {paymentFunctionCount}.");
                }
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCE2EFlowTests)]
        public void OrderChangeAddCol_AddSeat_SSR_Cancel()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderChangeHelperResult = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassic, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.SEA, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "CPDVisa", agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com",reshopFlightType:"Direct");

                string orderChangeResult = orderChangeHelperResult["OrderChangeResult"];

                string orderChangeRequest = orderChangeHelperResult["orderChangeRQ"];

                string orderCreateResult = orderChangeHelperResult["orderCreateResult"];

                string orderCreateRequest = orderChangeHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderChangeHelperResult["airShoppingRQ"];

                string offerPriceResult = orderChangeHelperResult["offerPriceRS"];

                string orderQuoteResult = orderChangeHelperResult["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //2 Add Seat

                Dictionary<string, string> addSeatResult = UpdatePNRDetails.AddSeatToPNR(airShoppingRequest, orderCreateResult, seatType: SeatPriceType.Paid, apiResult: ApiResult.OrderChangeSeatRS, orderChangePayment: "CPDVisa");

_ = addSeatResult["orderChangeSeatRQ"];

                string orderChangeSeatRS = addSeatResult["orderChangeSeatRS"];

                //3 Add SSR

                string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix());

                XmlDocument updatedSerivceListRequest = new XmlDocument();

                updatedSerivceListRequest.LoadXml(serviceListRequest);

                HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml).EnsureIsSuccessStatusCode();

                string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);

                XmlDocument updatedorderChangeServiceRequest = new XmlDocument();

                string orderChangeServiceRequest = RequestHelper.GenerateOrderChangeServiceRequest(airShoppingRequest, serviceListResult, "GBP", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), serviceCount: 1, selectedService: "AVML");

                updatedorderChangeServiceRequest.LoadXml(orderChangeServiceRequest);

                HttpResponseMessage orderChangeServiceResponse = NDCTransformationEndpoints.OrderChange(updatedorderChangeServiceRequest.OuterXml).EnsureIsSuccessStatusCode();

                string orderChangeServiceResult = orderChangeServiceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument updatedOrderChangeServiceResponse = new XmlDocument();

                updatedOrderChangeServiceResponse.LoadXml(orderChangeServiceResult);

                XmlNamespaceManager xmlNamespaceManager = updatedOrderChangeServiceResponse.CreateXmlNamespaceManager();

                int warning = updatedOrderChangeServiceResponse.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlNamespaceManager).Count;

                int serviceOrderItemCount = updatedOrderChangeServiceResponse.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlNamespaceManager).Count;

                softAssert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                //validate order change seat response

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderChangeSeatRS, out result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderChangeSeatRS, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCE2EFlowTests)]
        public void OrderChange_UpdatePaxName_DateChangeAddCol()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderCreateHelperResult = OrderCreateHelper.CreatePnr(120, 158, participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic, numberOfadults: 1, numberOfchildren: 0, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.ATL, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", passport: "passport", visa: "visa", agentMailId: "Bandaru.JeevanNagaSai@fly.virgin.com");

                string orderCreateResult = orderCreateHelperResult["orderCreateResult"];

                string orderCreateRequest = orderCreateHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderCreateHelperResult["airShoppingRQ"];

                string airShoppingResult = orderCreateHelperResult["airShoppingResult"];

                string offerPriceResult = orderCreateHelperResult["offerPriceResult"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                softAssert.True(OrderCreateAssertions.ValidateOrderStatus(orderCreateRequest, orderCreateResult, out ValidationResultHelper result, flow: "InstantPurchase"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaymentFunctions(orderCreateRequest, orderCreateResult, out result, paymentType: "Card"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePriceClassList(orderCreateRequest, orderCreateResult, offerPriceResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateAPIs(orderCreateRequest, orderCreateResult, out result, passport: "passport", visa: "visa"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderRetrieveRequest = GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // Name correction 
                string orderReshopUpdatePaxRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId: "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateResult) + FindAndReturnDataHelper.GetYearSuffix(), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "updatePaxName", paxRefID: "ADULT_1", orderCreateRS: orderCreateResult);

                HttpResponseMessage orderReshopUpdatePaxResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxRequest).EnsureIsSuccessStatusCode();

                string orderReshopUpdatePaxResult = orderReshopUpdatePaxResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderReshopValidator.ValidateNameChangeReshopRS(orderReshopUpdatePaxRequest, orderReshopUpdatePaxResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                string orderChangeNameCorrectionAdultRequest = RequestHelper.GenerateOrderChangeNameCorrectionRequest(airShoppingRequest, orderReshopUpdatePaxResult, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

                HttpResponseMessage orderChangeNameCorrectionAdultResponse = NDCTransformationEndpoints.OrderChange(orderChangeNameCorrectionAdultRequest).EnsureIsSuccessStatusCode();

                string orderChangeNameCorrectionAdultResult = orderChangeNameCorrectionAdultResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                softAssert.True(OrderChangeValidators.ValidateNameChangeOrderItemAndServiceDefinition(orderReshopUpdatePaxResult, orderChangeNameCorrectionAdultResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //validate Adult_1 name in retrieve response

                orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

                XmlDocument orderRetrieveResponseXmlDocument= new XmlDocument();
                orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                orderRetrieveResponseXmlDocument.LoadXml(orderChangeNameCorrectionAdultResult);
                XmlNamespaceManager xmlNamespaceManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();
                string adult1UpdatedGivenName = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurName = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                string adult1UpdatedGivenNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:GivenName", xmlNamespaceManager).InnerText;

                string adult1UpdatedSurNameRetrieveRS = orderRetrieveResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[1]/ct:Individual/ct:Surname", xmlNamespaceManager).InnerText;

                if (!adult1UpdatedGivenName.Is(adult1UpdatedGivenNameRetrieveRS) || !adult1UpdatedSurNameRetrieveRS.Is(adult1UpdatedSurName))
                {
                    result.Message.Add($"Adult1 updated givename and surname are not returned in retrieve response. Expected given name --->{adult1UpdatedGivenName} but actual is {adult1UpdatedGivenNameRetrieveRS}, Expected surname ---> {adult1UpdatedSurName} but actual is {adult1UpdatedSurNameRetrieveRS}");
                }

                //Date change
                Dictionary<string, string> dateChangeResult = UpdatePNRDetails.ChangeTravelDate(airShoppingRequest, orderCreateResult, cabinCode: "5", journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.DEL, exchangeType: "AddCol", changeOrder: ChangeOrder.CompleteJourney, apiResult: ApiResult.OrderChangeRS, orderChangePayment: "CPDVisa");

                string orderChangeResult = dateChangeResult["OrderChangeResult"];

                string orderQuoteResult = dateChangeResult["orderQuoteRS"];

                string orderChangeRequest = dateChangeResult["orderChangeRQ"];

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.AssertAll();

                NDCTransformationEndpoints.OrderCancel(OrderPaymentRequestBuilderHelper.PnrCancel(FindAndReturnDataHelper.ReturnBookingId(orderCreateResult)));

                Assert.True(true);
            }, 1);
        }

        [Fact]
        [Trait("Category", Traits.NDCE2EFlowTests)]
        public void OrderChange_DateChangeMultipleTimes()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> orderChangeHelperResult = OrderChangeHelper.ChangeOrder(100, 200, participatingOrgOrgID: AgentID.ParticipatingOrgIdUSAgent, numberOfadults: 2, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, cabinCode: "5", cabinName: CabinType.EconomyClassic, flight: FlightScenarioType.Virgin, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, locationThree: LocationType.SEA, locationFour: LocationType.LHR, bookingType: "InstantPurchase", paymentType: "CPDVisa", changeOrder: ChangeOrder.CompleteJourney, agentType: "Direct", apiResult: ApiResult.OrderChangeRS, exchangeType: "AddCol", orderChangePayment: "CPDVisa", agentEmailId: "Bandaru.JeevanNagaSai@fly.virgin.com", reshopFlightType: "Direct");

                string orderChangeResult = orderChangeHelperResult["OrderChangeResult"];

                string orderChangeRequest = orderChangeHelperResult["orderChangeRQ"];

                string orderCreateResult = orderChangeHelperResult["orderCreateResult"];

                string orderCreateRequest = orderChangeHelperResult["orderCreateRQ"];

                string airShoppingRequest = orderChangeHelperResult["airShoppingRQ"];

                string offerPriceResult = orderChangeHelperResult["offerPriceRS"];

                string orderQuoteResult = orderChangeHelperResult["orderQuoteRS"];

                SoftAssertHelper softAssert = new SoftAssertHelper();

                XmlDocument updatedOrderChangeResponse = new XmlDocument();

                updatedOrderChangeResponse.LoadXml(orderChangeResult);

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderChangeResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidatePriceDifferential(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOrderItemID(orderCreateRequest, orderChangeResult, orderQuoteResult, out result, exchangeType: "EvenExchange"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderChangeValidators.ValidateOriginalIssueInfo(orderChangeRequest, orderChangeResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                //Date change
                Dictionary<string, string> dateChangeResult = UpdatePNRDetails.ChangeTravelDate(airShoppingRequest, orderCreateResult, cabinCode: "5", journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.LOS, exchangeType: "AddCol", changeOrder: ChangeOrder.CompleteJourney, apiResult: ApiResult.OrderChangeRS, orderChangePayment: "CPDVisa");

                string changeResult = dateChangeResult["OrderChangeResult"];

                softAssert.AssertAll();

                Assert.True(true);
            }, 1);
        }

    }
}
