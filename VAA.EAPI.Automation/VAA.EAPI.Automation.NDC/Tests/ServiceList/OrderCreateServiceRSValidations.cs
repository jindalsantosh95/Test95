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
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.ServiceList
{
    public class OrderCreateServiceRSValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;



        public OrderCreateServiceRSValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture; memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderCreateRegressionV2)]
        [InlineData(100, 200, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1)]
        [InlineData(110, 150, 1, 2, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1)]
        [InlineData(100, 170, 1, 1, 1, 1, "5", Core.Helpers.Enums.CabinType.EconomyDelightFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1)]
        [InlineData(200, 80, 1, 0, 2, 0, "4", Core.Helpers.Enums.CabinType.PremiumFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1)]
        [InlineData(190, 260, 2, 0, 0, 2, "2", Core.Helpers.Enums.CabinType.UpperClassFlex, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1)]
        [InlineData(86, 186, 2, 2, 1, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.TLV, LocationType.TLV, LocationType.LHR, LocationType.None, LocationType.None, 1)]
        public void ServiceListOfferFlowRSValidation_OneWayAndReturnSingleSSR(int addDays, int maximumDays, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, Core.Helpers.Enums.CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, int serviceCount)
        {
            Dictionary<string, string> result = OrderCreateHelper.CreatePnr(addDays, maximumDays, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, flight: flight, cabinCode: cabinCode, cabinName: cabinName, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, bookingType: "InstantPurchase", paymentType: "CPDVisa", serviceCount: serviceCount);

            string orderCreateResult = result["orderCreateResult"];

            string orderCreateRequest = result["orderCreateRQ"];

            string airShoppingRequest = result["airShoppingRQ"];

            string serviceListResult = result["serviceListResult"];

            Assert.True(OrderCreateAssertions.ValidateServices(orderCreateRequest, orderCreateResult, serviceListResult, out ValidationResultHelper res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));
        }

        [Fact]
        [Trait("Category", Traits.NDCSanityV2)]
        public void OrderCreateWithSSR()
        {
            RetryHelper.RunWithRetry(() =>
            {
                Dictionary<string, string> result = OrderCreateHelper.CreatePnr(120, 158, numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 0, numberOfinfants: 0, flight: FlightScenarioType.Virgin, cabinCode: "5", cabinName: CabinType.EconomyClassicFlex, journeyType: JourneyType.Return, locationOne: LocationType.LHR, locationTwo: LocationType.BOS, locationThree: LocationType.BOS, locationFour: LocationType.LHR, locationFive: LocationType.None, locationSix: LocationType.None, bookingType: "InstantPurchase", paymentType: "CPDVisa", serviceCount: 1);

                string orderCreateResult = result["orderCreateResult"];

                string orderCreateRequest = result["orderCreateRQ"];

                string airShoppingRequest = result["airShoppingRQ"];

                string serviceListResult = result["serviceListResult"];

                XmlDocument orderCreateRSDoc = new XmlDocument();

                SoftAssertHelper softAssert = new SoftAssertHelper();

                orderCreateRSDoc.LoadXml(orderCreateResult);

                XmlNamespaceManager xmlNamespaceManager = orderCreateRSDoc.CreateXmlNamespaceManager();
                ValidationResultHelper validationResults = new ValidationResultHelper();

                int warning = orderCreateRSDoc.SelectNodes("//ct:Warning[ct:DescText='SSR Selection Failed. The Selected Service has been waitlisted' and ct:Code='182' and ct:LangCode='EN' and ct:TagText='SSR Pending' and ct:TypeCode='Warning']", xmlNamespaceManager).Count;

                int serviceOrderItemCount = orderCreateRSDoc.SelectNodes("//ct:OrderItem[not(.//ct:FareDetail)]", xmlNamespaceManager).Count;

                softAssert.True(warning == 1 || serviceOrderItemCount == 1, $"SSRs orderitem and warning not returned in the response");

                //retrieve the pnr
                string orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateServices(orderCreateRequest, orderRetrieveResult, serviceListResult, out ValidationResultHelper res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                if (warning == 0)
                {
                    softAssert.True(OrderCreateAssertions.ValidateServices(orderCreateRequest, orderCreateResult, serviceListResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));
                }

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

                string orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                HttpResponseMessage orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureIsSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderCreateRegressionV2)]
        [InlineData(100, 200, 1, 0, 0, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2)]
        [InlineData(230, 70, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyClassicFlex, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, LocationType.None, LocationType.None, 2)]
        public void ServiceListOfferFlowRSValidation_MultipleSSR(int addDays, int maximumDays, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, Core.Helpers.Enums.CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, int serviceCount)
        {
            string deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");

            int maxNumberOfDays = maximumDays;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                journeyType,
                locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
                cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, cabinName, flight);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string offerID = offerPriceResponseXmlDocument.SelectSingleNode("//def:PricedOffer//def:OfferID", offerPriceResponseXmlnsManager).InnerText;

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            Assert.True(ServiceListAssertions.ValidateOffers(serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]//def:OfferItemRefID", offerPriceResponseXmlnsManager).InnerText = updatedSerivceListResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem[def:Service//def:ServiceDefinitionRefID='Disable01']//def:OfferItemID", offerPriceResponseXmlnsManager).InnerText;

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.True(OrderCreateAssertions.ValidateServices(orderCreateRequest, orderCreateResult, serviceListResult, out ValidationResultHelper res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));
        }

        [Theory]
        [Trait("Category", Traits.NDCOrderCreateRegressionV2)]
        [InlineData(100, 200, 1, 1, 1, 0, "5", Core.Helpers.Enums.CabinType.EconomyLight, FlightScenarioType.Virgin, JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2)]
        [InlineData(230, 70, 1, 1, 1, 0, "4", Core.Helpers.Enums.CabinType.Premium, FlightScenarioType.Virgin, JourneyType.Return, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR, LocationType.None, LocationType.None, 2)]
        public void ServiceListOfferFlowRSValidation_PartialPaxSSR(int addDays, int maximumDays, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string cabinCode, Core.Helpers.Enums.CabinType cabinName, FlightScenarioType flight, JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, int serviceCount)
        {
            string deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");

            int maxNumberOfDays = maximumDays;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                journeyType,
                locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
                cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, cabinName, flight);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            string offerID = offerPriceResponseXmlDocument.SelectSingleNode("//def:PricedOffer//def:OfferID", offerPriceResponseXmlnsManager).InnerText;

            string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);

            XmlDocument updatedSerivceListRequest = new XmlDocument();

            updatedSerivceListRequest.LoadXml(serviceListRequest);

            HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

            string serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument updatedSerivceListResponse = new XmlDocument();

            updatedSerivceListResponse.LoadXml(serviceListResult);

            Assert.True(ServiceListAssertions.ValidateOffers(serviceListResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, paymentMethod: "CPDVisa");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]//def:OfferItemRefID", offerPriceResponseXmlnsManager).InnerText = updatedSerivceListResponse.SelectSingleNode("//def:ALaCarteOffer//def:OfferItem[def:Service//def:ServiceDefinitionRefID='Disable01']//def:OfferItemID", offerPriceResponseXmlnsManager).InnerText;

            XmlNode node1 = updatedOrderCreateRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[1]//def:PaxRefID[2]", offerPriceResponseXmlnsManager);

            node1.ParentNode.RemoveChild(node1);

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[1]//def:PaxRefID[2]", offerPriceResponseXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            XmlNode node3 = updatedOrderCreateRequest.SelectSingleNode("//def:AcceptSelectedQuotedOfferList//def:SelectedPricedOffer[2]//def:SelectedOfferItem[2]//def:PaxRefID[1]", offerPriceResponseXmlnsManager);

            node3.ParentNode.RemoveChild(node3);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));

            Assert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out res), "Validation failed. Reasons: " + string.Join(", ", res.Message));
        }
    }
}
