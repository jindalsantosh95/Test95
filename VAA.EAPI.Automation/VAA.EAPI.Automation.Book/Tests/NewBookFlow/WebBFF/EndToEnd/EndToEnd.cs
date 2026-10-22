using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.EndToEnd
{
    [Collection("EndToEnd")]
    public class EndToEnd : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        public string deltaEmbeddedHPPUrl = "https://delta-hpp.webdev.vholsinternal.co.uk/hpp?paymentReferenceId=";
        private readonly TestConfig _testConfig;

        public EndToEnd(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
        }

        #region OneWay

        [Theory]
        [Trait("Category", GlobalTraits.LocalRun)]
        [InlineData("4000000000002503", "Visa", "3DS", LocationType.JFK, LocationType.LHR)]
        [InlineData("4444333322221111", "Visa", "3DS", LocationType.LHR, LocationType.DEL)]
        [InlineData("5555555555554444", "Visa", "3DS", LocationType.LHR, LocationType.JFK)]
        public async Task OneWay_OneAdult_VirginOnly_3DS(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 0, 0, 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateBasketCurrency(summaryBFFPayload, getBasket, out string currency), currency);

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH", true);

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateOrderCurrency(getBasket, confirmationResult, out string orderCurrencyValidation), orderCurrencyValidation);

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currencyConfirmation);
        }

        //US Start Origin
        //5163613613613613 >2500 and WorldPay
        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task OneWay_OneAdult_VirginOnly_non3DS()
        {
            string cardNumber = "5333304500657872";
            string cardType = "MasterCard";
            string threeDS = "non3DS";

            _testConfig.Search.Origin = LocationType.LAX;
            _testConfig.Search.Destination = LocationType.LHR;
            _testConfig.Search.FareFamilyType = FareFamilyType.BUSFIRST;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 0, 0, infants: 1);
            _testConfig.Book.Contact.PeopleId = "0";

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United States", "12345", state: "California");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);
        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task OneWay_OneAdult_VirginOnly_non3DS_Upper()
        {
            string cardNumber = "5333304500657872";
            string cardType = "MasterCard";
            string threeDS = "non3DS";

            _testConfig.Search.Origin = LocationType.LHR;
            _testConfig.Search.Destination = LocationType.LAS;
            _testConfig.Search.FareFamilyType = FareFamilyType.BUSFIRST;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 0, 0, infants: 1);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task OneWay_OneAdult_VirginOnly_USBilling()
        {
            string cardNumber = "4761344136141390";
            string cardType = "Visa";
            string threeDS = "non3DS";

            _testConfig.Search.Origin = LocationType.LHR;
            _testConfig.Search.Destination = LocationType.BGI;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, youngAdults: 1, children: 1, infants: 1);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United States", "12345", state: "Alaska");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task OneWay_2Adults_2Child_VirginOnly()
        {
            string cardNumber = "4761344136141390";
            string cardType = "Visa";
            string threeDS = "Non3DS";
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.ATL;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, 0, children: 2, 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "1234", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task OneWay_AllPtcs_VirginOnly()
        {
            string cardNumber = "4761344136141390";
            string cardType = "Visa";
            string threeDS = "Non3DS";
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.MCO;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 1, children: 1, infants: 1);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "0128", "1234", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.LHR, LocationType.KOA, 2, 1, 0, 0)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.LHR, LocationType.CPT, 1, 0, 0, 0)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.LHR, LocationType.MCO, 1, 0, 0, 0)]
        public async Task OneWay_2Adult_1Teen_NonVirgin_MultiStop(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.Passengers.AddPassengers(adults: numberOfadults, youngAdults: numberOfteens, children: numberOfchildren, infants: numberOfinfants);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);
        }

        #endregion OneWay

        #region Return

        [Fact]
        [Trait("Category", Traits.BookBffSmoke)]
        public async Task Return_OneAdult_VirginOnly()
        {
            string cardNumber = "4761344136141390";
            string cardType = "Visa";
            string threeDS = "Non3DS";
            LocationType origin = LocationType.MAN;
            LocationType destination = LocationType.MCO;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, youngAdults: 0, children: 0, infants: 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task Return_1Adult_1Child_VirginOnly()
        {
            string cardNumber = "4761344136141390";
            string cardType = "Visa";
            string threeDS = "Non3DS";
            LocationType origin = LocationType.ATL;
            LocationType destination = LocationType.LHR;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, youngAdults: 0, children: 1, infants: 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "0128", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("AA99 9AA")]
        [InlineData("AA9 9AA")]
        [InlineData("A9 9AA")]
        public async Task Return_2Adult_VirginOnly_PostCode(string postCode)
        {
            string cardNumber = "5101081046006034";
            string threeDS = "Non3DS";
            string cardType = "Mastercard";
            LocationType origin = LocationType.LHR;
            LocationType destination = LocationType.JFK;

            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 0, children: 1, infants: 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", postCode);

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.LHR, LocationType.ATL, 2, 1, 0, 0)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.YYZ, LocationType.LHR, 1, 0, 0, 0)]
        public async Task Return_2Adult_1Teen_VirginOnly(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: numberOfadults, youngAdults: numberOfteens, children: numberOfchildren, infants: numberOfinfants);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.LHR, LocationType.MCO, 1, 0, 0, 0)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.ATL, LocationType.LHR, 1, 0, 0, 0)]
        public async Task Return_OneAdult_NonVirgin(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: numberOfadults, youngAdults: numberOfteens, children: numberOfchildren, infants: numberOfinfants);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);

            //Get order cache
            HttpResponseMessage getConfirmationRespone = WebBFFEndpoint.GetConfirmation(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
        }

        [Fact]
        [Trait("Category", Traits.BookBffRegression)]
        public async Task Return_2Adult_VirginOnly_Declined()
        {
            string cardNumber = "4000319872807223";

            _testConfig.Search.Origin = LocationType.LHR;
            _testConfig.Search.Destination = LocationType.JFK;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: 2, youngAdults: 0, children: 0, infants: 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            var pri = hppUrl.Split('=')[1];

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetailsDeclined("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.LAX, 2, 0, 0, 0)]
        [InlineData(LocationType.LAX, LocationType.LHR, 2, 0, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.YYZ, 1, 0, 0, 0)]
        [InlineData(LocationType.YYZ, LocationType.LHR, 1, 0, 0, 0)]
        public async Task Return_2Adult_VirginOnly_Paypal(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = JourneyType.Return;
            _testConfig.Search.Passengers.AddPassengers(adults: numberOfadults, youngAdults: numberOfteens, children: numberOfchildren, infants: numberOfinfants);
            _testConfig.Internal.TestCaseName = "BFF_EndtoEnd_Return_PayPal";
            _testConfig.Book.Payment.PaymentType = "PayPal";

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);
            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);
            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(_testConfig.Internal.BasketId).EnsureIsSuccessStatusCode();
            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());
            WebBFFHelper.UpdateAllPassengerDetails(getBasket, _testConfig.Internal.BasketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(_testConfig.Internal.BasketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);
            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(_testConfig.Internal.BasketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();
            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;
            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();
            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();
            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            _testConfig.Book.Order.HppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;
            WebBFFHelper.HPPUrl(_testConfig);

            //Delta HPP Page
            using var payPalPayment = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
            payPalPayment.SubmitPayPal(_testConfig);
            _testConfig.Book.Order.ChildPRI = payPalPayment.GetChildPri();

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.ConfirmationPayPal(_testConfig.Internal.BasketId, _testConfig.Book.Order.ChildPRI);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();
            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationInfo(confirmationResponse, _testConfig);

            //Assert PNR data in order document
            Assert.True(GraphQLAssertion.ValidatePnrOrderDocument(_testConfig.Book.Order.PNR, out string info), $"Order PayPal Validation:  {info}");

            //Azure storage data
            await WriteToAzureStorage.AddPNRToAzureStorageTable(_testConfig);
        }

        #endregion Return

        #region Error handling

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4761344136141390", "Visa", "Non3DS", LocationType.LHR, LocationType.LOS, 1, 1, 0, 0, "United Kingdom", "1226", "123")]
        public async Task OneWay_VirginOnly_FraudCheck(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, string country, string expireDate, string cvv)
        {
            //Generating offer search BFF request for fraud check with randomDates as true
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants, randomDatesWithin7Days: true);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, FlightScenarioType.Virgin);

            //Getting basketId from the response url
            WebBFFHelper.SummaryURL(url, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, basketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(basketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(basketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            Assert.True(tncResponse.termsAndConditions.validation.ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase), "VALID was not returned as expected for TnC");

            Assert.True(tncResponse.validation.ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase), "VALID was not returned as expected for Basket");

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(basketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, expireDate, cvv, "12 address line One", "addressline Two", "Town", country, "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(basketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.ConfirmationFraudCheck(basketId);

            // Pre condition failed message as fraud check conditions are there
            Assert.True(confirmationResponse.StatusCode.Equals(System.Net.HttpStatusCode.PreconditionFailed), "FRaud check failed");

            //Accept credit card required check and hit confirmation again
            HttpResponseMessage confirmationResponse1 = WebBFFEndpoint.Confirmation(basketId, "", "true").EnsureIsSuccessStatusCode();
            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse1.Content.ReadAsStringAsync());

            //Extract PNR, Total price, email, currency for report
            WebBFFHelper.ConfirmationResponse(confirmationResult, out string pnr, out double totalPrice, out string email, out string currency);
        }

        #endregion Error handling
    }
}