using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface ISkyRetroProvider<T>
    {
        Task<T> CheckClaimEligibility(CustomerDTO customerInfo, SkyFlightsPointsClaimRequest flightRequest);
        Task<T> FeedBackUpdate(CheckEligibilityClaimResponse feedbackRequest, SegmentResponse inRuleResponse);
    }
}
