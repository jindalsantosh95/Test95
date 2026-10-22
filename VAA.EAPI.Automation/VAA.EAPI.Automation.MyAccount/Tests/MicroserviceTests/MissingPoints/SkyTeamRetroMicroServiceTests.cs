using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.MissingPoints
{
    public class SkyTeamRetroMicroServiceTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public static string env = Config.Environment;

        public SkyTeamRetroMicroServiceTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForSkyTeamMicroserviceFromKeyVault("skyTeam");
        }

        [Theory]
        [InlineData("1078896725", "Test", "MemberB", "612", "LHR", "CDG", "5487626683478")]
        [InlineData("1078896733", "Test", "MemberC", "274", "YVR", "LHR", "5487626683479")]
        [InlineData("1078896741", "Test", "MemberD", "3013", "IST", "YVR", "5487626683480")]
        [InlineData("1078896758", "Test", "MemberE", "104A", "YYZ", "IST", "5487626683481")]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimStatus(string fcNumber, string firstName, string lastName, string flightNumber, string origin, string destination, string ticketNumber)
        {
            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber).EnsureIsSuccessStatusCode();

            JObject skyTeamClaimEligibilityResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            SoftAssertHelper softAssert = new();

            softAssert.True(fcNumber.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["ffpAccountNumber"].ToString()), $"In Response fcNumber is wrong expected {fcNumber} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["ffpAccountNumber"].ToString()}");

            softAssert.True(firstName.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["passengerFirstName"].ToString()), $"In Response passenger first name is wrong expected {firstName} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["passengerFirstName"].ToString()}");

            softAssert.True(lastName.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["passengerLastName"].ToString()), $"In Response passenger last name is wrong expected {lastName} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["passengerLastName"].ToString()}");

            softAssert.True(flightNumber.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["operatingFlightNumber"].ToString()), $"In Response flight number is wrong expected {flightNumber} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["operatingFlightNumber"].ToString()}");

            softAssert.True(origin.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["flightSegmentOrigin"].ToString()), $"In Response origin is wrong expected {origin} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["flightSegmentOrigin"].ToString()}");

            softAssert.True(destination.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["flightSegmentDestination"].ToString()), $"In Response destination is wrong expected {destination} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["flightSegmentDestination"].ToString()}");

            softAssert.True(ticketNumber.Equals(skyTeamClaimEligibilityResult["skyClaimResponse"]["eticketNumber"].ToString()), $"In Response ticket number is wrong expected {ticketNumber} but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["eticketNumber"].ToString()}");

            softAssert.True("000".Equals(skyTeamClaimEligibilityResult["eligibilityDenialCode"].ToString()), $"In Response Claim Eligibility Denial code is wrong expected '000' but actual {skyTeamClaimEligibilityResult["eligibilityDenialCode"].ToString()}");

            softAssert.True(skyTeamClaimEligibilityResult["skyClaimResponse"]["uniqueIdentificationNumber"].ToString() != null, $"In Response Unique identification number is expected not null but actual {skyTeamClaimEligibilityResult["skyClaimResponse"]["uniqueIdentificationNumber"].ToString()}");

            softAssert.AssertAll();
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenClaimAlreadyPending()
        {
            string fcNumber = "1078896774";
            string firstName = "Test";
            string lastName = "MemberF";
            string flightNumber = "613";
            string origin = "AMS";
            string destination = "YYZ";
            string ticketNumber = "5487626683482";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S201R";
            string errorMessage = "There is already a retro claim request pending for the same flight.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenClaimAlreadyCompleted()
        {
            string fcNumber = "1078896808";
            string firstName = "Test";
            string lastName = "MemberI";
            string flightNumber = "3014";
            string origin = "CDG";
            string destination = "PEK";
            string ticketNumber = "5487626683484";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S201Y";
            string errorMessage = "There is already a retro claim request completed for the same flight.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenClaimDateIsWrong()
        {
            string fcNumber = "1078896857";
            string firstName = "Test";
            string lastName = "MemberM";
            string flightNumber = "615";
            string origin = "PEK";
            string destination = "CDG";
            string ticketNumber = "5487626683490";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S101";
            string errorMessage = "The flight date entered is invalid.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenFlightNoIsWrong()
        {
            string fcNumber = "1078896816";
            string firstName = "Test";
            string lastName = "MemberJ";
            string flightNumber = "3017";
            string origin = "AMS";
            string destination = "YVR";
            string ticketNumber = "5487626683496";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S102";
            string errorMessage = "The flight number entered is invalid.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenOriginAndDestinationIsWrong()
        {
            string fcNumber = "1078896832";
            string firstName = "Test";
            string lastName = "MemberL";
            string flightNumber = "617";
            string origin = "LHR";
            string destination = "YYZ";
            string ticketNumber = "5487626683498";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S103";
            string errorMessage = "The origin/destination entered does not match with the flight number.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenPassengerInfoIsWrong()
        {
            string fcNumber = "1078896881";
            string firstName = "Test";
            string lastName = "MemberN";
            string flightNumber = "618";
            string origin = "YYZ";
            string destination = "CDG";
            string ticketNumber = "5487626683502";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S104";
            string errorMessage = "Passenger not found on this flight by the operating airline.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenNeedAdditionalInfo()
        {
            string fcNumber = "1078896816";
            string firstName = "Test";
            string lastName = "MemberJ";
            string flightNumber = "619";
            string origin = "LHR";
            string destination = "YYZ";
            string ticketNumber = "5487626683506";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S666";
            string errorMessage = "The operating airline has requested additional information to locate the passenger.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }

        [Fact]
        [Trait("Category", "SkyMicroServiceTests")]
        public async Task ValidateSkyTeamTicketClaimWhenTicketNumberIsWrong()
        {
            string fcNumber = "1078896816";
            string firstName = "Test";
            string lastName = "MemberJ";
            string flightNumber = "620";
            string origin = "AMS";
            string destination = "PEK";
            string ticketNumber = "5487626683510";

            System.Net.Http.HttpResponseMessage skyTeamClaimEligibilityResponse = Microservice.SkyTeamClaimEligibility(env, tokenFixture.GetToken("skyTeam"), fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber);

            Assert.True(skyTeamClaimEligibilityResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from sky ms, returned {skyTeamClaimEligibilityResponse.ReadResponseAsString()}");

            JArray skyTeamClaimEligibilityResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await skyTeamClaimEligibilityResponse.Content.ReadAsStringAsync());

            string errorCode = "S106";
            string errorMessage = "The ticket type does not qualify for points.";

            Assert.True(skyTeamClaimEligibilityResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {skyTeamClaimEligibilityResult}");

            Assert.True(skyTeamClaimEligibilityResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {skyTeamClaimEligibilityResult}");
        }
    }
}
