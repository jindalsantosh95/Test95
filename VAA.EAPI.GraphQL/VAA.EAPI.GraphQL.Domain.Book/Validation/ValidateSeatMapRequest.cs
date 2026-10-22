using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;


namespace VAA.EAPI.GraphQL.Domain.Book.Validation
{
    public class ValidateSeatMap
    {
        public static async Task<bool> ValidateSeatMapRequest(FlightSeatMapRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.FlightSegment == null)
            {
                throw new ArgumentNullException(nameof(request.FlightSegment));
            }

            return true;

        }
    }
}
