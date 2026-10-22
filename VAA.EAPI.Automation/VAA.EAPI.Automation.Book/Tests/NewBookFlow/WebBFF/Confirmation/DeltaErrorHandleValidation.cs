using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Confirmation
{
    [Collection("Bff_E2E_extensive")]
    public class DeltaErrorHandleValidation : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();
        public string deltaHPPUrl = "https://delta-hpp.webdev.vholsinternal.co.uk/hpp?paymentReferenceId=";

        public DeltaErrorHandleValidation(TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("374500261001009", "AMEX", "Non3DS", LocationType.SFO, LocationType.LHR, 1, 1, 1, 1, FareFamilyType.COMFORTPLUSPREMIUMECON)]
        public async Task TC01_7522PaymentError_PASSENGER_VISANon3DS_card_ExUS_USbilling_VirginOnly(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamily)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            string summaryLink = string.Empty;
            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(result, fareFamily, FlightType.VS);
            Assert.False(summaryLinks.Length == 0, $"No summary links found for {origin} and {destination} and {fareFamily}, {cardNumber}");
            summaryLink = summaryLinks[0];
            var url = "";
            var basketId = "";
            //Getting basketId from the response url
            basketId = summaryLink.Split('/')[5];
            var slice = summaryLink.Split('/')[7];
            var offerId = summaryLink.Split('/').LastOrDefault();

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            Assert.True(WebBFFAssertions.ValidateFareFamily(summaryBffResponse.ReadResponseAsString(), fareFamily, out string fareFamilyType), fareFamilyType);

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

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United States", "12345", state: "Alaska");

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(basketId);
            Assert.True(confirmationResponse.StatusCode.Equals(System.Net.HttpStatusCode.UnprocessableEntity));
            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());
            var expCode = confirmationResult[0]["code"].Value.ToString();
            Assert.True(expCode.Equals("7522"), $"Error code not as expected, it is {expCode}");
            var expErrorTag = confirmationResult[0]["errorTag"].Value.ToString();
            Assert.True(expErrorTag.Equals("PASSENGER"), $"Error Tag not as expected, it is {expErrorTag}");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4000027891380961", "VISA", "Non3DS", LocationType.LHR, LocationType.JFK, 1, 0, 0, 0, FareFamilyType.ECON)]
        public async Task TC02_4067FqtFLyError_PASSENGER_VISANon3DS_card_VirginOnly(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamily)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            string summaryLink = string.Empty;
            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(result, fareFamily, FlightType.VS);
            Assert.False(summaryLinks.Length == 0, $"No summary links found for {origin} and {destination} and {fareFamily}, {cardNumber}");

            summaryLink = summaryLinks[0];
            var url = "";
            var basketId = "";
            //Getting basketId from the response url
            basketId = summaryLink.Split('/')[5];
            var slice = summaryLink.Split('/')[7];
            var offerId = summaryLink.Split('/').LastOrDefault();

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            Assert.True(WebBFFAssertions.ValidateFareFamily(summaryBffResponse.ReadResponseAsString(), fareFamily, out string fareFamilyType), fareFamilyType);

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());
            string peopleLink = getBasket["people"][0]["link"].ToString();
            //Helper.UpdateAllPassengerDetails(getBasket, basketId);
            string firstName = "Antoinette";
            string lastName = "Stamm";
            string dateOfBirth = "2000-10-10";
            string gender = "Male";
            string type = "ADULT";
            string fqtProgram = "VS";
            string flyNumber = "2104553389";
            var passenegerReq = WebBFFHelper.UpdatePassengerRequestBff(firstName, lastName, dateOfBirth, gender, type, fqtProgram, flyNumber);

            var updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(basketId, peopleLink, passenegerReq).EnsureIsSuccessStatusCode();

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

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United States", "12345", state: "Alaska");

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(basketId);
            Assert.True(confirmationResponse.StatusCode.Equals(System.Net.HttpStatusCode.UnprocessableEntity));
            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());
            var expCode = confirmationResult[0]["code"].Value.ToString();
            Assert.True(expCode.Equals("4067"), $"Error code not as expected, it is {expCode}");
            var expErrorTag = confirmationResult[0]["errorTag"].Value.ToString();
            Assert.True(expErrorTag.Equals("PASSENGER"), $"Error Tag not as expected, it is {expErrorTag}");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4000027891380961", "VISA", "Non3DS", LocationType.LHR, LocationType.JFK, 1, 0, 0, 0, FareFamilyType.ECON)]
        public async Task TC03_2007Error_HOME_VISANon3DS_card_VirginOnly(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamily)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            string summaryLink = string.Empty;
            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(result, fareFamily, FlightType.VS);
            Assert.False(summaryLinks.Length == 0, $"No summary links found for {origin} and {destination} and {fareFamily}, {cardNumber}");

            summaryLink = summaryLinks[0];
            var url = "";
            var basketId = "";
            //Getting basketId from the response url
            basketId = summaryLink.Split('/')[5];
            var slice = summaryLink.Split('/')[7];
            var offerId = summaryLink.Split('/').LastOrDefault();

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            Assert.True(WebBFFAssertions.ValidateFareFamily(summaryBffResponse.ReadResponseAsString(), fareFamily, out string fareFamilyType), fareFamilyType);

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());
            string peopleLink = getBasket["people"][0]["link"].ToString();
            //Helper.UpdateAllPassengerDetails(getBasket, basketId);
            string firstName = "Antoinette";
            string lastName = "Stamm";
            string dateOfBirth = "2000-10-10";
            string gender = "Male";
            string type = "ADULT";
            string fqtProgram = "";
            string flyNumber = "2104553389";
            var passenegerReq = WebBFFHelper.UpdatePassengerRequestBff(firstName, lastName, dateOfBirth, gender, type, fqtProgram, flyNumber);

            var updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(basketId, peopleLink, passenegerReq).EnsureIsSuccessStatusCode();

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

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United States", "12345", state: "Alaska");

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(basketId);

            Assert.True(confirmationResponse.StatusCode.Equals(System.Net.HttpStatusCode.UnprocessableEntity), $"Status code was not as expected. Expected {System.Net.HttpStatusCode.UnprocessableEntity} (415) but was {confirmationResponse.StatusCode}.");

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            var expCode = confirmationResult[0]["code"].Value.ToString();
            Assert.True(expCode.Equals("2007"), $"Error code not as expected, it is {expCode}");

            var expErrorTag = confirmationResult[0]["errorTag"].Value.ToString();
            Assert.True(expErrorTag.Equals("HOME"), $"Error Tag not as expected, it is {expErrorTag}");
        }
    }
}