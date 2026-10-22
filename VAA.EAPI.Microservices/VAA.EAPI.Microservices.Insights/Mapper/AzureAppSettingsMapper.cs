using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Mapper
{
    public static class AzureAppSettingsMapper
    {
        public static IEnumerable<AzureAppSettingViewModel> Map(
            IEnumerable<AzureAppSetting> appSettings,
            IEnumerable<AzureKeyVaultReference> keyVaultReferences)
        {
            var resolverStatuses = (keyVaultReferences ?? Enumerable.Empty<AzureKeyVaultReference>())
                .Where(reference => reference?.Name != null)
                .GroupBy(reference => reference.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().Properties?.Status,
                    StringComparer.OrdinalIgnoreCase);

            return (appSettings ?? Enumerable.Empty<AzureAppSetting>()).Select(appSetting =>
            {
                string resolverStatus = null;
                if (appSetting?.Name != null)
                {
                    resolverStatuses.TryGetValue(appSetting.Name, out resolverStatus);
                }

                return new AzureAppSettingViewModel
                {
                    Name = appSetting?.Name,
                    Value = appSetting?.Value,
                    ResolverStatus = resolverStatus
                };
            });
        }
    }
}
