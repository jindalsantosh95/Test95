using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface
{
    public interface ICachedAmenities
    {
        Task<List<AmenitiesTableEntity>> GetAmenitiesInformation(string codeshare);
    }
}
