using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities
{
    public class UpdatePNRRequest
    {
        public IEnumerable<PNRDetailsforFCNumber> FCStampingDetails { get; set; }
    }
}
