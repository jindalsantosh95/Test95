using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.EndToEnd
{
    [Collection("BFFE2EFlex")]
    public class BFFE2EFlex : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        private string deltaHPPUrl = "https://delta-hpp.webdev.vholsinternal.co.uk/hpp?paymentReferenceId=";
        private readonly TestConfig _testConfig;

        public BFFE2EFlex(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
        }

        #region OneWay

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4761344136141390", "Visa", "non3DS", LocationType.LHR, LocationType.DEL, FareFamilyType.ECONCLASSICFLEXIBLE, "1224", "123")]
        [InlineData("4761344136141390", "Visa", "non3DS", LocationType.LHR, LocationType.DEL, FareFamilyType.ECONDELIGHTFLEXIBLE, "1224", "123")]
        [InlineData("4761344136141390", "Visa", "non3DS", LocationType.LHR, LocationType.DEL, FareFamilyType.BUSFIRSTFLEXIBLE, "1224", "123")]
        [InlineData("4761344136141390", "Visa", "non3DS", LocationType.LHR, LocationType.DEL, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE, "1224", "123")]
        public async Task Return_OneAdult_VirginOnly_3DS(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, FareFamilyType fareFamily, string expireDate, string cvv)
        {
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.FareFamilyType = fareFamily;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 0, 0, 0);

            //Search and Summary offer
            string summaryUrl = GraphQLHelper.OfferSearchGraphQL(_testConfig, true);

            WebBFFHelper.GetOfferIdAndSliceFromSummaryURL(summaryUrl, out string slice, out string offerId);

            summaryUrl = WebBFFHelper.BuildSummaryUrl(_testConfig.Internal.BasketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();

            Assert.True(WebBFFAssertions.ValidateFareFamily(summaryBffResponse.ReadResponseAsString(), fareFamily, out string fareFamilyType), fareFamilyType);

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

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(_testConfig.Internal.BasketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            deltaHPPUrl = deltaHPPUrl + pri;

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, expireDate, cvv, "12 address line One", "addressline Two", "Town", "United Kingdom", "CV47 2TH");

            //Payment Status
            HttpResponseMessage paymentResponse = WebBFFEndpoint.UpdatePaymentStatus(_testConfig.Internal.BasketId, null, "SUCCESS").EnsureIsSuccessStatusCode();

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(_testConfig.Internal.BasketId);

            if (confirmationResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR
            var pnr = confirmationResult.summary.outboundFlights[0].sectors[0].pnr.Value;

            //Get order cache not sure this is required in coversation
        }
    }
    #endregion OneWay
}