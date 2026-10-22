using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Upgrade
    {
        public string FareFamilyType { get; set; }
        public Price Price { get; set; }
        public Content Content { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public string Id { get; set; }
        public string FareId { get; set; }
        public string PromoCodeApplied { get; set; }
    }
}