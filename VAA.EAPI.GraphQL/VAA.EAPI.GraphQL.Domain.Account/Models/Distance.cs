namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class Distance
    {
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public int DistanceInMiles { get; set; }
    }
}
