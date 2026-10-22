using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper
{
    public static class GetCachedSecret
    {
        public static string GetSecretsManagedIdentity(string secretName, string cachedSecretValue)
        {
            return string.IsNullOrEmpty(cachedSecretValue) ? Environment.GetEnvironmentVariable(secretName) : cachedSecretValue;
        }
    }
}
