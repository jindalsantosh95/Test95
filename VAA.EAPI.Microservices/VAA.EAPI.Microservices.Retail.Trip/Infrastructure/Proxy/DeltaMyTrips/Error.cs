namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public IEnumerable<MoreInfo> MoreInfo { get; set; }
    }
}