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
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public sealed class SvcOrderRepository(CosmosReaderServiceQueryHandler queryHandler) : ISvcOrderRepository, IScvBookingRepository
    {
        private readonly CosmosReaderServiceQueryHandler _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));

        public Task<IReadOnlyCollection<OrderDoc>> GetByPnrAsync(string pnr, CancellationToken cancellationToken)
            => ReadAsync(CreateOrderQuery(pnr), cancellationToken);

        public async Task<IReadOnlyCollection<string>> GetOrderIdsByPnrAsync(string pnr, CancellationToken cancellationToken)
        {
            var query = CreateOrderQuery(pnr);
            query.SqlQuery = Constants.Application.ScvOrderIdsByPnrQuery;
            return (await ReadAsync(query, cancellationToken)).Select(document => document.OrderID).ToArray();
        }

        public async Task<IReadOnlyCollection<string>> GetMatchingOrderIdsAsync(string orderId, string flightDate, string firstName, string lastName, CancellationToken cancellationToken)
        {
            var query = CreateOrderQuery(orderId);
            query.SqlQuery = Constants.Application.ScvMatchingOrderIdsQuery;
            query.QueryParams =
            [
                new KeyValuePair<string, object>("DocType", Constants.Application.OrderDocType),
                new KeyValuePair<string, object>("OrderId", orderId),
                new KeyValuePair<string, object>("FlightDate", flightDate + "T"),
                new KeyValuePair<string, object>("FirstName", firstName),
                new KeyValuePair<string, object>("LastName", lastName)
            ];
            return (await ReadAsync(query, cancellationToken)).Select(document => document.OrderID).ToArray();
        }

        public Task<IReadOnlyCollection<OrderDoc>> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(orderId);
            var query = CreateOrderQuery(orderId);
            query.SqlQuery = Constants.Application.ScvOrderByOrderIdQuery;
            query.QueryParams =
            [
                new KeyValuePair<string, object>("DocType", Constants.Application.OrderDocType),
                new KeyValuePair<string, object>("OrderId", orderId)
            ];
            return ReadAsync(query, cancellationToken);
        }

        private async Task<IReadOnlyCollection<OrderDoc>> ReadAsync(CosmosReaderServiceQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var documents = await _queryHandler.Handle(query, cancellationToken);
                return documents.Select(document =>
                {
                    if (document is null)
                        return null;

                    // Round-trip tokens as JSON so date-valued JTokens retain ISO
                    // text instead of culture-formatted strings during conversion.
                    return Json.Deserialize<OrderDoc>(Json.Serialize(document));
                })
                .Where(document => document is not null)
                .ToList();
            }
            catch (Exception exception) when (exception is not OperationCanceledException and not CosmosDBException)
            {
                var error = new Response<string>
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };

                throw new CosmosDBException(Json.Serialize(error.Errors));
            }
        }

        private static CosmosReaderServiceQuery CreateOrderQuery(string pnr)
        {
            return new CosmosReaderServiceQuery
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderSCVDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.OrderDB),
                CollectionName = ConfigReader.Get(Constants.Environment.OrdersCollection),
                SqlQuery = Constants.Application.ScvOrderByPnrQuery,
                QueryParams =
                [
                    new KeyValuePair<string, object>("DocType", Constants.Application.OrderDocType),
                    new KeyValuePair<string, object>("Pnr", pnr)
                ]
            };
        }
    }
}
