namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

public class FlightOfferNextRequest : FlightOfferBaseRequest
{
    public Guid? BasketId { get; set; }
}