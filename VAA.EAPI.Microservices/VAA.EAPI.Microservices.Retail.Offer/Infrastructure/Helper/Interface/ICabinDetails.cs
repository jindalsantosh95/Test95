using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface ICabinDetails
    {
        public List<Cabins> GetCabinDetails();
    }
}
