using MediatR;
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
    public class CustomerTransactionService<T> : ICustomerTransactionService<T> where T : CosmosDocument
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string PartitionKey { get; set; }
        #region Private Variables and Methods       
        private readonly IMediator _mediator;


        public CustomerTransactionService(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion

        #region Public Methods

        public async Task<T> CreateItemAsync(T item, string partitionKey, DocumentType cosmosCollectionEnum)
        {

            DatabaseName = ConfigReader.Get(Environment.SCVCustTxnDB);
            if (cosmosCollectionEnum == DocumentType.CustomerProfile)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBCustPrflCntnr);
            }
            else if (cosmosCollectionEnum == DocumentType.FlyingClubMember)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBMmbrPrfleContnr);
            }
            else if (cosmosCollectionEnum == DocumentType.MembershipChange)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBMmbrshpChgContnr);
            }
            else if (cosmosCollectionEnum == DocumentType.PartnerMembership)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBPartnerMemContnr);
            }
            else if (cosmosCollectionEnum == DocumentType.Award)
            {
                DatabaseName = ConfigReader.Get(Environment.TransactionDB);
                CollectionName = ConfigReader.Get(Environment.TransactionDBCollection);
            }
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = CollectionName,
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustTxnDBEndPoint),
                DatabaseName = DatabaseName,
                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Create
                }
            };
            await _mediator.Send(cosmosModifyRepoCommand);
            return item;

        }
        public async Task<List<T>> GetItemAsync(DocumentType cosmosCollectionEnum, string sqlQueryText, string customerId)
        {
            List<KeyValuePair<string, object>> parameters = null;
            List<T> currentResultSet = new List<T>();
            if (cosmosCollectionEnum == DocumentType.Award)
            {
                DatabaseName = ConfigReader.Get(Environment.TransactionDB);
                CollectionName = ConfigReader.Get(Environment.TransactionDBCollection);
            }
            parameters = new()
            {
                new KeyValuePair<string, object>("DocType", "Award"),
                new KeyValuePair<string, object>("CustomerId", customerId),
                new KeyValuePair<string, object>("ProductId", ConfigReader.Get(Environment.ProductIdTopMember))
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(CollectionName, ConfigReader.Get(Environment.SCVCustTxnDBEndPoint), DatabaseName, parameters, sqlQueryText));

            if (customerSearchResponse != null && customerSearchResponse.Count > 0)
            {
                customerSearchResponse.ForEach(item =>
                {
                    currentResultSet.Add(Json.DeserializeObject<T>((JToken)item));
                });
            }
            return currentResultSet;
        }
        public async Task PatchItemAsync(string id, string partitionKey)
        {
            DatabaseName = ConfigReader.Get(Environment.TransactionDB);
            CollectionName = ConfigReader.Get(Environment.TransactionDBCollection);
            var updateEvent = new CosmosDocumentUpdateEvent()
            {
                Document = new CosmosDocument() { Id = id, PartitionKey = partitionKey },
                ModificationType = CosmosModificationType.Update
            };
            updateEvent.UpdateField(Constants.Application.AwardStartDateField, System.DateTime.UtcNow);
            updateEvent.UpdateField(Constants.Application.AwardExpiryDateField, string.Empty);

            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = CollectionName,
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustTxnDBEndPoint),
                DatabaseName = DatabaseName,
                Event = updateEvent

            };
            await _mediator.Send(cosmosModifyRepoCommand);

        }

        public async Task<T> ReplaceItemAsync(T item, string partitionKey, DocumentType cosmosCollectionEnum)
        {

            DatabaseName = ConfigReader.Get(Environment.SCVCustTxnDB);
            if (cosmosCollectionEnum == DocumentType.CustomerProfile)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBCustPrflCntnr);
            }
            else if (cosmosCollectionEnum == DocumentType.FlyingClubMember)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBMmbrPrfleContnr);
            }
            else if (cosmosCollectionEnum == DocumentType.MembershipChange)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBMmbrshpChgContnr);
            }
            else if (cosmosCollectionEnum == DocumentType.PartnerMembership)
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustTxnDBPartnerMemContnr);
            }
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = CollectionName,
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustTxnDBEndPoint),
                DatabaseName = DatabaseName,
                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Replace
                }
            };
            await _mediator.Send(cosmosModifyRepoCommand);
            return item;

        }
        #endregion
    }
}
