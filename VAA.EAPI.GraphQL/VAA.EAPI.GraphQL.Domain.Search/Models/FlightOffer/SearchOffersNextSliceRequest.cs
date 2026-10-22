namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

public class SearchOffersNextSliceRequest
{
    public Guid? BasketId { get; set; }
    public string OfferId { get; set; }
    public int CurrentTripIndexId { get; set; }
    public string? SortRequest { get; set; }
    public FlightOfferRequestFilters? Filters { get; set; }
}