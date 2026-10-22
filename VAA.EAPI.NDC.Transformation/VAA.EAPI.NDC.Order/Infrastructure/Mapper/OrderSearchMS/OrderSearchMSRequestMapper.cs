using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS
{
    public class OrderSearchMSRequestMapper
    {
        public static OrderSearchRequest Mapper(OrderDocument orderDocument, bool historyFlag = false, bool isCancelledAndRebooked = false)
        {
            return new ()
            {
                SearchFilterCriteria = new()
                {
                    BookingReferenceCriteria = new()
                    {
                        BookingReference = new()
                        {
                            RLOC = orderDocument.BookingReference.RLOC
                        }
                    },
                    OrderReferenceCriteria = new()
                    {
                        OrderID = orderDocument.OrderNumber.OrderId
                    },
                    Parties = orderDocument.Parties,
                    CustomerDetails = orderDocument.DataList.CustomerDetails,
                    SegmentDetails = orderDocument.DataList.SegmentDetails,
                    HistoryFlag = historyFlag,
                    IsCancelledAndRebooked = isCancelledAndRebooked
                }
            };
        }
    }
}
