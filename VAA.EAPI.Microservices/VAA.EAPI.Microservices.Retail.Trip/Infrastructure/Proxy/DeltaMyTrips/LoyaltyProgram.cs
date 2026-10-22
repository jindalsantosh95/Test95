namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class LoyaltyProgram
    {
        public string? Name { get; set; }
        public IEnumerable<Owner>? Owner { get; set; }
    }
}