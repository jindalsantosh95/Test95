using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ILoyaltyMemberMgtProvider
    {
        Task<RewardBookingPointCalculationResponse> RetrieveRewardBookingPoints(RewardBookingPointCalculationRequest rewardBookingPointCalculationRequest);
        Task<TransactionStatementTextResponse> GenerateStatetmentText(TransactionStatementTextRequest statementRequest);
    }
}