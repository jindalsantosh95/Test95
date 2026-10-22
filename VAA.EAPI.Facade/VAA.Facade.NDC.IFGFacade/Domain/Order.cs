using System.Collections.Generic;

namespace VAA.Facade.NDC.IFGFacade.Domain
{
    public class Order
    {
        public string TransactionCode { get; set; }
        public IEnumerable<FlightRecord> FlightRecords { get; set; }
        public IEnumerable<ReportedFormOfPayment> ReportedFormOfPayment { get; set; }
        public AdditionalInformation AdditionalInformation { get; set; }
        public IEnumerable<AdditionalSalesInformation> AdditionalSalesInformation { get; set; }
    }
}
