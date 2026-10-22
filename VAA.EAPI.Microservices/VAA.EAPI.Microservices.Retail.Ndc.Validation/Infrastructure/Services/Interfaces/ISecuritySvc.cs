using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces
{
    public interface ISecuritySvc
    {
        Task<SubscriptionResponse> RetrieveSubscription(string agencyID, string environment);
    }
}
