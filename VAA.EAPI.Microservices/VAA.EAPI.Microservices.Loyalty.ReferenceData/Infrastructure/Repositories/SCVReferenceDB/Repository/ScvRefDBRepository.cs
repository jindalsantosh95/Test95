using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Repository
{
    public class ScvRefDBRepository : IScvRefDBRepository
    {
        private readonly IScvRefDbService _scvRefDBServices;

        public ScvRefDBRepository(IScvRefDbService scvRefDBServices)
        {
            _scvRefDBServices = scvRefDBServices ?? throw new ArgumentNullException(nameof(scvRefDBServices));
        }

        public async Task<IEnumerable<PartnerProfileDetails>> GetPartnerCodeDetails()
        {
            return PartnerCodeMapper.MapPartnerCodeData(await _scvRefDBServices.GetPartnerCodeAsync());
        }

        public async Task<IEnumerable<ProductDetails>> GetProductsDetails()
        {
            return ProductMapper.MapProductData(await _scvRefDBServices.GetProductsAsync());
        }
    }
}
