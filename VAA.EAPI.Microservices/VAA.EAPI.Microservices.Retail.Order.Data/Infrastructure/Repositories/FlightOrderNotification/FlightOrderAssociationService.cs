using MediatR;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Model.FlightCollection;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification
{
    public class FlightOrderAssociationService : IFlightOrderAssociationService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlightOrderAssociationService> _logger;

        public FlightOrderAssociationService(IMediator mediator, ILogger<FlightOrderAssociationService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<string>> GetOrderIdsAsync(DateTime flightDate, string flightNumber, CancellationToken cancellationToken)
        {
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderSCVDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.OrderDB),
                CollectionName = ConfigReader.Get(Constants.Environment.FlightCollection),
                SqlQuery = Constants.Application.GetOrderIdsQuery,
                QueryParams = new()
                {
                    new KeyValuePair<string, object>("PartitionKey", flightDate.ToString("yyyyMM")),
                    new KeyValuePair<string, object>("FlightDate", flightDate.ToString("yyyy-MM-dd")),
                    new KeyValuePair<string, object>("FlightNumber", flightNumber.PadLeft(4, '0'))
                }

            };

            IEnumerable<object> cosmosDocs;
            try
            {
                cosmosDocs = await _mediator.Send(query, CancellationToken.None);

                if (cosmosDocs == null || !cosmosDocs.Any())
                    return Enumerable.Empty<string>();

                return cosmosDocs
                    .Select(doc => Json.Deserialize<FlightCollection>(Json.Serialize(doc))?.OrderID?.ToString())
                    .Where(id => !string.IsNullOrWhiteSpace(id));
            }
            catch
            {
                var error = new Response<string>
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(
                        ErrorCodes.CosmosErorrCode,
                        Guid.NewGuid().ToString(),
                        EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };

                throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }
        }
    }

}
