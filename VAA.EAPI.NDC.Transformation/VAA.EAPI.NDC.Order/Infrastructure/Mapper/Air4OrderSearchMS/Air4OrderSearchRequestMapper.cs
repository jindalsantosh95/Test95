using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.Air4OrderSearchMS
{
    public class Air4OrderSearchRequestMapper
    {
        public static OrderSearchRequest MapOrderSearchRequest(string airline , string RLOC)
        {
            return new OrderSearchRequest()
            {
                SearchFilterCriteria = new SearchFilterCriteria()
                {
                    BookingReferenceCriteria = new BookingReferenceCriteria()
                    {
                        BookingReference = new()
                        {
                            AirlineDesignator = airline,
                            RLOC = RLOC
                        }
                    }
                }
            };
        }
    }
}
