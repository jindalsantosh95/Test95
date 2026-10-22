using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.OCI.Tests.PnrsCreation
{
    public class RevenuePNRCreationTest : IClassFixture<TokenFixture>
    {
        protected readonly TestConfig _testConfig;

        protected virtual Task WritePnrToStorage(TestConfig config)
            => WriteToAzureStorage.AddOCIPNRToAzureStorageTable(config);

        public RevenuePNRCreationTest(TokenFixture tokenFixture)
        {
            _testConfig = new TestConfig();
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetRevenuePNRCreationData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRCreationForOneway(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, string leadPassengerFirstName, string leadPassengerLastName, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Internal.TestCaseName = "OneWayScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId, passengerFirstName: leadPassengerFirstName, passengerLastName: leadPassengerLastName);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetRevenuePNRCreationDataWithSeat), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRForReturnWithSeat(LocationType origin, LocationType destination, FareFamilyType fareFamily, int adults, int teens, int child, int infants, string productName, CabinName cabinName, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.FareFamilyType = fareFamily;
            _testConfig.Search.Passengers.AddPassengers(adults, teens, child, infants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Internal.TestCaseName = "ReturnWithSeatScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            double totalSeatPrice = 0;

            //Basket GraphQL request
            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(_testConfig.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse);

            dynamic basketResponse = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //get Flight price before the seat selection
            double totalFlightPrice = GraphQLHelper.GetBasketSummaryTotalPrice(basketResponse);

            //Validation all segment Sector ID are not same
            GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);

            //Retrieve Seat Map GraphQL Request
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(_testConfig.Internal.BasketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            //Calling Retrieve SeatMap and Select Seat with respect to inbound and outbound sector
            GraphQLHelper.SelectSeatsForAllSectors(_testConfig.Internal.BasketId, fareFamily, retrieveBasketGQLResponse, cabinName, out totalSeatPrice, productName);

            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(_testConfig.Internal.BasketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatAfterSeatSelectionGQLResponse, out message), message);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

            HttpResponseMessage retrieveBasketAfterGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketAfterGQLResponse.Content.ReadAsStringAsync());

            Assert.True(retrieveBasketResult.data.retrieveBasket.basketId.Value == _testConfig.Internal.BasketId, "Basket ID dont match");

            //Validate Total Seat price and Total price in basket
            Assert.True(totalFlightPrice + totalSeatPrice == GraphQLHelper.GetBasketSummaryTotalPrice(retrieveBasketResult), $" Total price is not valid -----\n Total Seat+flight price : {totalFlightPrice + totalSeatPrice} \n Total price from Basket : {GraphQLHelper.GetBasketSummaryTotalPrice(basketResponse)}");

            int basketAssignedSeatsCount = GraphQLHelper.ReturnAssignedSeats(retrieveBasketAfterGQLResponse).Length;

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);

            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();

            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);

            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();

            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRWithInsuranceOneWay(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Internal.TestCaseName = "OneWayWithInsuranceScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Insurance GraphQL
            string insuranceQuoteRequest = GraphQLRequestBuilder.InsuranceQuote(_testConfig.Internal.BasketId);
            HttpResponseMessage insuranceQuoteResponse = GraphQLEndpoint.Book(insuranceQuoteRequest).EnsureGQLResponseSuccess();
            JToken? insuranceQuoteResult = JsonConvert.DeserializeObject<JToken>(await insuranceQuoteResponse.Content.ReadAsStringAsync());

            HttpResponseMessage insuranceDecisionResponse = GraphQLEndpoint.InsuranceDecision(_testConfig.Internal.BasketId, true).EnsureGQLResponseSuccess();
            JToken? insuranceDecisionResult = JsonConvert.DeserializeObject<JToken>(await insuranceDecisionResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            _testConfig.Book.Cost.InsurancePrice = (double)afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.insurancePrice").Value<decimal>();
            double insuranceDecisionPrice = (double)insuranceDecisionResult.SelectToken("data.insuranceDecision.summary.insurancePrice").Value<decimal>();
            double insuranceQuotePrice = (double)insuranceQuoteResult.SelectToken("data.InsuranceQuote[0].products[0].totalPrice").Value<decimal>();

            Assert.True(_testConfig.Book.Cost.InsurancePrice == insuranceQuotePrice, $"\nBasket Insurance price : {_testConfig.Book.Cost.InsurancePrice} is not equal to Insurance Quote :{insuranceQuotePrice}  ");
            Assert.True(_testConfig.Book.Cost.InsurancePrice == insuranceDecisionPrice, $"\nBasket Insurance price : {_testConfig.Book.Cost.InsurancePrice} is not equal to Insurance decision :{insuranceDecisionPrice}  ");

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationJVPartnersData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRWithJVPartnersForReturn(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.ReturnDate = DataGenerator.ReturnDate(_testConfig.Search.DepartureDate);
            _testConfig.Internal.TestCaseName = $"ReturnJVPartnersScenarioRevenue";

            // For Outbound Leg
            _testConfig.Search.FareFamilyType = outboundFareFamilyType;
            _testConfig.Search.FlightType = outboundFlightType;

            //For Inbound Leg
            _testConfig.Search.InboundFareFamilyType = inboundFareFamilyType;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationCodeshareData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRWithCodesharePartnersForReturn(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType, FlightType inboundFlightType, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.ReturnDate = DataGenerator.ReturnDate(_testConfig.Search.DepartureDate);
            _testConfig.Internal.TestCaseName = $"ReturnCodesharePartnersScenarioRevenue";

            // For Outbound Leg
            _testConfig.Search.FareFamilyType = outboundFareFamilyType;
            _testConfig.Search.FlightType = outboundFlightType;

            //For Inbound Leg
            _testConfig.Search.InboundFareFamilyType = inboundFareFamilyType;
            _testConfig.Search.InboundFlightType = inboundFlightType;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationInterlineData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRWithInterlinePartnersForReturn(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.ReturnDate = DataGenerator.ReturnDate(_testConfig.Search.DepartureDate);
            _testConfig.Internal.TestCaseName = $"ReturnInterlinePartnersScenarioRevenue";

            // For Outbound Leg
            _testConfig.Search.FareFamilyType = outboundFareFamilyType;
            _testConfig.Search.FlightType = outboundFlightType;

            //For Inbound Leg
            _testConfig.Search.InboundFareFamilyType = inboundFareFamilyType;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationMultiCity3SlicesData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task MultiCity3SlicesOCIPnr(LocationType[] locations, int departureOffset)
        {
            _testConfig.Internal.TestCaseName = "MultiCity3SlicesScenarioRevenue";

            _testConfig.Search.JourneyType = JourneyType.MultiCity;

            _testConfig.Search.Origin = locations[0];

            _testConfig.Search.Destination = locations[1];

            string departureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, departureDate: departureDate);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            _testConfig.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(config: _testConfig, offerId: sliceTwoOfferId, currentTripIndexId: 3);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            // Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? retrieveBasketGQLResult = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketGQLResult, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment();
            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationMultiCity4SlicesData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task MultiCity4SlicesOCIPnr(LocationType[] locations, int departureOffset)
        {
            _testConfig.Internal.TestCaseName = "MultiCity4SlicesScenarioRevenue";

            _testConfig.Search.JourneyType = JourneyType.MultiCity;

            _testConfig.Search.Origin = locations[0];

            _testConfig.Search.Destination = locations[1];

            string departureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);

            //generating offer search graph ql request.
            string offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(locations, numberOfAdults: 1, departureDate: departureDate);

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest).EnsureGQLResponseSuccess();

            //get basket id from search graphql response body
            _testConfig.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            //getting offer id from graph ql response
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchResponse);

            //generating offer search graph ql slice 1request.
            string offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "1");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceOneOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceOneResponse);

            //generating offer search graph ql slice 2 request.
            string offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "2");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceTwoOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSliceTwoResponse);

            //generating offer search graph ql slice 3 request.
            string offerSearchGQLSlice3Request = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceTwoOfferId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "3");

            //post search GRAPHQL endpoint.
            HttpResponseMessage offerSearchSlice3Response = GraphQLEndpoint.OfferSearch(offerSearchGQLSlice3Request).EnsureGQLResponseSuccess();

            //getting offer id from graph ql response
            string sliceThreeOfferId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(offerSearchSlice3Response);

            //generating offer price graph ql request.
            string offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(config: _testConfig, offerId: sliceThreeOfferId, currentTripIndexId: 4);

            //post price GRAPHQL endpoint.
            HttpResponseMessage offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            // Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? retrieveBasketGQLResult = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketGQLResult, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment();
            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetOCIReturnDateScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationForOneWayOCIReturnDate(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Book.Payment.PaymentType = "PayPal";
            _testConfig.Internal.TestCaseName = "OneWayReturnDateScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Order.ChildPRI = await HostedPaymentAutomation.SubmitPayPalAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetOCIDirectWithStopScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationForOneWayDirectWithStop(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.DirectWithStop = true;
            _testConfig.Internal.TestCaseName = "OneWayDirectWithStopScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.FlightsWithTwoStopScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationOneWayWithTwoStop(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.TwoStops = true;
            _testConfig.Internal.TestCaseName = "OneWayWithTwoStopScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.FlightsWithOneStopScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationOneWayWithOneStop(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.OneStop = true;
            _testConfig.Internal.TestCaseName = "OneWayWithOneStopScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.DirectFlightsScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationOneWayDirectFlight(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.DirectFlight = true;
            _testConfig.Internal.TestCaseName = "OneWayDirectFlightScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.LongLayoverFlightsScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationOneWayWithLongLayover(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int connectionTimeDuration, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.LongLayover = true;
            _testConfig.OfferFilterCriteria.LongLayoverDuration = connectionTimeDuration;
            _testConfig.Internal.TestCaseName = "OneWayLongLayoverFlightScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetTwo)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetPNRCreationData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenueSameDayReturn(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.ReturnDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.DirectFlight = true;
            _testConfig.Internal.TestCaseName = $"ReturnSameDayScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetThree)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.DirectFlightsScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationOneWaySkyPriorityWithBusinessClass(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.FareFamilyType = FareFamilyType.BUSFIRST;
            _testConfig.Search.CabinName = CabinName.VSUP;
            _testConfig.Internal.TestCaseName = "OneWaySkyPriorityScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetThree)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.TSAPNRCreationData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task TSAPNRCreationForOneway(string leadPassengerFirstName, string leadPassengerLastName, int departureOffset)
        {
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.Passengers.AddPassengers(1, 0, 0, 0);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Internal.TestCaseName = "OneWayTSAScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId, passengerFirstName: leadPassengerFirstName, passengerLastName: leadPassengerLastName);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetThree)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.SeamlessFlightsScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task SeamlessPNRCreationForOneway(LocationType origin, LocationType destination, FlightType flightType, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.FlightType = flightType;
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.Search.Passengers.AddPassengers(1, 0, 0, 0);
            _testConfig.Internal.TestCaseName = "OneWaySeamlessScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetThree)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.NonSeamlessFlightsScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task NonSeamlessPNRCreationForOneway(LocationType origin, LocationType destination, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(1, 0, 0, 0);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Book.Payment.PaymentType = "PayPal";
            _testConfig.Internal.TestCaseName = "OneWayNonSeamlessScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Order.ChildPRI = await HostedPaymentAutomation.SubmitPayPalAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "PayPal");
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.USABookingWithSpecialNamesESTAData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task PNRCreationESTAOneWay(LocationType origin, LocationType destination, string leadPassengerFirstName, string leadPassengerLastName, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(1, 0, 0, 0);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Internal.TestCaseName = "OneWayESTAScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId, passengerFirstName: leadPassengerFirstName, passengerLastName: leadPassengerLastName);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetOCIReturnDateScenarioData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task RevenuePNRForReturn(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, int departureOffset)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(numberOfadults, numberOfteens, numberOfchildren, numberOfinfants);
            _testConfig.Internal.TestCaseName = $"ReturnScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreationSetThree)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.SeasonalFlightsRoutes), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public async Task SeasonalRoutesPNRCreationForOneway(LocationType origin, LocationType destination, int departureOffset)
        {
            DateTime baseSeedDate = DateTime.UtcNow.Date.AddDays(1);

            DateTime baseDeparture = RouteDateUtility.GetTravelDates(origin.ToString(), destination.ToString(), baseSeedDate).DepartureDate;

            DateTime seedDate = baseDeparture.AddDays(departureOffset);

            RouteDateUtility.TravelDates travelDates = RouteDateUtility.GetTravelDates(origin.ToString(), destination.ToString(), seedDate);

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(1, 0, 0, 0);
            _testConfig.Search.DepartureDate = travelDates.DepartureDate.ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture);
            _testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
            _testConfig.Internal.TestCaseName = "OneWaySeasonalRouteScenarioRevenue";

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();

            JToken? beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, basketId: _testConfig.Internal.BasketId);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);

            GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            JToken? afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            //Delta HPP Page
            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);

            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();

            dynamic? paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }
    }
}
