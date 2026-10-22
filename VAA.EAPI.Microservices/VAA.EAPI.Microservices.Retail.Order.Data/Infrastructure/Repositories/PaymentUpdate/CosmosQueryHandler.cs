using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate
{
    public class CosmosQueryHandler : IRequestHandler<CosmosQueryCommand, List<object>>
    {
        private readonly IMediator _mediator;

        public CosmosQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<List<object>> Handle(CosmosQueryCommand request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> pnrSearchQueryParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.PartitionKey),
                new KeyValuePair<string, object>("docType", request.DocType)
            };
            CosmosReaderServiceQuery cosmosReaderRepoCommand = CosmosReaderServiceQueryMapper.Mapper(Constants.Application.OrderDBSqlQuery, pnrSearchQueryParams);
            try
            {
                return await _mediator.Send(cosmosReaderRepoCommand, cancellationToken);
            }
            catch
            {
                var error = new Response<string>()
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                }; throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }
        }
    }
}
