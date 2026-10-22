using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class PassengerTicketDetails
    {
        public CurrencyAmount Amount { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
    }
}
