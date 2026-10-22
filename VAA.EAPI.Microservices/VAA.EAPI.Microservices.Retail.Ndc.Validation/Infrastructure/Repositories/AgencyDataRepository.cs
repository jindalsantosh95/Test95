using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;


namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories
{
    public class AgencyDataRepository : IAgencyDataRepository
    {
        private readonly IAgencyDataService _agencyDataService;
        private readonly IDistributedCacheService _distributedCacheService;


        public AgencyDataRepository(IAgencyDataService agencyDataService, IDistributedCacheService distributedCacheService)
        {
            _agencyDataService = agencyDataService;
            _distributedCacheService = distributedCacheService;
        }

        public async Task<List<AgencyData>> GetAgencyDetails(AgencyValidateQuery requestData)
        {
            var absoluteExpirationTime = ConfigReader.Get<TimeSpan>(Constants.Environment.AgencyAbsoluteExpirationInDay);

            var cacheKey = CommonHelper.GenerateCacheKey(requestData);

            List<AgencyData> agencyData = await _distributedCacheService.Get<List<AgencyData>>(cacheKey);

            if (agencyData == null || agencyData.Count == 0)
            {
                agencyData = await _agencyDataService.GetAgencyDetails(requestData);

                if (agencyData != null && agencyData.Count > 0)
                    await _distributedCacheService.Set(cacheKey, agencyData, false, TimeSpan.Zero, absoluteExpirationTime);
            }

            return agencyData;
        }
    }
}
