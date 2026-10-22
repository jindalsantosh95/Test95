using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ISkyPartnerRetroMutationProvider
    {
        //will be updated
        Task<FlyingActivityDetails> GetFlyingActivityDetails(SkyPartnerMSRequest ticketDetails);
        Task<SkyMSSuccessResponse> RetroCreditFeedback(SkyMSFeedbackRequest request);
    }
}
