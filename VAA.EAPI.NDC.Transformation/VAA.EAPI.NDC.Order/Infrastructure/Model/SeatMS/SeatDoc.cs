using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatDoc
    {
        public SeatOffers offer { get; set; }
        public string id { get; set; }
        public string docType { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
