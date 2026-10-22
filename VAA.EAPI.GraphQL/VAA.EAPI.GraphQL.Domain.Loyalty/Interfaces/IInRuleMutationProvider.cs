using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IInRuleMutationProvider
    {
        Task<CreditPointsResponse> CalculatePoints(AirlineEarnedPointsRequest request);
    }
}
