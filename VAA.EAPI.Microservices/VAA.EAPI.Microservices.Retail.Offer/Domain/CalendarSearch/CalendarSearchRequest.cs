using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch;

public class CalendarSearchRequest
{
    public List<SearchOriginDestination> SearchOriginDestinations { get; set; }
    public string CurrentTripIndexId { get; set; }
    public string BrandId { get; set; }
    public string PromoCode { get; set; }
    public bool AwardSearch { get; set; }
}
