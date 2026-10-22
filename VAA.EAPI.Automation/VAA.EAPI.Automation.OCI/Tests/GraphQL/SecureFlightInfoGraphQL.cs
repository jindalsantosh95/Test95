using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SecureFlightInfo;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL;

public class SecureFlightInfoGraphQL
{
    [Fact]
    [Trait("Category", Traits.CheckInGraphQLRegression)]
    public void GetAndUpdateSecureFlightInfo_ReturnsAndUpdatesSelectedPassengerDetails()
    {
        Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(
            origin: LocationType.LHR.ToString(),
            destination: "USA",
            airline: FlightType.VS.ToString());

        Assert.False(
            pnrDetails.ContainsKey("error"),
            pnrDetails.TryGetValue("error", out string errorMessage)
                ? $"Test case failed: {errorMessage}"
                : string.Empty);

        string pnrLocator = pnrDetails["pnr"];
        string givenNames = pnrDetails["firstName"];
        string surname = pnrDetails["lastName"];

        Assert.True(
            !pnrLocator.IsNullOrWhiteSpace()
            && !givenNames.IsNullOrWhiteSpace()
            && !surname.IsNullOrWhiteSpace(),
            "PNR locator, given names, and surname should not be null or empty.");

        string travelRequirementsRequest = GraphQLRequestBuilder.GetTravelRequirementsGraphQLRequest(
            pnrLocator,
            givenNames,
            surname);

        HttpResponseMessage travelRequirementsResponse = GraphQLEndpoint
            .GraphQLCheckInMobile(travelRequirementsRequest)
            .EnsureGQLResponseSuccess();

        JObject travelRequirementsResult = JObject.Parse(
            travelRequirementsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        JToken travelRequirements = travelRequirementsResult.RequireToken("data.GetTravelRequirements");

        Assert.False(
            travelRequirements["travelRequirementsId"]?.ToString().IsNullOrWhiteSpace() ?? true,
            $"travelRequirementsId is missing from response: {travelRequirementsResult}");

        JArray travelRequirementsPassengers = Assert.IsType<JArray>(
            travelRequirements.RequireToken("passengers"));
        JToken? travelRequirementsPassenger = travelRequirementsPassengers.FirstOrDefault(passenger =>
            passenger["givenNames"]?.ToString().Is(givenNames) == true
            && passenger["surname"]?.ToString().Is(surname) == true);

        Assert.True(
            travelRequirementsPassenger is not null,
            $"No passenger matched given names '{givenNames}' and surname '{surname}' in GetTravelRequirements response: {travelRequirementsResult}");

        string passengerFirstNin = travelRequirementsPassenger!.RequireToken("passengerFirstNin").ToString();
        string passengerLastNin = travelRequirementsPassenger.RequireToken("passengerLastNin").ToString();

        Assert.False(passengerFirstNin.IsNullOrWhiteSpace(),
            $"passengerFirstNin is missing from GetTravelRequirements response: {travelRequirementsResult}");
        Assert.False(passengerLastNin.IsNullOrWhiteSpace(),
            $"passengerLastNin is missing from GetTravelRequirements response: {travelRequirementsResult}");

        string secureFlightInfoRequest = GraphQLRequestBuilder.GetSecureFlightInfoGraphQLRequest(
            pnrLocator,
            givenNames,
            surname,
            passengerFirstNin,
            passengerLastNin);

        HttpResponseMessage secureFlightInfoResponse = GraphQLEndpoint
            .GraphQLCheckInMobile(secureFlightInfoRequest)
            .EnsureGQLResponseSuccess();

        JObject secureFlightInfoResult = JObject.Parse(
            secureFlightInfoResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        JToken secureFlightInfo = secureFlightInfoResult.RequireToken("data.GetSecureFlightInfo");

        Assert.False(
            secureFlightInfo["secureFlightReservationId"]?.ToString().IsNullOrWhiteSpace() ?? true,
            $"secureFlightReservationId is missing from response: {secureFlightInfoResult}");
        Assert.True(
            secureFlightInfo["showRedressNumber"]?.Type == JTokenType.Boolean,
            $"showRedressNumber must be a Boolean: {secureFlightInfoResult}");
        Assert.True(
            secureFlightInfo["showCanadianTravelNumber"]?.Type == JTokenType.Boolean,
            $"showCanadianTravelNumber must be a Boolean: {secureFlightInfoResult}");

        JToken selectedPassenger = secureFlightInfo.RequireToken("passengerDetails");

        Assert.Equal(passengerFirstNin, selectedPassenger.RequireToken("passengerFirstNin").ToString());
        Assert.Equal(passengerLastNin, selectedPassenger.RequireToken("passengerLastNin").ToString());

        string secureFlightReservationId = secureFlightInfo.RequireToken("secureFlightReservationId").ToString();
        string passengerId = selectedPassenger.RequireToken("passengerId").ToString();

        Assert.False(secureFlightReservationId.IsNullOrWhiteSpace(),
            $"secureFlightReservationId is missing from response: {secureFlightInfoResult}");
        Assert.False(passengerId.IsNullOrWhiteSpace(),
            $"passengerId is missing for the selected passenger: {secureFlightInfoResult}");

        const string canadianTravelNumber = "CAN1234567891";
        string updateSecureFlightInfoRequest = GraphQLRequestBuilder.UpdateSecureFlightInfoGraphQLRequest(
            secureFlightReservationId,
            passengerId,
            passengerFirstNin,
            passengerLastNin,
            [
                new SecureFlightDetailsInput
                {
                    type = "CanadianTravelNumber",
                    number = canadianTravelNumber
                }
            ]);

        HttpResponseMessage updateSecureFlightInfoResponse = GraphQLEndpoint
            .GraphQLCheckInMobile(updateSecureFlightInfoRequest)
            .EnsureGQLResponseSuccess();

        JObject updateSecureFlightInfoResult = JObject.Parse(
            updateSecureFlightInfoResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        JToken updatedSecureFlightInfo = updateSecureFlightInfoResult.RequireToken("data.UpdateSecureFlightInfo");

        Assert.Equal(secureFlightReservationId, updatedSecureFlightInfo.RequireToken("secureFlightReservationId").ToString());

        JToken updatedPassenger = updatedSecureFlightInfo.RequireToken("passengerDetails");

        Assert.Equal(passengerId, updatedPassenger.RequireToken("passengerId").ToString());
        Assert.Equal(canadianTravelNumber, updatedPassenger.RequireToken("canadianTravelNumber").ToString());

        string refreshedSecureFlightInfoRequest = GraphQLRequestBuilder.GetSecureFlightInfoGraphQLRequest(
            pnrLocator,
            givenNames,
            surname,
            passengerFirstNin,
            passengerLastNin);

        HttpResponseMessage refreshedSecureFlightInfoResponse = GraphQLEndpoint
            .GraphQLCheckInMobile(refreshedSecureFlightInfoRequest)
            .EnsureGQLResponseSuccess();

        JObject refreshedSecureFlightInfoResult = JObject.Parse(
            refreshedSecureFlightInfoResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        JToken refreshedPassenger = refreshedSecureFlightInfoResult
            .RequireToken("data.GetSecureFlightInfo.passengerDetails");

        Assert.Equal(canadianTravelNumber, refreshedPassenger.RequireToken("canadianTravelNumber").ToString());
    }
}
