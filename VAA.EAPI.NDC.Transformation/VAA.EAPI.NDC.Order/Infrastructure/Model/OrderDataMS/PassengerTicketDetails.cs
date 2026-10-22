using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS
{
    public class PassengerTicketDetails
    {
        public CurrencyAmount Amount { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
    }
}
