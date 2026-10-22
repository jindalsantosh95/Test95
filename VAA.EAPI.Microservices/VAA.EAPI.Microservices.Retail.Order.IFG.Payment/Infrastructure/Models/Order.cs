using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class Order
    {
        public string TransactionCode { get; set; }
        public IEnumerable<ReportedFormOfPayment> ReportedFormOfPayment { get; set; }
        public IEnumerable<FlightRecord> FlightRecords { get; set; }
        public AdditionalInformation AdditionalInformation { get; set; }
        public IEnumerable<AdditionalSalesInformation> AdditionalSalesInformation { get; set; }
        public MonetaryAmounts MonetaryAmounts { get; set; }
        public BookingInformation BookingInformation { get; set; }
        public IEnumerable<object> AdditionalTaxInformationRecords { get; set; }
    }
}