using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface
{
    public interface IScvRefDBRepository
    {
        public Task<IEnumerable<PartnerProfileDetails>> GetPartnerCodeDetails();
        public Task<IEnumerable<ProductDetails>> GetProductsDetails();
    }
}
