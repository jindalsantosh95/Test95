using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails
{
    public class AgencyOCNDetailsQuery(string agencyId, string aggregatorId, string environment) : IRequest<Response<IEnumerable<Domain.AgencyOCNDetails.AgencyOCNDetails>>>
    {
        public string AgencyId { get; set; } = agencyId;
        public string AggregatorId { get; set; } = aggregatorId;
        public string Environment { get; set; } = environment;
    }
}
