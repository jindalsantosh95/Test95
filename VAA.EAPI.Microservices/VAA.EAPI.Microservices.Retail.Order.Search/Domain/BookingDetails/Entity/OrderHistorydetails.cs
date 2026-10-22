using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class OrderHistorydetails
    {
        public List<HistoryActionItem> HistoryActionItem { get; set; }
        public List<HistoryCreditOrSignature> HistoryCreditOrSignature { get; set; }
        public HistoryPnrCreation HistoryPnrCreation { get; set; }
        public List<OtherServiceInformation> OtherServiceInformation { get; set; }
        public List<Remark> Remarks { get; set; }
    }
}
