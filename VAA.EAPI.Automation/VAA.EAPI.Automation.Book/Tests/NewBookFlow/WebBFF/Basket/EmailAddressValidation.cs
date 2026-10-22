using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    //[Collection("PhoneNumberValidation")]
    public class EmailAddressValidation
    {
        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "fly.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "flycom")]
        [InlineData(LocationType.LHR, LocationType.JFK, "fly&com.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "fly_com.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "#@%^%#$@#$@#.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "abc.def@mail#archive.com")]
        public async Task TC01_EmailAddressNoAtSymbol(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //Generating offer search request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: 1, teens: 0, child: 0, infants: 0);

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

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            string emailId = inValidEmailID;
            string phoneNumer = "89896565123";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(emailId, phoneNumer, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(basketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());

            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n  {updateContact["contactDetails"]}\n----------");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "")]
        [InlineData(LocationType.LHR, LocationType.JFK, null)]
        public async Task TC02_EmailAddressEmpty(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //Generating offer search request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: 1, teens: 0, child: 0, infants: 0);

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

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            string emailId = inValidEmailID;
            string phoneNumer = "89896565123";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(emailId, phoneNumer, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(basketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());

            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n  {updateContact["contactDetails"]}\n----------");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy@@santy.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy@@santy.")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy@@")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@@santy")]
        [InlineData(LocationType.LHR, LocationType.JFK, "santy@")]
        public async Task TC03_EmailAddressDoubleAtSymbol(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //Generating offer search request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: 1, teens: 0, child: 0, infants: 0);

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

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            string emailId = inValidEmailID;
            string phoneNumer = "89896565123";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(emailId, phoneNumer, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(basketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());

            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n  {updateContact["contactDetails"]}\n----------");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test.")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test@test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test_")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@_Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@-Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester @ Test")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "123@123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "$$$$@123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "$$$$@$$$")]
        public async Task TC04_EmailAddressNoDotCom(LocationType origin, LocationType destination, string inValidEmailID)
        {
            //Generating offer search request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: 1, teens: 0, child: 0, infants: 0);

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

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            string emailId = inValidEmailID;
            string phoneNumer = "89896565123";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(emailId, phoneNumer, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(basketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());

            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == "INVALID", $"Validation value in basketResponse is not INVALID \n----------\n  {updateContact["contactDetails"]}\n----------");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@Test.test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester123@Test.test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester123@123.123.123.123")]
        [InlineData(LocationType.LHR, LocationType.JFK, "1234567890@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "firstname.lastname@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "firstname+lastname@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "_______@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "firstname-lastname@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "email@domain.co.jp")]
        [InlineData(LocationType.LHR, LocationType.JFK, "123test@test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "!fly@com.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "email@domain.museum")]
        [InlineData(LocationType.LHR, LocationType.JFK, "user.name+tag+sorting@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "abc#def@mail.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "1234567890123456789012345678901234567890123456789012345678901234+x@domain.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "domain-indeed@strange-domain.inininini")]
        [InlineData(LocationType.LHR, LocationType.JFK, "TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTes@Test.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "Tester@TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTeste.com")]
        [InlineData(LocationType.LHR, LocationType.JFK, "TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTes@TesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTesTesterTesterTesterTesterTesterTesterTesterTesterTesterTesterTeste.com")]

        public async Task TC05_EmailAddressValid(LocationType origin, LocationType destination, string validEmailID)
        {
            //Generating offer search request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: 1, teens: 0, child: 0, infants: 0);

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

            // get link to update Contact details from getBasketBFF response
            string link = getBasket["contactDetails"]["link"].ToString();

            //getbasket BFF
            HttpResponseMessage getBasketResponse_BFF = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            //update contact details BFF request
            string emailId = validEmailID;
            string phoneNumer = "89896565123";
            string altPhoneNumber = "9898965656";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            var updateContactRequest = WebBFFHelper.UpdateContactRequestBff(emailId, phoneNumer, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(basketId, link, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic updateContact = JsonConvert.DeserializeObject<dynamic>(await updateContactResponse_BFF.Content.ReadAsStringAsync());

            var validationField = updateContact["contactDetails"]["validation"].ToString();

            Assert.True(validationField == "VALID", $"Validation value in basketResponse is not VALID \n----------\n  {updateContact["contactDetails"]}\n----------");
        }
    }
}
