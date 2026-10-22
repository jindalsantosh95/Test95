using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class PaymentDocRepoQueryHandler : IRequestHandler<PaymentDocRepoQuery, IEnumerable<object>>
    {
        private readonly IMediator _mediator;

        public PaymentDocRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<object>> Handle(PaymentDocRepoQuery request, CancellationToken cancellationToken)
        {
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.PaymentDBEndpoint),
                CollectionName = ConfigReader.Get(Constants.Environment.PaymentCollection),
                DatabaseName = ConfigReader.Get(Constants.Environment.PaymentDatabaseName),
                SqlQuery = request.Query,
                QueryParams = new()
                {
                   new KeyValuePair<string, object>("OrderId", request.OrderId)
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
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };

                throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }

            return cosmosDocs;
        }
    }
}
