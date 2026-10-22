using System.Web;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class FlightOfferRequestFilters
    {
        private string? cabins;

        public string? Airlines { get; set; }
        public string? DepartureTimes { get; set; }
        public string? Connections { get; set; }
        public string? Cabins
        {
            get => HttpUtility.UrlDecode(cabins);
            set => cabins = value;
        }
    }
}