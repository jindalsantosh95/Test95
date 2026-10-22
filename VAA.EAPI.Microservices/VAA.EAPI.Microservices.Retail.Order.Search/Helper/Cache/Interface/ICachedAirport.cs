using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface
{
    public interface ICachedAirport
    {
        Task<AirportLocation> GetCachedAirport(string rowKey);
    }
}
