using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories
{

    public class SecuritySvcRetrieveQueryHandler : IRequestHandler<SecuritySvcRetrieveQuery, SubscriptionResponse>
    {
        private readonly ISecuritySvc _securitySvc;
        private readonly IDistributedCacheService _distributedCacheService;

        public SecuritySvcRetrieveQueryHandler(ISecuritySvc securityService, IDistributedCacheService distributedCacheService)
        {
            _securitySvc = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _distributedCacheService = distributedCacheService;
        }

        public async Task<SubscriptionResponse> Handle(SecuritySvcRetrieveQuery request, CancellationToken cancellationToken)
        {
            var slidingExpirationTime = ConfigReader.Get<TimeSpan>(Constants.Environment.SubscriptionCachingSlidingExpirationInDay);
            var absoluteExpirationTime = ConfigReader.Get<TimeSpan>(Constants.Environment.SubscriptionCachingAbsoluteExpirationInDay);
            var subscriptionCacheKey = GenerateSubscriptionCacheKey(request);

            SubscriptionResponse subscriptionResponse = await _distributedCacheService.Get<SubscriptionResponse>(subscriptionCacheKey);

            if (subscriptionResponse == null)
            {
                subscriptionResponse = await _securitySvc.RetrieveSubscription(request.AgencyID, request.Environment);

                if (subscriptionResponse != null && subscriptionResponse.SubscriptionKeys != null && subscriptionResponse.SubscriptionKeys.FirstOrDefault().Key != null)
                    await _distributedCacheService.Set(subscriptionCacheKey, subscriptionResponse, true, slidingExpirationTime, absoluteExpirationTime);
            }
            return subscriptionResponse;
        }

        private string GenerateSubscriptionCacheKey(SecuritySvcRetrieveQuery request)
        {
            var agencyID = request.AgencyID;
            var environment = request.Environment;

            return $"{agencyID}-{environment}";
        }
    }
}
