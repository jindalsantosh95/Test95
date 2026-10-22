using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderSearchMS
{
    public class OrderSearchMSRequestMapper
    {
        public static OrderSearchRequest MapOrderSearchRequest(OrderDocument orderDocument)
        {
            return new()
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
                    SegmentDetails = orderDocument.DataList.SegmentDetails
                }
            };
        }
    }
}
