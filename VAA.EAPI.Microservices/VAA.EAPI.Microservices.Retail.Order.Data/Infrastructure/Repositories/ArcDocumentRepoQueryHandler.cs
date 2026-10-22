using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class ArcDocumentRepoQueryHandler : IArcDocumentRepoQuery
    {
        private readonly CosmosReaderRepoQueryHandler  _cosmosReaderRepoQueryHandler;

        public ArcDocumentRepoQueryHandler(CosmosReaderRepoQueryHandler cosmosReaderRepoQueryHandler)
        {
            _cosmosReaderRepoQueryHandler = cosmosReaderRepoQueryHandler ?? throw new ArgumentNullException(nameof(cosmosReaderRepoQueryHandler));
        }
        public async Task<ARCDocument> Handle( string orderId, CancellationToken cancellationToken)
        {
            var query = new CosmosReaderRepoQuery
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.NDCOrderCollection),

                SqlQuery = Constants.Application.NDCStoreQuery,
                QueryParams =
                [
                    new KeyValuePair<string, object>("PartitionKey", orderId),
                    new KeyValuePair<string, object>("Id", orderId)
                ]
            };

            IEnumerable<object> cosmosDocs;
            try
            {
                cosmosDocs = await _cosmosReaderRepoQueryHandler.Handle (query, cancellationToken);
            }
            catch
            {
                var error = new Response<string>
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException( ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(),EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };

                throw new CosmosDBException(Json.Serialize(error.Errors));
            }

            if (!cosmosDocs.Any())
                return null;

            var doc = cosmosDocs.First();

            return Json.Deserialize<ARCDocument>(Json.Serialize(doc)
            );
        }
    }
}