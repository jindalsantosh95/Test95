using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.OfferMS
{
    public class Parties
    {
        public IEnumerable<Participant> Participants { get; set; }
        public CorporateAccount CorporateAccount { get; set; }
    }
}
