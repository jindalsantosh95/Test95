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

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Preference
{
    public class PreferenceAuditHistoryRepoQueryHandler : IRequestHandler<PreferenceAuditHistoryRepoQuery, IEnumerable<PreferenceAudit>>
    {
        private readonly IMediator _mediator;

        public PreferenceAuditHistoryRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<PreferenceAudit>> Handle(PreferenceAuditHistoryRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CustomerId),
                new KeyValuePair<string, object>("docType", DocumentType.PreferenceAudit.ToString())
            };

            return Json.Deserialize<List<PreferenceAudit>>(Json.Serialize(await _mediator.Send(new CosmosReaderServiceQuery(ConfigReader.Get(Constants.Environment.AuditDBCollection),
                ConfigReader.Get(Constants.Environment.AuditDBEndPoint),
                ConfigReader.Get(Constants.Environment.AuditDBName),
                dBSearchParams,
                Constants.Application.SearchDbQuery))));
        }
    }
}