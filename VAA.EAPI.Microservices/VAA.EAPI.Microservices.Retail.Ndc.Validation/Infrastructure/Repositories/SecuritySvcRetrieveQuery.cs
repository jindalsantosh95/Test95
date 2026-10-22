using MediatR;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories
{
    public class SecuritySvcRetrieveQuery : IRequest<SubscriptionResponse>
    {
        public string AgencyID { get; set; }
        public string Environment { get; set; }
    }
}
