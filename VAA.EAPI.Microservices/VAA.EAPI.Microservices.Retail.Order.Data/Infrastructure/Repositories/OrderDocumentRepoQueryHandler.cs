using MediatR;
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
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class OrderDocumentRepoQueryHandler : IRequestHandler<OrderDocumentRepoQuery, List<OrderDocument>>
    {
        private readonly IMediator _mediator;

        public OrderDocumentRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<List<OrderDocument>> Handle(OrderDocumentRepoQuery request, CancellationToken cancellationToken)
        {
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.NDCOrderCollection),
                SqlQuery = Constants.Application.NDCStoreQuery,
                QueryParams =
                [
                    new KeyValuePair<string, object>("PartitionKey", request.OrderId),
                    new KeyValuePair<string, object>("Id", request.OrderId)
                ]
            };

            List<object> cosmosDocs;
            try
            {
                cosmosDocs = await _mediator.Send(query, CancellationToken.None);
            }
            catch
            {
                var error = new Response<string>()
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                }; 
                
                throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }

            if (cosmosDocs.Any())
            {
                return Json.Deserialize<List<OrderDocument>>(Json.Serialize(cosmosDocs));
            }
            else
            {
                //Fetch by PNR
                return await FetchByPnr(request.OrderId);
            }
        }

        private async Task<List<OrderDocument>> FetchByPnr(string pnr)
        {
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.NDCOrderCollection),
                SqlQuery = Constants.Application.NDCStoreQueryWithPnr,
                QueryParams =
                [
                    new KeyValuePair<string, object>("PNR", pnr)
                ]
            };
            List<object> cosmosDocs;
            try
            {
                cosmosDocs = await _mediator.Send(query, CancellationToken.None);
            }
            catch
            {
                var error = new Response<string>()
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
                throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }
            if (cosmosDocs.Any())
            {
                return Json.Deserialize<List<OrderDocument>>(Json.Serialize(cosmosDocs));
            }
            else
            {
                return [];
            }
        }
    }
}
