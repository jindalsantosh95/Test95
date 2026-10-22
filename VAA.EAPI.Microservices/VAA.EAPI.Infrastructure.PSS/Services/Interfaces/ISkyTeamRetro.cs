using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface ISkyTeamRetro
    {
        Task<OnlineRetroCreditingResponse> SkyTeamRetroEligibiltycheck(OnlineRetroCreditRequestType eligibiltyCheckRequest);
        Task<OnlineRetroCreditFeedbackResponse> SkyTeamRetroFeedback(OnlineFeedbackRequestType skyFeedbackRequest);
    }
}