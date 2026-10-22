using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;

public class FlexiDateSearchRequest
{
    public List<SearchOriginDestination> SearchOriginDestinations { get; set; }
    public string CurrentTripIndexId { get; set; }
    public string BrandId { get; set; }
    public string PromoCode { get; set; }
    public bool AwardSearch { get; set; }
}
