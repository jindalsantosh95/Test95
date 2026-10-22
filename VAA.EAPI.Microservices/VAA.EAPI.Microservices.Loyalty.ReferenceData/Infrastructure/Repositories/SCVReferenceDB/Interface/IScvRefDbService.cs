using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface
{
    public interface IScvRefDbService
    {
        public Task<IEnumerable<PartnerProfile>> GetPartnerCodeAsync();
        public Task<IEnumerable<Product>> GetProductsAsync();
    }
}
