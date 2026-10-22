using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class SeatDetail
    {
        public string SeatNumber { get; set; }
        public string ElectronicMiscellaneousDocumentNumber { get; set; }
        public string CustomerRefId { get; set; }
        public CurrencyAmount Amount { get; set; }

    }
}
