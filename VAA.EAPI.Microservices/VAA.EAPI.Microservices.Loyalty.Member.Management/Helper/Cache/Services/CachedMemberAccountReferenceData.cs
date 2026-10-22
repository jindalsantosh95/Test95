using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Services
{
    public class CachedMemberAccountReferenceData : ICachedMemberAccountReferenceData
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBServices _commonRefDBServices;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public CachedMemberAccountReferenceData(IMemoryCache cache, ICommonRefDBServices commonRefDBServices)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDBServices = commonRefDBServices ?? throw new ArgumentNullException(nameof(commonRefDBServices));
        }

        public async Task<Result<Response<MemberAccountReferenceData>>> GetCachedMemberAccountReferenceData()
        {
            Result<Response<MemberAccountReferenceData>> memberRefenceData;

            //Using SemaphoreSlim here, as lock doesn't allow async calls
            await semaphoreSlim.WaitAsync();

            try
            {
                _cache.TryGetValue(Constants.Application.MemberAccountReferenceDatasCacheKey, out memberRefenceData);
                if (memberRefenceData.Value == null)
                {
                    TimeSpan CommonCachingSlidingExpiration = TimeSpan.Parse(ConfigReader.Get(Constants.Environment.CommonCachingSlidingExpiration));
                    TimeSpan CommonCachingAbsoluteExpiration = TimeSpan.Parse(ConfigReader.Get(Constants.Environment.CommonCachingAbsoluteExpiration));
                    memberRefenceData = await _commonRefDBServices.GetMemberAccountReferenceData();
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(CommonCachingSlidingExpiration).SetAbsoluteExpiration(CommonCachingAbsoluteExpiration);
                    _cache.Set(Constants.Application.MemberAccountReferenceDatasCacheKey, memberRefenceData, cacheEntryOptions);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return memberRefenceData;
        }
    }
}
