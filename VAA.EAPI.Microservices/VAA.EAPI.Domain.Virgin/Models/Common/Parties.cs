using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class Parties
    {
        public IEnumerable<Participant> Participants { get; set; }
        public CorporateAccount CorporateAccount { get; set; }

        public static explicit operator Parties(JObject source)
        {
            return new Parties
            {
                Participants = source["Participants"]?.Select(a => (Participant)a),
                CorporateAccount = source["CorporateAccount"] != null ? (CorporateAccount)source["CorporateAccount"] : null
            };
        }
    }
}