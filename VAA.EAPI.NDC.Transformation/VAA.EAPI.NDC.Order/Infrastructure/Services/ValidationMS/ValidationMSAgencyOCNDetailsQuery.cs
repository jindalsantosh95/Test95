using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS
{
    public class ValidationMSAgencyOCNDetailsQuery : IRequest<IEnumerable<AgencyOCNDetails>>
    {
        public string AgencyId {  get; set; }   
        public string AggregatorId { get; set; }
        public string Environment { get; set; }
    }
}
