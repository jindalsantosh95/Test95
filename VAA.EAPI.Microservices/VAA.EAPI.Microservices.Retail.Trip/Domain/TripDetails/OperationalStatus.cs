namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class OperationalStatus
    {
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public bool? RebookEligible { get; set; }

        public static explicit operator OperationalStatus(Infrastructure.Proxy.DeltaMyTrips.OperationalStatus source)
        {
            return new()
            {
                Type = source?.Type,
                SubType = source?.SubType,
                RebookEligible = source?.RebookEligible
            };
        }
    }
}