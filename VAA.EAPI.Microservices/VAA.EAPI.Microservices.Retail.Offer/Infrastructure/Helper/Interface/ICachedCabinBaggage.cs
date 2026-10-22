using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface ICachedCabinBaggage
    {
        Task<IEnumerable<CabinBaggageEntity>> GetCachedCabinBaggageInformation(string allowance);
    }
}
