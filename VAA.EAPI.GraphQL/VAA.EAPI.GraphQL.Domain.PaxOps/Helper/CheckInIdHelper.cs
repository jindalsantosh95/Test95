using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public class CheckInIdHelper : BaseIdHelper
    {
        private static readonly string jwtKey = ConfigReader.Get(Common.Constants.Environment.CheckInIdJwtKey);

        public static async Task<string> CreateCheckInId(string? existingCheckInId, string? requirementsId = null, bool? resetReqIdExpiryTime = null, string? travelPartyId = null, bool? resetTraParIdExpiryTime = null, string? pnrLocator = null, string? originCode = null, IEnumerable<string>? selectedPassengers = null, string? firstName = null, string? lastName = null, string? basketId = null, bool? resetBasketIdExpiryTime = null)
        {
            var claims = new List<Claim> { };

            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "requirementsId", requirementsId);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "travelPartyId", travelPartyId);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "pnrLocator", pnrLocator);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "originCode", originCode);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "firstName", firstName);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "lastName", lastName);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "basketId", basketId);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "requirementsIdExpiryTime",
                resetReqIdExpiryTime == true && requirementsId.IsNullOrEmpty() == false
                ? new DateTimeOffset(DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Common.Constants.Environment.RequirementIdExpiryTimeinMin))).ToUnixTimeSeconds().ToString() : null);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "travelPartyIdExpiryTime",
                resetTraParIdExpiryTime == true && travelPartyId.IsNullOrEmpty() == false
                ? new DateTimeOffset(DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Common.Constants.Environment.TravelPartyIdExpiryTimeinMin))).ToUnixTimeSeconds().ToString() : null);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "basketIdExpiryTime",
                resetBasketIdExpiryTime == true && basketId.IsNullOrEmpty() == false
                ? new DateTimeOffset(DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Common.Constants.Environment.BasketIdExpiryTimeinMin))).ToUnixTimeSeconds().ToString() : null);
            claims = await MergeClaim(claims, existingCheckInId, jwtKey, "selectedPassengers", selectedPassengers != null ? string.Join(";", selectedPassengers) : null);

            return await GetTokenString(claims, DateTime.UtcNow.AddDays(1), jwtKey);
        }

        public static async Task<string> ExtendSessions(ICheckInServiceProvider checkInServiceProvider, string checkInId)
        {
            if ((await CheckInIdHelper.CheckIdExpiryTime(checkInId, "requirementsId") || await CheckInIdHelper.CheckIdExpiryTime(checkInId, "travelPartyId")) == false)
            {
                var selectedPassengers = await CheckInIdHelper.GetSelectedPassengers(checkInId);

                var checkInResponse = await checkInServiceProvider.InitiateCheckInAsync(new CheckInRequest
                {
                    PnrLocator = await GetPnrLocator(checkInId),
                    OriginCode = await GetOriginCode(checkInId),
                    GivenNames = await GetFirstName(checkInId),
                    Surname = await GetLastName(checkInId)
                }, mapHttpFailureToResponse: false);

                checkInId = await CheckInIdHelper.CreateCheckInId(
                    checkInId,
                    checkInResponse?.RequirementsId, true,
                    checkInResponse?.TravelPartyId, true,
                    null, null, null, null, null);

                if (checkInResponse.Passengers != null && checkInResponse.Passengers.Count() != selectedPassengers.Count())
                {
                    checkInResponse = await checkInServiceProvider.SelectPassengersAsync(
                        await Models.SelectPassenger.SelectPassengerServiceRequest.CreateRequestAsync(checkInId, selectedPassengers),
                        checkInId,
                        mapHttpFailureToResponse: false);
                }

                checkInId = await CheckInIdHelper.CreateCheckInId(
                    checkInId,
                    checkInResponse?.RequirementsId, true,
                    checkInResponse?.TravelPartyId, true,
                    null, null,
                    checkInResponse?.Passengers?.Select(psg => psg.PassengerId),
                    null, null);
            }

            return checkInId;
        }

        public static async Task<string?> GetRequirementsId(string checkInId)
        {
            if (await CheckIdExpiryTime(checkInId, "requirementsId"))
                return await GetClaim(checkInId, jwtKey, "requirementsId");

            return null;
        }

        public static async Task<string?> GetTravelPartyId(string checkInId)
        {
            if (await CheckIdExpiryTime(checkInId, "travelPartyId"))
                return await GetClaim(checkInId, jwtKey, "travelPartyId");

            return null;
        }

        public static async Task<IEnumerable<string>?> GetSelectedPassengers(string checkInId)
        {
            var selectedPassengers = await GetClaim(checkInId, jwtKey, "selectedPassengers");
            if (selectedPassengers.IsNullOrEmpty()) return null;

            return selectedPassengers.Split(";");
        }

        public static async Task<bool> CheckIdExpiryTime(string checkInId, string idType)
        {
            var idExpiryTime = await GetClaim(checkInId, jwtKey, $"{idType}ExpiryTime");

            if (idExpiryTime.IsNullOrEmpty() || long.Parse(idExpiryTime) < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return false;

            return true;
        }

        public static async Task<string?> GetPnrLocator(string checkInId)
        {
            return await GetClaim(checkInId, jwtKey, "pnrLocator");
        }

        public static async Task<string?> GetOriginCode(string checkInId)
        {
            return await GetClaim(checkInId, jwtKey, "originCode");
        }

        public static async Task<string?> GetFirstName(string checkInId)
        {
            return await GetClaim(checkInId, jwtKey, "firstName");
        }

        public static async Task<string?> GetLastName(string checkInId)
        {
            return await GetClaim(checkInId, jwtKey, "lastName");
        }

        public static async Task<string?> GetBasketId(string checkInId)
        {
            if (await CheckIdExpiryTime(checkInId, "basketId"))
                return await GetClaim(checkInId, jwtKey, "basketId");

            return null;
        }
    }
}
