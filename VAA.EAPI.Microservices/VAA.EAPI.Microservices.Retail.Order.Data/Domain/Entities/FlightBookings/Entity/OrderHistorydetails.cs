

using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class OrderHistorydetails
    {
        public IEnumerable<HistoryActionItem> HistoryActionItem { get; set; }
        public IEnumerable<HistoryCreditOrSignature> HistoryCreditOrSignature { get; set; }
        public HistoryPnrCreation HistoryPnrCreation { get; set; }
        public IEnumerable<OtherServiceInformation> OtherServiceInformation { get; set; }
        public IEnumerable<Remark> Remarks { get; set; }
    }
}
