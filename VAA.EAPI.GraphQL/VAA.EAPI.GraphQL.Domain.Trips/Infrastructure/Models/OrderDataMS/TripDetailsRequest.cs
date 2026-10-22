namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderDataMS
{
    public class TripDetailsRequest
    {
        public IEnumerable<TripDetails> TripDetails { get; set; }

        public static explicit operator TripDetailsRequest(Trips.Models.Trips.TripDetailsRequest request)
        {
            return new()
            {
                TripDetails = request.TripDetails.Select(x => (TripDetails)x)
            };
        }
    }
}