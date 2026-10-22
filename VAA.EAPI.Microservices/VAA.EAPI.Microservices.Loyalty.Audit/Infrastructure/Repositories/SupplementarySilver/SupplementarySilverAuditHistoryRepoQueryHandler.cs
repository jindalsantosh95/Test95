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

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.SupplementarySilver
{
    public class SupplementarySilverAuditHistoryRepoQueryHandler : IRequestHandler<SupplementarySilverAuditHistoryRepoQuery, IEnumerable<LoyaltyGenericAudit>>
    {
        private readonly IMediator _mediator;

        public SupplementarySilverAuditHistoryRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<LoyaltyGenericAudit>> Handle(SupplementarySilverAuditHistoryRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CustomerId),
                new KeyValuePair<string, object>("docType", DocumentType.OtherAudit.ToString()),
                new KeyValuePair<string, object>("auditCategory", Constants.Application.SupplementarySilverAuditCategory)
            };

            return Json.Deserialize<List<LoyaltyGenericAudit>>(Json.Serialize(await _mediator.Send(new CosmosReaderServiceQuery(ConfigReader.Get(Constants.Environment.AuditDBCollection),
                ConfigReader.Get(Constants.Environment.AuditDBEndPoint),
                ConfigReader.Get(Constants.Environment.AuditDBName),
                dBSearchParams,
                Constants.Application.LoyaltyGenericAuditSearchDbQuery))));
        }
    }
}