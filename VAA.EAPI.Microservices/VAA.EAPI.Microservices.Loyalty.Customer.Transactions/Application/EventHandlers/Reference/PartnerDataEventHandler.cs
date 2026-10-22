using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference
{
    public class PartnerDataEventHandler : IPartnerDataEventHandler
    {
        private readonly ITableStorageService<PartnerDetails> _tableStorage;

        public PartnerDataEventHandler(ITableStorageService<PartnerDetails> tableStorage)
        {
            _tableStorage = tableStorage;
            _tableStorage.ConnectionString = ConfigReader.Get(Constants.EnvironmentSettings.MicroServiceDBKey);
            _tableStorage.TableName = Constants.Application.ReferenceTableName;
        }

        public async Task<string> HandleAsync(PartnerDataMessage message)
        {
            try
            {

                if (message?.Data?.Partners == null)
                    throw new ArgumentNullException(nameof(message.Data.Partners));

                if (!message.Data.Partners.Any())
                    throw new ArgumentException("Partners collection cannot be empty.", nameof(message.Data.Partners));
                
                foreach (var partner in message.Data.Partners)
                {
                    if (string.IsNullOrWhiteSpace(partner.PartnerId))
                        continue;

                    var entity = new PartnerDetails
                    {
                        PartitionKey = Constants.Application.PartnerPartitionKey,
                        RowKey = partner.PartnerId,
                        PartnerName = partner.PartnerName,
                        PartnerCode = partner.PartnerCode,
                        PartnerType = partner.PartnerType,
                        PartnerSubType = partner.PartnerSubType,
                        SkyTeamPartnerFlag = Convert.ToBoolean(partner.SkyTeamPartnerFlag)
                    };

                    await _tableStorage.InsertOrMergeAsync(entity);

                }

                return "Partner data processed successfully.";

            }

            catch (Exception ex)
            {
                throw new Exception($"Error processing Partner data: {ex.Message}", ex);
            }
        }
    }
}

