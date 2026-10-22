using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class TripDetailsLoggedInUserTests : TripsGraphQlTestsBase
    {
        [SkippableTheory]
        [Trait("Category", Traits.TripDetailsForLoggedInUser)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveMultipleTripsForLoggedInFlyingClubUser_ReturnsTripDetails(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching PNRs from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, loggedInUser: true, environment: env);

            if (pnrDetails == null)
            {
                TestDataPrerequisiteHelper.Skip("Test skipped: prerequisite PNR data is not available in storage.");
                return;
            }

            string preCheckContext = RequestHelper.FormatTripsBookingContext(
                pnrDetails.TryGetValue("pnr", out string preCheckPnr) ? preCheckPnr ?? string.Empty : string.Empty,
                pnrDetails.TryGetValue("firstName", out string preCheckFirstName) ? preCheckFirstName ?? string.Empty : string.Empty,
                pnrDetails.TryGetValue("lastName", out string preCheckLastName) ? preCheckLastName ?? string.Empty : string.Empty);

            if (pnrDetails.TryGetValue("error", out string error) && !error.IsNullOrWhiteSpace())
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: prerequisite PNR data unavailable. {error}. {preCheckContext}");
                return;
            }

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion

            #region calling MYA graphql for generation of the idToken for the member

            string password = VAA.EAPI.Automation.Core.Vault.Default.GetSecretValue("MYA-Password");

            HttpResponseMessage accountMemberSignInresponse = GraphQlEndpoint.AccountMemberSignIn(env, loyaltyNum, password).EnsureIsSuccessStatusCode();
            dynamic accountMemberSignInResult = JsonConvert.DeserializeObject<dynamic>(await accountMemberSignInresponse.Content.ReadAsStringAsync());

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(accountMemberSignInResult, "data"), $"Response should contain data property. {debugContext}");

            string signInErrors = DynamicHelper.HasProperty(accountMemberSignInResult, "errors")
                ? accountMemberSignInResult.errors?.ToString() ?? string.Empty
                : string.Empty;
            Assert.True(signInErrors.IsNullOrWhiteSpace(), $"AccountMemberSignIn returned GraphQL errors for loyalty number {loyaltyNum}: {signInErrors}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(accountMemberSignInResult.data, "accountMemberSignIn"), $"Response should contain accountMemberSignIn property. {debugContext}");

            dynamic signInPayload = accountMemberSignInResult.data.accountMemberSignIn;
            Assert.True(signInPayload != null, $"accountMemberSignIn is null — sign-in failed for loyalty number {loyaltyNum}. Check credentials or MFA requirement. {debugContext}");

            string idToken = signInPayload?.idToken?.ToString() ?? string.Empty;

            Assert.True(!idToken.IsNullOrWhiteSpace(), $"idToken should not be null or empty. {debugContext}");

            #endregion

            #region - calling TripsDetails endpoint with token details passsed in Headers section

            HttpResponseMessage TripDetailsLoggedInUserResponse = GraphQlEndpoint.TripDetailsLoggedInUserToken(env, idToken).EnsureIsSuccessStatusCode();
            dynamic TripDetailsLoggedInUserResult = JsonConvert.DeserializeObject<dynamic>(await TripDetailsLoggedInUserResponse.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(TripDetailsLoggedInUserResult, "data"), $"Response should contain data property. {debugContext}");

            #endregion

            #region - Validations

            dynamic tripDetails = TripDetailsLoggedInUserResult.data.TripDetails;
            Assert.NotNull(tripDetails);

            if (tripDetails.Count == 0)
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: No trips returned for logged-in user. PNR {pnr} does not exist in Cosmos instance for loyalty number {loyaltyNum}. {debugContext}");
                return;
            }

            // Validate trip details contains at least 1 trip
            Assert.True(tripDetails.Count >= 1, $"Expected at least 1 trip in response but got {tripDetails.Count}. {debugContext}");

            // Validate the PNR from storage exists in response
            List<string> returnedPnrs = new();
            foreach (dynamic? trip in tripDetails)
            {
                string pnrFromTrip = trip.tripSearchRequest.pnrLocator.ToString();
                returnedPnrs.Add(pnrFromTrip);
            }
            Assert.True(returnedPnrs.Contains(pnr), $"Expected PNR {pnr} to be in response but got {string.Join(", ", returnedPnrs)}. {debugContext}");

            // Validate isAccountLinkedPNR is true for the matched PNR
            dynamic matchedTrip = null;
            foreach (dynamic? trip in tripDetails)
            {
                string tripPnr = trip.tripSearchRequest.pnrLocator.ToString();
                if (tripPnr == pnr)
                {
                    matchedTrip = trip;
                    break;
                }
            }
            Assert.True(matchedTrip != null, $"Could not find trip details for PNR {pnr} in the response");

            Assert.True((bool)matchedTrip.isAccountLinkedPNR, $"Expected isAccountLinkedPNR to be true for PNR {pnr}");

            #endregion
        }

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OrderDataMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_RetrieveOneWayTripForLoggedInFlyingClubUser()
        {
            await RetrieveMultipleTripsForLoggedInFlyingClubUser_ReturnsTripDetails(
                JourneyType.OneWay,
                ReservationType.REVENUE);
        }
    }
}

