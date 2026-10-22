using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.StampFCNumberToPNR
{
    public class StampFCNumToPNRRequest
    {
        public IEnumerable<FCStampingDetails> FCStampingDetails { get; set; }
    }
}
