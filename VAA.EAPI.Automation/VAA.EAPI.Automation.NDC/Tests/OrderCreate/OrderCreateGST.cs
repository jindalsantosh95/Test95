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
using VAA.EAPI.Automation.NDC.Tests.OrderRetrieve;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Tests.OrderCreate
{
    public class OrderCreateGST : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderCreateGST(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));

        }

        [Trait("Category", "NDC - OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_1AdultTC01()
        {
            RetryHelper.RunWithRetry(() =>
            {
                string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");
                int maxNumberOfDays = 60;
                string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

                //Generate Air Shopping request
                string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                    JourneyType.Return,
                    locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                    cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

                //Post Air Shopping request
                HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

                //Getting the Air Shopping response and converting to XML Document
                string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
                airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

                //Adding namespace manager
                XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
                airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                //Find offer based on Cabin Type and Flight Scenraio
                Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

                Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

                string offerRefId = offerDetails["OfferRefID"];
                string totalAmount = offerDetails["TotalAmount"];
                string shoppingRefId = offerDetails["ShoppingResponseRefID"];
                string currencyCode = offerDetails["CurrencyCode"];

                //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
                string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

                //Post Offer Price request
                HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

                //Getting the Offer Price response and converting to XML Document
                string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
                offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

                //Adding namespace manager
                XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
                offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

                //Generating the Order Create Request
                string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa", gst: "gst");

                //Post Order Create request
                XmlDocument updatedOrderCreateRequest = new XmlDocument();

                XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
                orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

                XmlDocument xmlDocument_1 = new XmlDocument();

                HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

                XmlDocument updatedOrderCreateResponse = new XmlDocument();

                dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
                orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                updatedOrderCreateResponse.LoadXml(orderCreateResult);

                SoftAssertHelper softAssert = new SoftAssertHelper();

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

                //validate xsd against request
                Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

                XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

                XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
                orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

                //Order Retrieve request
                HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

                //Getting the Order retrieve response and converting to XML Document
                string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
                orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

                softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderRetrieveResult, out result, gst: "gst"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

                softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
                dynamic orderHistoryRQ = GenerateOrderHistoryRQ(airShoppingRequest, orderCreateResult);
                dynamic orderHistoryResponse = NDCTransformationEndpoints.OrderHistory(orderHistoryRQ).EnsureSuccessStatusCode();
                
                softAssert.AssertAll();

                Assert.True(true);
            }, 3);
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_YoungPaxTC02()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Indian Test Agent", participatingOrgOrgID: "14369880",
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "lin";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_ChildTC03()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(240).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR, cabinTypeCodeOne: "4", cabinTypeCodeTwo: "4", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDMaster", SeatPriceType.Free, seatAvailabilityResult, gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            XmlNode node1 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Employer", orderCreateRequestXmlnsManager);

            node1.ParentNode.RemoveChild(node1);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "cin";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();

            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderRetrieveResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_AdultAndChildPaxTC04()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "kim";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Sam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 2), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_AdultAndyouthPaxTC05()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "ram";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Sam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            //Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 2), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();

            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_AllPaxTC06()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            string seatAvailabilityRequest = RequestHelper.GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

            HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(seatAvailabilityResponse.IsSuccessStatusCode, $"Success status code was not returned from seatAvailabilityResponse, returned: {seatAvailabilityResponse.StatusCode} with message: {seatAvailabilityResponse.ReadResponseAsString()}" + $"seatAvailabilityResponse sent was {seatAvailabilityRequest}");

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            //Running XSD validation against the response from SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDMaster", SeatPriceType.Paid, seatAvailabilityResult, gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "sana";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Jam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            //Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 3), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            Assert.True(warning.Count == 0, $"seats are not booked");

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidteOrderItemIDSeat(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderRetrieve_GST_MultiPaxTC07()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 2, numberOfchildren: 2, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[6]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "singh";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Jam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Kam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[5]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Lam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[5]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[6]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Pam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[6]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            //Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 5), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_HoldBookingAdultTC08()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_HoldBookingYoungPaxTC09()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.BOM, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "kim";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            //Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();

            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_HoldBookingChildPaxTC10()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.DEL, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Employer", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "vain";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 1), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            softAssert.AssertAll();
            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_HoldBookingAllPaxTC011()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
               JourneyType.Return,
               locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR, cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode node2 = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);

            node2.ParentNode.RemoveChild(node2);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "mini";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Jam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            XmlNode node = updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[4]//def:Employer", orderCreateRequestXmlnsManager);

            node.ParentNode.RemoveChild(node);

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result, gst: "gst", gstPaxCount: 3), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidOrganizationNameTC12()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:Name", orderCreateRequestXmlnsManager).InnerText = "QWERTYUIOPASDFGHJKLZXCVBNMQWERTYUIOOJHGFGHXCNMVCJGCNCNHCGNCCHNCGGC";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result, gst: "gst"), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Organaization Name in the request"), $"GST Details Validation - Organaization Name in the request but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidEmailAddressTextTC13()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "4", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid E-Mail details in the request"), $"Expected 914 but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidCityNameTC14()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:CityName", orderCreateRequestXmlnsManager).InnerText = "%%%%%%%%%";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid address details in the request"), $"GST Details Validation - Invalid address details in the request but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidPhoneNumberTC15()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:PhoneNumber", orderCreateRequestXmlnsManager).InnerText = "%%%%%%";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;
            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid Phone number in the request"), $"Expected GST Details Validation - Invalid Phone number in the request but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage1), orderItemIdServiceMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidPostalCodeTC16()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyDelight, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:PostalCode", orderCreateRequestXmlnsManager).InnerText = "%%%%%%%%%";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;
            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid address details in the request"), $"Expected GST Details Validation - Invalid address details in the request but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidSalesTaxRegistrationIDTC17()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");
            int maxNumberOfDays = 60;
            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM,
                cabinTypeCodeOne: "4", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.Premium, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:SalesTaxRegistrationID", orderCreateRequestXmlnsManager).InnerText = "%%%%%%%%%";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - GST Number in the request"), $"Expected GST Details Validation - GST Number in the requestt but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_InvalidStreetTextTC18()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.BOM, locationThree: LocationType.BOM, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax//def:Employer//def:SalesTaxRegistration//def:StreetText", orderCreateRequestXmlnsManager).InnerText = "aesdhjfgkjhcgjhfxgxbvxbgnchmvhcghtfhmcnxvfhdhfmvcbxvnzbdfxgnchmc bvnfdhmcbxnvghmcgxnc";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            // Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateServiceDefinitionList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            string code = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:Code", orderCreateResponseXmlnsManager).InnerText;

            string descText = updatedOrderCreateResponse.SelectSingleNode("//def:Warning//def:DescText", orderCreateResponseXmlnsManager).InnerText;

            Assert.True(code.Equals("914"), $"Expected 914 but actual was {code}");

            Assert.True(descText.Equals("GST Details Validation - Invalid address details in the request"), $"Expected GST Details Validation - Invalid address details in the request but actual was {descText}");

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_NonIndianOriginAndAgent19()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "91281035",
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlNodeList gstdetails = updatedOrderCreateResponse.SelectNodes("//def:DataLists//def:PaxList//def:Pax//def:Employer", orderCreateResponseXmlnsManager);

            Assert.False(gstdetails.Count != 0, $"GST details are not expected in response");

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            XmlNodeList gstdetails1 = orderRetrieveResponseXmlDocument.SelectNodes("//def:DataLists//def:PaxList//def:Pax//def:Employer", orderCreateResponseXmlnsManager);

            Assert.False(gstdetails1.Count != 0, $"GST details are not expected in response");

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage1), orderItemIdServiceMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_NonIndianOriginAndIndiaAgent20()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.ATL, locationThree: LocationType.ATL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);
            SoftAssertHelper softAssert = new SoftAssertHelper();
            XmlNodeList gstdetails = updatedOrderCreateResponse.SelectNodes("//def:DataLists//def:PaxList//def:Pax//def:Employer", orderCreateResponseXmlnsManager);

            Assert.False(gstdetails.Count != 0, $"GST details are not expected in response");

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            XmlNodeList gstdetails1 = orderRetrieveResponseXmlDocument.SelectNodes("//def:DataLists//def:PaxList//def:Pax//def:Employer", orderCreateResponseXmlnsManager);

            Assert.False(gstdetails1.Count != 0, $"GST details are not expected in response");

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage1), orderItemIdServiceMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }

        [Trait("Category", "NDCE2ESanity")]
        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_IndianOriginAndNonIndianAgent21()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(200).ToString("yyyy-MM-dd");
            int maxNumberOfDays = 60;
            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL, locationThree: LocationType.DEL, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassicFlex, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDVisa", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            XmlNodeList gstdetails = updatedOrderCreateResponse.SelectNodes("//def:DataLists//def:PaxList//def:Pax//def:Employer", orderCreateResponseXmlnsManager);

            Assert.False(gstdetails.Count != 0, $"GST details are not expected in response");

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            XmlNodeList gstdetails1 = orderRetrieveResponseXmlDocument.SelectNodes("//DataLists//def:Paxlist//def:Pax//def:Employer", orderCreateResponseXmlnsManager);

            Assert.False(gstdetails1.Count != 0, $"GST details are not expected in response");

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage1), orderItemIdServiceMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreate_GST_MultiCity22()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "IndiaTestAgent", participatingOrgOrgID: AgentID.ParticipatingOrgIdIndiaTestAgent,
                JourneyType.MultiCity,
                locationOne: LocationType.IAD, locationTwo: LocationType.LHR, locationThree: LocationType.LHR, locationFour: LocationType.DEL, locationFive: LocationType.DEL, locationSix: LocationType.ATL,
                cabinTypeCodeOne: "5", numberOfadults: 1, numberOfchildren: 1, numberOfyouths: 1, deptDate: journey1Date);

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
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
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

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "CPDMaster", gst: "gst");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//def:DistributionChainLink//def:ContactInfo//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "bandaru.jeevannagasai@fly.virgin.com";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Da";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[1]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Man";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[2]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:GivenName", orderCreateRequestXmlnsManager).InnerText = "Sam";

            updatedOrderCreateRequest.SelectSingleNode("//def:DataLists//def:PaxList//def:Pax[3]//def:Surname", orderCreateRequestXmlnsManager).InnerText = "Smith";

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(updatedOrderCreateResponse.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateContactList(orderCreateRequest, orderCreateResult, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            //softAssert.True(OrderCreateAssertions.ValidatePaxList(updatedOrderCreateRequest.OuterXml, orderCreateResult, out string orderCreatePaxListMessage, gst: "gst", gstPaxCount: 3), orderCreatePaxListMessage); ; ;

            softAssert.True(OrderCreateAssertions.ValidateOriginDestList(airShoppingRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxJourneyList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidatePaxSegmentList(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIDService(updatedOrderCreateRequest.OuterXml, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateTicketDocument(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OrderCreateAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderCreateResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();

            dynamic orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            //validate xsd against request
            Assert.True(XsdValidator.ValidateOrderRetrieveRequest(orderRetrieveRequest, out string[] orderRetrieveRequestExceptions), string.Join(System.Environment.NewLine, orderRetrieveRequestExceptions));

            XmlDocument updatedOrderRetrieveRequest = new XmlDocument();

            XmlNamespaceManager orderRetrieveRequestXmlnsManager = new XmlNamespaceManager(updatedOrderRetrieveRequest.NameTable);
            orderRetrieveRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedOrderRetrieveRequest.LoadXml(orderRetrieveRequest);

            //Order Retrieve request
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(updatedOrderRetrieveRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdIndiaTestAgent)).EnsureIsSuccessStatusCode();

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderRetrieve Response, returned: {orderRetrieveResponse.StatusCode} with message: {orderRetrieveResponse.ReadResponseAsString()}" + $"OrderRetrieve Request sent was {orderRetrieveRequest}");

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            Assert.True(OrderRetrieveAssertions.ValidateContactList(orderCreateRequest, orderRetrieveResult, out string contactListMessage1), contactListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOriginDestList(airShoppingRequest, orderRetrieveResult, out string originDestListMessage1), originDestListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxJourneyList(orderCreateRequest, orderRetrieveResult, out string paxJourneyListMessage1), paxJourneyListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateDatedMarketingAndOperatingList(orderCreateRequest, orderRetrieveResult, out string datedMarketingAndOperatingListMessage1), datedMarketingAndOperatingListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidatePaxSegmentList(orderCreateRequest, orderRetrieveResult, out string paxSegmentListMessage1), paxSegmentListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIDService(orderCreateRequest, orderRetrieveResult, out string orderItemIdServiceMessage1), orderItemIdServiceMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateBaggaeAllowancelist(orderCreateRequest, orderRetrieveResult, out string baggageAllowanceListMessage1), baggageAllowanceListMessage1);

            Assert.True(OrderRetrieveAssertions.ValidateOrderItemIdPrice(orderCreateRequest, orderRetrieveResult, out string orderItemIdPriceMessage1), orderItemIdPriceMessage1);

        }
    }
}
