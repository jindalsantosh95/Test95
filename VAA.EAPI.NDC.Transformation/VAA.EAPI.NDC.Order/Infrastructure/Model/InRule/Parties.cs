using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using SourceParties = VAA.EAPI.NDC.Order.Infrastructure.Model.Parties;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class Parties
    {
        public IEnumerable<Participant> Participants { get; set; }
        public CorporateAccount CorporateAccount { get; set; }

        public static explicit operator Parties(SourceParties source) =>
            source is null
                ? null
                : new Parties
                {
                    Participants = source.Participants?
                        .Where(participant => participant is not null)
                        .Select(participant => (Participant)participant),
                    CorporateAccount = source.CorporateAccount is null
                        ? null
                        : (CorporateAccount)source.CorporateAccount
                };
    }
}
