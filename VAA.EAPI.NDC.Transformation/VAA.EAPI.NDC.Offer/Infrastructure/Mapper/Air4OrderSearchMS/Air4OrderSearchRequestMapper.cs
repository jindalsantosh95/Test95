using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Mapper.Air4OrderSearchMS
{
    public class Air4OrderSearchRequestMapper
    {
        public static OrderSearchRequest MapOrderSearchRequest(string RLOC)
        {
            return new OrderSearchRequest()
            {
                SearchFilterCriteria = new SearchFilterCriteria()
                {
                    BookingReferenceCriteria = new BookingReferenceCriteria()
                    {
                        BookingReference = new()
                        {
                            RLOC = RLOC
                        }
                    }
                }
            };
        }
    }
}
