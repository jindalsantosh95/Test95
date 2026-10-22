using CSharpFunctionalExtensions;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAmexUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAmexUpdate
{
    public class CampaignAmexUpdateRequestCommandHandler(ITableStorageService<CampaignUniqueIdTableEntity> tableStorageService) : ICampaignAmexUpdateRequestCommand
    {
        private readonly ITableStorageService<CampaignUniqueIdTableEntity> _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));

        public async Task<Result<Response<string>>> Handle(CampaginAmexUpdateRequest campaginAmexUpdateRequest, CancellationToken cancellationToken)
        {
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _tableStorageService.TableName = Constants.Application.CampaignUniqueIdTableName;

            try
            {
                if (campaginAmexUpdateRequest == null) throw new ArgumentNullException(nameof(campaginAmexUpdateRequest));
                if (string.IsNullOrWhiteSpace(campaginAmexUpdateRequest?.AmexUniqueId))
                    throw new ArgumentNullException(nameof(campaginAmexUpdateRequest.AmexUniqueId));
                if (string.IsNullOrWhiteSpace(campaginAmexUpdateRequest?.FCnumber))
                    throw new ArgumentNullException(nameof(campaginAmexUpdateRequest.FCnumber));

                var tableDetails = await _tableStorageService.RetrieveAsync(Constants.Application.CampaignAmexPartitionKey, campaginAmexUpdateRequest.AmexUniqueId.ToUpper());

                if (tableDetails != null)
                {
                    await _tableStorageService.InsertOrMergeAsync(CampiganAmexUpdateTableEntityMapper.UpdateCampaignUniqueIdTableEntities(campaginAmexUpdateRequest));

                    return new Response<string>()
                    {
                        Status = Status.OK,
                        Value = "Amex PCM Campaign updated successfully",
                        Errors=null
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Status = Status.Error,
                        Value = "Amex PCM Campaign UniqueId not present in table",
                        Errors = new List<ErrorDetails> { new ErrorDetails { Code = "404", Message = "Not Found" } }

                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
