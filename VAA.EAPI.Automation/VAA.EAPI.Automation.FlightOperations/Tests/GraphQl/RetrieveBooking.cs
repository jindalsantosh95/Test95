using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.FlightOperations.Constants;
using VAA.EAPI.Automation.FlightOperations.Requests.Endpoints.GraphQL;
using VAA.EAPI.Automation.MyAccount.Helpers;

namespace VAA.EAPI.Automation.FlightOperations.Tests.GraphQl
{
    public class RetrieveBooking : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private static string _ocpSubKey = string.Empty;

        public RetrieveBooking(TokenFixture tokenFixture)
        {
            this._tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlSecure");
            _ocpSubKey = Vault.Default.GetSecretValue("graphqlRetailingSecure");
        }

        [Fact]
        [Trait("Category", Traits.FlightOperationsGraphQlTests)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OrderDataMS")]
        [Trait("SharedServiceConsumer", "FlightOperations")]
        [Trait("TestType", "Contract")]
        public async Task RetrieveBooking_LoyaltyAccount_Success()
        {
            Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(JourneyType.OneWay.ToString(), ReservationType.AWARD.ToString());

            if (loyaltyDetails.TryGetValue("error", out string testDataError) && !testDataError.IsNullOrWhiteSpace())
            {
                Assert.Fail($"Unable to retrieve the current-week UAT one-way AWARD loyalty PNR required by the test: {testDataError}");
            }

            string fcMembershipNumber = loyaltyDetails["flyingClubNumber"];
            string email = loyaltyDetails.TryGetValue("email", out string loyaltyEmail) ? loyaltyEmail : string.Empty;
            string loyaltyAccountPassword = Vault.Default.GetSecretValue("MYA-Password");

            string idToken = await EntraTokenHelper.GetIdToken(Config.Environment, email, fcMembershipNumber, loyaltyAccountPassword);

            Assert.False(idToken.IsNullOrWhiteSpace(), "Entra ID token is null.");

            HttpResponseMessage response = GraphQL.RetrieveBookingsForLoyaltyAccount(_tokenFixture.GetToken("graphqlSecure"), idToken, _ocpSubKey);

            Assert.False(response.ReadResponseAsString().Has("No bookings found for the provided FC Number"), $"No bookings were found for the loyalty account when they should have been. FcNumber - {fcMembershipNumber}");
        }

        [Theory]
        [Trait("Category", Traits.FlightOperationsGraphQlTests)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "Retail.OrderSearchMS")]
        [Trait("SharedServiceConsumer", "FlightOperations")]
        [Trait("TestType", "Contract")]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public void RetrieveBooking_Pnr_Success(JourneyType journeyType, ReservationType reservationType)
        {
            Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyType.ToString(), reservationType.ToString());

            string pnr = loyaltyDetails["pnr"];
            string firstName = loyaltyDetails["firstName"];
            string lastName = loyaltyDetails["lastName"];
            string destination = loyaltyDetails["destination"];

            var response = GraphQL.RetrieveBookingPnr(_tokenFixture.GetToken("graphqlSecure"), pnr, firstName, lastName, _ocpSubKey);

            JObject retrieveBookingJobject = JObject.Parse(response.ReadResponseAsString());

            string returnedPnr = retrieveBookingJobject.SelectToken("$.data.retrieveBooking.recordLocator").ToString();
            string returnedFirstName = retrieveBookingJobject.SelectToken("$.data.retrieveBooking.passengers[0].firstName").ToString();
            string returnedLastName= retrieveBookingJobject.SelectToken("$.data.retrieveBooking.passengers[0].lastName").ToString();
            string returnedDestination = retrieveBookingJobject.SelectToken("$.data.retrieveBooking.segments[0].destination").ToString();

            Assert.True(pnr.Equals(returnedPnr), $"Pnr returned was not as expected. Expected {pnr} returned {returnedPnr}");
            Assert.True(firstName.Equals(returnedFirstName), $"FirstName returned was not as expected. Expected {firstName} returned {returnedFirstName}");
            Assert.True(lastName.Equals(returnedLastName), $"LastName returned was not as expected. Expected {lastName} returned {returnedLastName}");
            Assert.True(destination.Equals(returnedDestination), $"Destination returned was not as expected. Expected {destination} returned {destination}");
        }
    }
}
