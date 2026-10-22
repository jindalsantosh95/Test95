using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IRetrieveAmenities
    {
        Task<RetrieveAmenitiesResponse?> Retrieve(RetrieveAmenitiesRequest request);
    }
}
