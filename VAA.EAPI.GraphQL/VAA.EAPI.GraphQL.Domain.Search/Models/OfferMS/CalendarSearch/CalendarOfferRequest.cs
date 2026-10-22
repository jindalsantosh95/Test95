using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;

public class CalendarOfferRequest
{
    public Pos? Pos { get; set; }
    public Parties? Parties { get; set; }
    public CalendarSearchRequest? FlightSearchRequest { get; set; }
    public IEnumerable<Customer>? CustomerDetails { get; set; }
    public LoyaltyMemberDetails? LoyaltyMember { get; set; }
    public string? CurrencyCode { get; set; }

    public static explicit operator CalendarOfferRequest(FlightOfferBaseRequest source)
    {
        return new CalendarOfferRequest
        {
            Pos = new Pos(),
            Parties = new Parties(),
            CurrencyCode = source.CurrencyCode,
            FlightSearchRequest = new CalendarSearchRequest()
            {
                SearchOriginDestinations = source.FlightSearchRequest.SearchOriginDestinations,
                CalendarSearch = true,
                AwardSearch = source.FlightSearchRequest.AwardSearch ?? false,
                PromoCode = source.FlightSearchRequest.PromoCode,
                CurrentTripIndexId = source.FlightSearchRequest.CurrentTripIndexId,
                BrandId = source.FlightSearchRequest.BrandId
            },
            CustomerDetails = source.CustomerDetails
        };
    }
}