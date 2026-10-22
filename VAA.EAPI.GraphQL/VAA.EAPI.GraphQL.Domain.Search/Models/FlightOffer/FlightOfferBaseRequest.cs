namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class FlightOfferBaseRequest
    {
        public Pos? Pos { get; set; }
        public Parties? Parties { get; set; }
        public FlightSearchRequest? FlightSearchRequest { get; set; }
        public LoyaltyMemberDetails? LoyaltyMember { get; set; }
        public IEnumerable<Customer>? CustomerDetails { get; set; }
        public string? SortRequest { get; set; }
        public FlightOfferRequestFilters? Filters { get; set; }
        public string? CurrencyCode { get; set; }
        public string? GetPassengerCounts()
        {
            if (CustomerDetails == null || CustomerDetails.Any() == false)
            {
                return null;
            }
            var numAdults = CustomerDetails.Count(c => c.Ptc == PtcEnums.PtcTypeEnum.ADT.ToString());
            var numTeens = CustomerDetails.Count(c => c.Ptc == PtcEnums.PtcTypeEnum.GBE.ToString());
            var numChildren = CustomerDetails.Count(c => c.Ptc == PtcEnums.PtcTypeEnum.CNN.ToString());
            var numInfants = CustomerDetails.Count(c => c.Ptc == PtcEnums.PtcTypeEnum.INF.ToString());
            return $"a{numAdults}t{numTeens}c{numChildren}i{numInfants}";
        }
    }
}