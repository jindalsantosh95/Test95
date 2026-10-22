using VAA.EAPI.Microservices.Loyalty.Sky.Constants;
using static VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities.SkyClaimResponse;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Mapper
{
    public class SkyErrorMapper
    {
        public static string RetroError(string denialCode, PostingNonEligibilityIndicators? indicator)
        {
            return denialCode switch
            {
                "101" => ErrorCodes.InvalidFlightDate,
                "102" => ErrorCodes.InvalidOperatingFlightNumber,
                "103" => ErrorCodes.FlightNumberandDetailsMismatch,
                "104" => ErrorCodes.PassengerNotFound,
                "105" => ErrorCodes.SkyTechnicalIssue,
                "106" => ErrorCodes.InvalidTicketType,
                "107" => ErrorCodes.PassengerNotFoundDuringUpdate,
                "201" => indicator == PostingNonEligibilityIndicators.Y ? ErrorCodes.ClaimRequestAlreadyPending : ErrorCodes.FFPTaggedDifferent,
                "301" => ErrorCodes.TechnicalIssueSystemDown,
                "302" => ErrorCodes.RetroClaimUnavailable,
                "303" => ErrorCodes.InvalidOperatingAirline,
                "304" => ErrorCodes.TechnicalIssueHub,
                "666" => ErrorCodes.AdditionalInfoNeeded,
                "888" => ErrorCodes.FlightNumberInvalid,
                "997" => ErrorCodes.TechnicalIssueDuringUpdate,

                _ => ErrorCodes.CustomerIneligbleorIncorrectTicketInfo,
            };
        }
    }
}
