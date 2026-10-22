using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.EnrolmentCustomer
{
    public class GetCustTransDocumentService<T> : IGetCustTransDocumentService<T>
    {
        private readonly IMediator _mediator;
        public GetCustTransDocumentService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<T> GetCosmosDoc(string docType, string customerId)
        {
            ArgumentNullException.ThrowIfNull(docType);
            ArgumentNullException.ThrowIfNull(customerId);

            var cosmosReaderRepo = new CosmosReaderRepoQuery()
            {
                CollectionName = ConfigReader.Get(Constants.Environment.SCVCustTxnDBPartnerMemContnr),
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                QueryParams =
                    [
                        new KeyValuePair<string, object>("docType", docType),
                        new KeyValuePair<string, object>("partitionKey", customerId)
                    ],
                SqlQuery = Constants.Application.SearchDbQuery
            };

            return Json.DeserializeObject<T>((JToken)(await _mediator.Send(cosmosReaderRepo, new CancellationToken()))?.FirstOrDefault());
        }
    }
}