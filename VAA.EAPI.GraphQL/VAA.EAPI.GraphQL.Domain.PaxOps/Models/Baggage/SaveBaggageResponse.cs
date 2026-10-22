
using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class SaveBaggageResponse
    {
        public string CheckInId { get; set; }
        public bool Success { get; set; }

        public static explicit operator SaveBaggageResponse(string checkInId)
        {
            return new SaveBaggageResponse
            {
                CheckInId = checkInId,
                Success = !string.IsNullOrWhiteSpace(checkInId) ? true : false
            };
        }
    }
}
