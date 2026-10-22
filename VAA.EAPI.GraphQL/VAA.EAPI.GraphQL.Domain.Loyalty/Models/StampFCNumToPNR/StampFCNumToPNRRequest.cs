using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR
{
    public class StampFCNumToPNRRequest
    {
        public IEnumerable<FCNumToPNRDetails> FCStampingDetails { get; set; }
    }
}
