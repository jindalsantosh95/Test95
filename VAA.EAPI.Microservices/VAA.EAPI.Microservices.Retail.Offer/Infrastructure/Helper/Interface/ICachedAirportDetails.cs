using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface ICachedAirportDetails
    {
        Task<AirportDetailsTableEntity> GetCachedAirportDetails(string rowKey);
    }
}
