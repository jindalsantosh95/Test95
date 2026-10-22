using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Domain.Exceptions;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightBookings
{
    public class CustomerOrderRepoQueryHandler : IRequestHandler<CustomerOrderRepoQuery, List<CustomerOrderAssociation>>
    {
        private readonly IMediator _mediator;
        private List<KeyValuePair<string,object>> Params;

        public CustomerOrderRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<List<CustomerOrderAssociation>> Handle(CustomerOrderRepoQuery request, CancellationToken cancellationToken)
        {
            string orderIdQuery = Constants.Application.OrderStoreQuery;
            Params = new List<KeyValuePair<string, object>>();
            if (!string.IsNullOrEmpty(request.CustomerID))
            {
                orderIdQuery += " AND c.CustomerID=@CustomerID";
                Params.Add(new KeyValuePair<string, object>("CustomerID", request.CustomerID));
            }
            if (!string.IsNullOrEmpty(request.CustomerEmail))
            {
                orderIdQuery += " AND c.CustomerEmail=@CustomerEmail";
                Params.Add(new KeyValuePair<string, object>("CustomerEmail", request.CustomerEmail));
              
            }
            if (!string.IsNullOrEmpty(request.CustomerPhone))
            {
                orderIdQuery += " AND c.CustomerPhone=@CustomerPhone";
                Params.Add(new KeyValuePair<string, object>("CustomerPhone", request.CustomerPhone));
            }
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.CustomerOrderAssociation),
                SqlQuery = orderIdQuery,
                QueryParams = Params
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
                return Json.Deserialize<List<CustomerOrderAssociation>>(Json.Serialize(cosmosDocs));
            }
            else
            {
                return new();
            }
        }
    }
}
