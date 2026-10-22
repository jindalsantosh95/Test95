using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public class TravelRequirementsIdHelper : BaseIdHelper
    {
        private static readonly string jwtKey = ConfigReader.Get(Common.Constants.Environment.CheckInIdJwtKey);

        public static async Task<string> CreateTravelRequirementsId(string? existingTravelRequirementsId, string? requirementsId = null, bool? resetReqIdExpiryTime = null, string? firstName = null, string? lastName = null, string? pnrLocator = null, IEnumerable<string>? selectedPassengers = null)
        {
            var claims = new List<Claim> { };

            claims = await MergeClaim(claims, existingTravelRequirementsId, jwtKey, "requirementsId", requirementsId);
            claims = await MergeClaim(claims, existingTravelRequirementsId, jwtKey, "pnrLocator", pnrLocator);
            claims = await MergeClaim(claims, existingTravelRequirementsId, jwtKey, "firstName", firstName);
            claims = await MergeClaim(claims, existingTravelRequirementsId, jwtKey, "lastName", lastName);
            claims = await MergeClaim(claims, existingTravelRequirementsId, jwtKey, "selectedPassengers", selectedPassengers != null ? string.Join(";", selectedPassengers) : null);
            claims = await MergeClaim(claims, existingTravelRequirementsId, jwtKey, "requirementsIdExpiryTime", resetReqIdExpiryTime == true && requirementsId.IsNullOrEmpty() == false
                ? new DateTimeOffset(DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Common.Constants.Environment.RequirementIdExpiryTimeinMin))).ToUnixTimeSeconds().ToString() : null);

            return await GetTokenString(claims, DateTime.UtcNow.AddDays(ConfigReader.Get<int>(Common.Constants.Environment.PaxOpsJwtTokenExpiryTimeinDays)), jwtKey);
        }

        public static async Task<string> ExtendSessions(ITravelRequirementsServiceProvider travelRequirementsServiceProvider, string travelRequirementsId)
        {
            if (await TravelRequirementsIdExpiryTime(travelRequirementsId, "requirementsId") == false)
            {
                var travelRequirementsResponse = await travelRequirementsServiceProvider.GetTravelRequirementsAsync(new Models.TravelRequirements.TravelRequirementsRequest
                {
                    PnrLocator = await GetPnrLocator(travelRequirementsId),
                    GivenNames = await GetFirstName(travelRequirementsId),
                    Surname = await GetLastName(travelRequirementsId)
                });
                if (travelRequirementsResponse?.RequirementsId.IsNullOrEmpty() != false)
                {
                    throw new InvalidOperationException("Unable to extend Travel Requirements session: RequirementsId was missing from the service response.");
                }

                travelRequirementsId = await CreateTravelRequirementsId(travelRequirementsId, travelRequirementsResponse.RequirementsId, true);
            }

            return travelRequirementsId;
        }

        public static async Task<string?> GetPnrLocator(string travelRequirementsId)
        {
            return await GetClaim(travelRequirementsId, jwtKey, "pnrLocator");
        }

        public static async Task<string?> GetFirstName(string travelRequirementsId)
        {
            return await GetClaim(travelRequirementsId, jwtKey, "firstName");
        }

        public static async Task<string?> GetLastName(string travelRequirementsId)
        {
            return await GetClaim(travelRequirementsId, jwtKey, "lastName");
        }

        public static async Task<IEnumerable<string>?> GetSelectedPassengers(string travelRequirementsId)
        {
            var selectedPassengers = await GetClaim(travelRequirementsId, jwtKey, "selectedPassengers");
            if (selectedPassengers.IsNullOrEmpty()) return null;

            return selectedPassengers.Split(";");
        }

        public static async Task<string?> GetRequirementsId(string travelRequirementsId)
        {
            if (await TravelRequirementsIdExpiryTime(travelRequirementsId, "requirementsId"))
                return await GetClaim(travelRequirementsId, jwtKey, "requirementsId");

            return null;
        }
         
        public static async Task<bool> TravelRequirementsIdExpiryTime(string travelRequirementsId, string idType)
        {
            var idExpiryTime = await GetClaim(travelRequirementsId, jwtKey, $"{idType}ExpiryTime");

            if (idExpiryTime.IsNullOrEmpty() || !long.TryParse(idExpiryTime, out var expiry) || expiry < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return false;

            return true;
        }
    }
}
