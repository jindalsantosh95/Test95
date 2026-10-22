using Microsoft.Extensions.Caching.Memory;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service
{
    public class PartnerDataService : IPartnerDataService
    {
        private readonly ITableStorageService<PartnerDetails> _tableStorage;
        private readonly IMemoryCache _cache;

        public PartnerDataService(ITableStorageService<PartnerDetails> tableStorage, IMemoryCache cache)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _tableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceMicroServiceDBKey);
            _tableStorage.TableName = Constants.Application.ReferenceTableName;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<IReadOnlyCollection<Partner>> GetSkyTeamPartnersAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_cache.TryGetValue(Constants.Application.SkyTeamPartnersCacheKey, out IReadOnlyCollection<Partner>? cachedSkyTeamPartners) && cachedSkyTeamPartners is not null)
            {
                return cachedSkyTeamPartners;
            }

            var partnerList = await _tableStorage.RetrieveItemsAsync(default) ?? [];
            cancellationToken.ThrowIfCancellationRequested();

            var skyTeamPartners = partnerList
                .Where(partner => partner.PartitionKey == Constants.Application.PartnerPartitionKey && partner.SkyTeamPartnerFlag)
                .Select(partner => new Partner
                {
                    PartnerId = partner.RowKey,
                    PartnerCode = partner.PartnerCode,
                    PartnerName = partner.PartnerName,
                    PartnerType = partner.PartnerType,
                    PartnerSubType = partner.PartnerSubType,
                    SkyTeamPartnerFlag = partner.SkyTeamPartnerFlag
                })
                .ToList();

            _cache.Set(Constants.Application.SkyTeamPartnersCacheKey, skyTeamPartners, TimeSpan.FromHours(int.Parse(ConfigReader.Get(Constants.Environment.SkyTeamPartnerCacheExpiryHours))));

            return skyTeamPartners;
        }
    }
}
