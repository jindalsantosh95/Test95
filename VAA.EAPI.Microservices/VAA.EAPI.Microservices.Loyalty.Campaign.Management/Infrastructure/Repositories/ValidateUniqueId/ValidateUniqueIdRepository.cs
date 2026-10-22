using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using InfrastructureConstants = VAA.EAPI.Infrastructure.Constants.Constants;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId
{
    public class ValidateUniqueIdRepository : IValidateUniqueIdRepository
    {
        private readonly ITableStorageService<CampaignUniqueIdTableEntity> _tableStorageService;
        
        public ValidateUniqueIdRepository(ITableStorageService<CampaignUniqueIdTableEntity> tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(InfrastructureConstants.MicroServiceDBKey);
            _tableStorageService.TableName = VAA.EAPI.Microservices.Loyalty.Campaign.Management.Constants.Application.CampaignUniqueIdTableName;
        }
        
        public async Task<CampaignUniqueIdTableEntity> GetByUniqueIdAsync(string uniqueId)
        {
            return await _tableStorageService.RetrieveAsync(VAA.EAPI.Microservices.Loyalty.Campaign.Management.Constants.Application.CampaignUniqueIdPartitionKey, uniqueId);
        }
    }
}
