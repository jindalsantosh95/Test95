namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

public class FlightOfferRequest : FlightOfferBaseRequest
{
    public string? PromoCode { get; set; }
    public Guid? BasketId { get; set; }
}