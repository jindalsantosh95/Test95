using MediatR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Constants;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Interfaces;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Services
{
    public class CustomerService<T> : ICustomerService<T> where T : CosmosDocument
    {
        private readonly IMediator _mediator;

        public CustomerService(IMediator mediator)
        {
            _mediator = mediator;
        }
        #region Public Methods       
        public async Task<T> InsertItemAsync(T item, DocumentType cosmosCollectionEnum)
        {
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustSearchDBCollection),
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustSearchDBEndPoint),
                DatabaseName = ConfigReader.Get(Environment.SCVCustSearchDBName),
                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Create
                }
            };
            await _mediator.Send(cosmosModifyRepoCommand);
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<bool> Exists(string memberId, string docType)
        {
            List<KeyValuePair<string, object>> parameters = null;
            List<T> currentResultSet = new();

            parameters = new()
                {
                    new KeyValuePair<string, object>("DocType",docType),
                    new KeyValuePair<string, object>("LoyaltyMemberNumber", memberId)
                };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustSearchDBCollection),
                                                              ConfigReader.Get(Environment.SCVCustSearchDBEndPoint),
                                                              ConfigReader.Get(Environment.SCVCustSearchDBName),
                                                              parameters,
                                                              Constants.Application.SqlQuery));

            return customerSearchResponse != null && customerSearchResponse.Any();
        }

        public async Task<T> GetItemAsync(string memberId, string docType)
        {
            List<KeyValuePair<string, object>> parameters = null;
            List<T> currentResultSet = new();

            parameters = new()
                {
                    new KeyValuePair<string, object>("DocType",docType),
                    new KeyValuePair<string, object>("LoyaltyMemberNumber", memberId)
                };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustSearchDBCollection),
                                                              ConfigReader.Get(Environment.SCVCustSearchDBEndPoint),
                                                              ConfigReader.Get(Environment.SCVCustSearchDBName),
                                                              parameters,
                                                              Constants.Application.SqlQuery));

            if (customerSearchResponse.Count > 0)
                return Json.DeserializeObject<T>((JToken)customerSearchResponse.FirstOrDefault());
            
            return null;
        }

        public async Task<T> PatchItemAsync(T item, DocumentType cosmosCollectionEnum)
        {
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = ConfigReader.Get(Environment.SCVCustSearchDBCollection),
                DatabaseEndpoint = ConfigReader.Get(Environment.SCVCustSearchDBEndPoint),
                DatabaseName = ConfigReader.Get(Environment.SCVCustSearchDBName),
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
