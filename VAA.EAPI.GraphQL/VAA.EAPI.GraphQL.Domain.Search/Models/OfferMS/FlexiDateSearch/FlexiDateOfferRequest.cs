using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;

public class FlexiDateOfferRequest
{
    public Pos? Pos { get; set; }
    public Parties? Parties { get; set; }
    public FlexiDateSearchRequest? FlightSearchRequest { get; set; }
    public IEnumerable<Customer>? CustomerDetails { get; set; }
    public LoyaltyMemberDetails? LoyaltyMember { get; set; }
    public string? CurrencyCode { get; set; }

    public static explicit operator FlexiDateOfferRequest(FlightOfferBaseRequest source)
    {
        return new FlexiDateOfferRequest
        {
            Pos = new Pos(),
            Parties = new Parties(),
            CurrencyCode = source.CurrencyCode,
            FlightSearchRequest = new FlexiDateSearchRequest()
            {
                SearchOriginDestinations = source.FlightSearchRequest.SearchOriginDestinations,
                FlexiDateSearch = true,
                AwardSearch = source.FlightSearchRequest.AwardSearch ?? false,
                PromoCode = source.FlightSearchRequest.PromoCode,
                CurrentTripIndexId = source.FlightSearchRequest.CurrentTripIndexId,
                BrandId = source.FlightSearchRequest.BrandId
            },
            CustomerDetails = source.CustomerDetails
        };
    }
}