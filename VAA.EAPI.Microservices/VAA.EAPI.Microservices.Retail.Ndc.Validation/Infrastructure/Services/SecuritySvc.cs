using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;
using Environment = VAA.EAPI.Microservices.Retail.Ndc.Validation.Constants.Environment;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services
{
    public class SecuritySvc : ISecuritySvc
    {
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private static string bearerToken = null;

        private readonly IHttpConnector _apiConnector;

        public SecuritySvc(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<SubscriptionResponse> RetrieveSubscription(string agencyID, string environment)
        {
            string url = ConfigReader.Get(Environment.SecuritySvcEndPoint) + $"{environment}/{agencyID}";
            int retry = 0;
            Dictionary<string, string> headers = new()
            {
                { Environment.OcpKeyHeader, ConfigReader.Get(Environment.SecuritySvcOCPKey) }
            };

            return await _apiConnector.GetAsync<SubscriptionResponse>(uri: url, headers: headers, null);
        }
    }
}
