using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model
{
    public class Parties
    {
        public IEnumerable<Participant> Participants { get; set; }
        public CorporateAccount CorporateAccount { get; set; }
    }
}
