using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.CustomerProfile
{
    public class CustomerProfileAuditHistoryRepoQueryHandler : IRequestHandler<CustomerProfileAuditHistoryRepoQuery, IEnumerable<CustomerProfileAudit>>
    {
        private readonly IMediator _mediator;

        public CustomerProfileAuditHistoryRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<CustomerProfileAudit>> Handle(CustomerProfileAuditHistoryRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CustomerId),
                new KeyValuePair<string, object>("docType", DocumentType.CustomerProfileAudit.ToString())
            };

            return Json.Deserialize<List<CustomerProfileAudit>>(Json.Serialize(await _mediator.Send(new CosmosReaderServiceQuery(ConfigReader.Get(Constants.Environment.AuditDBCollection),
                ConfigReader.Get(Constants.Environment.AuditDBEndPoint),
                ConfigReader.Get(Constants.Environment.AuditDBName),
                dBSearchParams,
                Constants.Application.SearchDbQuery))));
        }
    }
}