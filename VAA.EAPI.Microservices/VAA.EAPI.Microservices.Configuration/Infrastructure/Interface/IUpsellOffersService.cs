using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface IUpsellOffersService
    {
        Task<IEnumerable<UpsellOffersDbEntity>> GetUpsellOffers();
    }
}
