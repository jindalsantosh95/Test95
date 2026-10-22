namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS
{
    public class GetBookingDetailsResponse
    {
        public string CabinClass { get; set; }
        public string BookClass { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public int DistanceInMiles { get; set; }
    }
}
