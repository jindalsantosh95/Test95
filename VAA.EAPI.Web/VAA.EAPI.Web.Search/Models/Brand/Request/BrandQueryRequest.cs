using VAA.EAPI.Web.Search.Models.Search.Request;

namespace VAA.EAPI.Web.Search.Models.Brand.Request
{
    public class BrandQueryRequest
    {
        public string OfferId { get; set; }
        public string PassengerCounts { get; set; }

        public static explicit operator BrandQueryRequest(DynamicCabinRequest source)
        {
            return new()
            {
                OfferId = source.OfferId,
                PassengerCounts = source.PassengerCounts
            };
        }
    }
}
