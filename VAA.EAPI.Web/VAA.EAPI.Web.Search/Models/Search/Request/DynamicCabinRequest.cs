using MediatR;
using VAA.EAPI.Web.Search.Models.Amenity;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class DynamicCabinRequest : IRequest<CabinAmenity>
    {
        public string OfferId { get; set; }
        public string PassengerCounts { get; set; }
    }
}
