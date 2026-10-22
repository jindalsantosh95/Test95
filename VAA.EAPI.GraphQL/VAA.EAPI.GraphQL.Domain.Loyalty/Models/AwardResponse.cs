using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class AwardResponse
    {
        public IEnumerable<Award> Awards { get; set; }
    }
}
