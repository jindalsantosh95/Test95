using MediatR;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.EnrolmentCustomer
{
    public class CustomerService<T> : ICustomerService<T> where T : CosmosDocument
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string PartitionKey { get; set; }
        private readonly CosmosClient _client;
        private readonly IMediator _mediator;

        public CustomerService(IMediator mediator)
        {
            _mediator = mediator;
        }
        #region Public Methods

        public async Task<T> CreateItemAsync(T item, string partitionKey, DocumentType cosmosCollectionEnum)
        {

            DatabaseName = ConfigReader.Get(Environment.SCVCustDB);
            if (cosmosCollectionEnum == DocumentType.Customer)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustColCntnr);
                await CreateCosmosDoc(item);
            }
            else if (ConfigReader.Get<bool>(Environment.IsSearchOnCustSearch))
            {

                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr);
                await CreateCosmosDoc(item);
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnrTest);
                item.PartitionKey = Constants.Application.CustomerSearchDocType;
                await CreateCosmosDoc(item);
            }
            else
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr);
                await CreateCosmosDoc(item);
            }
            return item;
        }

        private async Task CreateCosmosDoc(T item)
        {
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = CollectionName,
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustDBEndPoint),
                DatabaseName = DatabaseName,
                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Create
                }
            };
            await _mediator.Send(cosmosModifyRepoCommand);
        }

        public async Task<List<T>> GetItemAsync(DocumentType cosmosCollectionEnum, string sqlQueryText, string MemberId)
        {
            List<KeyValuePair<string, object>> parameters = null;
            List<T> currentResultSet = new();
            DatabaseName = ConfigReader.Get(Environment.SCVCustDB);
            if (cosmosCollectionEnum == DocumentType.Customer)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustColCntnr);
                parameters = new()
                {
                     new KeyValuePair<string, object>("DocType","Customer"),
                     new KeyValuePair<string, object>("LoyaltyMemberNumber", MemberId)
                };

            }
            else if (cosmosCollectionEnum == DocumentType.CustomerSearch && !ConfigReader.Get<bool>(Environment.IsSearchOnCustSearch))
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr);
                parameters = new()
                {
                    new KeyValuePair<string, object>("CustSearchDocType", Constants.Application.CustomerSearchDocType),
                    new KeyValuePair<string, object>("FCMemberNumber", MemberId)
                };
            }
            else
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnrTest);
                parameters = new()
                {
                    new KeyValuePair<string, object>("CustSearchDocType", Constants.Application.CustomerSearchDocType),
                    new KeyValuePair<string, object>("FCMemberNumber", MemberId)
                };
            }
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(CollectionName, ConfigReader.Get(Environment.SCVCustDBEndPoint), DatabaseName, parameters, sqlQueryText));

            if (customerSearchResponse != null && customerSearchResponse.Count > 0)
            {
                customerSearchResponse.ForEach(item => { currentResultSet.Add(Json.DeserializeObject<T>((JToken)item)); });
            }
            return currentResultSet;
        }

        public async Task<T> ReplaceItemAsync(T item, DocumentType cosmosCollectionEnum)
        {

            DatabaseName = ConfigReader.Get(Environment.SCVCustDB);
            if (cosmosCollectionEnum == DocumentType.Customer)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustColCntnr);
                await ModifyCosmosDoc(item);
            }
            else if (ConfigReader.Get<bool>(Environment.IsSearchOnCustSearch))
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr);
                await ModifyCosmosDoc(item);                
            }
            else
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr);
                await ModifyCosmosDoc(item);
            }
            return item;
        }

        private async Task ModifyCosmosDoc(T item)
        {
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = CollectionName,
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustDBEndPoint),
                DatabaseName = DatabaseName,
                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Replace
                }
            };
            await _mediator.Send(cosmosModifyRepoCommand);
        }
        #endregion
    }
}
