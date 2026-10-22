using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Common.Serialisation;
using System.Linq;
namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class OrdersRepoQueryHandler : IRequestHandler<OrdersRepoQuery, IEnumerable<OrderDoc>>
    {
        private readonly ILogger<OrdersRepoQueryHandler> _logger;
        private readonly IMediator _mediator;

        public OrdersRepoQueryHandler(IMediator mediator, ILogger<OrdersRepoQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<OrderDoc>> Handle(OrdersRepoQuery request, CancellationToken cancellationToken)
        {
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderSCVDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.OrderDB),
                CollectionName = ConfigReader.Get(Constants.Environment.OrdersCollection),
                SqlQuery = Constants.Application.GetOrderDocsQuery,
                QueryParams = new()
                {
                    new KeyValuePair<string, object>("OrderID", request.Orders),
                   
                }
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
                return Json.Deserialize<List<OrderDoc>>(Json.Serialize(cosmosDocs));
            }
            else
            {
                return new List<OrderDoc>(); ;
            }
        }
    }
}
