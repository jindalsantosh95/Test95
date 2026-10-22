namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class FlightsearchRequest
    {
        public IEnumerable<SearchOriginDestination> SearchOriginDestinations { get; set; }
        public string BrandId { get; set; }
        public bool isAwardSearch { get; set; }
        public string PromoCode { get; set; }

        public static explicit operator FlightsearchRequest(FlightOffer.FlightSearchRequest source)
        {
            if (source == null)
                return null;

            return new()
            {
                SearchOriginDestinations = source.SearchOriginDestinations.Select(OnD => (SearchOriginDestination)OnD),
                BrandId = source.BrandId,
                isAwardSearch = source.AwardSearch ?? false,
                PromoCode = source.PromoCode
            };
        }
    }

}