using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;

public class CalendarSearchRequest
{
    public IEnumerable<SearchOriginDestination>? SearchOriginDestinations { get; set; }
    public bool CalendarSearch { get; set; }
    public bool AwardSearch { get; set; }
    public string PromoCode { get; set; }
    public string CurrentTripIndexId { get; set; }
    public string? BrandId { get; set; }
}