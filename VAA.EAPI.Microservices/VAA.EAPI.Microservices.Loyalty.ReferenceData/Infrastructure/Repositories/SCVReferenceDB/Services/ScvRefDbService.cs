using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Services
{
    public class ScvRefDbService : IScvRefDbService
    {
        private readonly ScvReferenceDbContext _refDbContext;
        private readonly ICacheService _iCacheService;

        public ScvRefDbService(ScvReferenceDbContext refDbContext, ICacheService iCacheService)
        {
            _refDbContext = refDbContext ?? throw new ArgumentNullException(nameof(refDbContext));
            _refDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            _iCacheService = iCacheService ?? throw new ArgumentNullException(nameof(iCacheService));
        }

        public async Task<IEnumerable<PartnerProfile>> GetPartnerCodeAsync()
        {
            IEnumerable<PartnerProfile> partnerCodeData = await _iCacheService.GetCacheValue<IEnumerable<PartnerProfile>>(Constants.Application.PartnerCodeCacheKey);

            if (partnerCodeData == null)
            {
                partnerCodeData = await _refDbContext.PartnerProfile.ToListAsync();
                await _iCacheService.SetCacheValue(Constants.Application.PartnerCodeCacheKey, partnerCodeData, true);
            }

            return partnerCodeData;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            IEnumerable<Product> productData = await _iCacheService.GetCacheValue<IEnumerable<Product>>(Constants.Application.ProductCacheKey);

            if (productData == null)
            {
                productData = await _refDbContext.Product.ToListAsync();
                await _iCacheService.SetCacheValue(Constants.Application.ProductCacheKey, productData, true);
            }

            return productData;
        }
    }
}
