using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
    /// <summary>
    /// Helpers for caching ADO deployment status data in a distributed cache.
    /// </summary>
    public static class ADODeploymentStatusCache
    {
        private static readonly SemaphoreSlim CacheLock = new(1, 1);

        public static async Task<ADODeploymentCollection> GetAsync(IDistributedCacheService distributedCacheService)
        {
            ArgumentNullException.ThrowIfNull(distributedCacheService);

            await CacheLock.WaitAsync();
            try
            {
                return await GetUnlockedAsync(distributedCacheService);
            }
            finally
            {
                CacheLock.Release();
            }
        }

        public static async Task SaveAsync(IDistributedCacheService distributedCacheService, ADODeploymentCollection deploymentCollection)
        {
            ArgumentNullException.ThrowIfNull(distributedCacheService);
            ArgumentNullException.ThrowIfNull(deploymentCollection);

            await CacheLock.WaitAsync();
            try
            {
                await SaveUnlockedAsync(distributedCacheService, deploymentCollection);
            }
            finally
            {
                CacheLock.Release();
            }
        }

        public static async Task<ADODeploymentCollection> UpdateAsync(IDistributedCacheService distributedCacheService, Func<ADODeploymentCollection, bool> updateCollection)
        {
            ArgumentNullException.ThrowIfNull(distributedCacheService);
            ArgumentNullException.ThrowIfNull(updateCollection);

            await CacheLock.WaitAsync();
            try
            {
                var deploymentCollection = await GetUnlockedAsync(distributedCacheService) ?? new ADODeploymentCollection();
                if (updateCollection(deploymentCollection))
                {
                    await SaveUnlockedAsync(distributedCacheService, deploymentCollection);
                }

                return deploymentCollection;
            }
            finally
            {
                CacheLock.Release();
            }
        }

        public static async Task ResetAsync(IDistributedCacheService distributedCacheService)
        {
            ArgumentNullException.ThrowIfNull(distributedCacheService);

            await CacheLock.WaitAsync();
            try
            {
                await distributedCacheService.Remove(Constants.Application.DeploymentStatus);
            }
            finally
            {
                CacheLock.Release();
            }
        }

        private static async Task<ADODeploymentCollection> GetUnlockedAsync(IDistributedCacheService distributedCacheService)
        {
            return await distributedCacheService.Get<ADODeploymentCollection>(Constants.Application.DeploymentStatus);
        }

        private static async Task SaveUnlockedAsync(IDistributedCacheService distributedCacheService, ADODeploymentCollection deploymentCollection)
        {
            deploymentCollection.LastUpdated = DateTime.UtcNow;

            await distributedCacheService.Set(
                Constants.Application.DeploymentStatus,
                deploymentCollection,
                false,
                TimeSpan.Zero,
                TimeSpan.FromDays(30));
        }
    }
}
