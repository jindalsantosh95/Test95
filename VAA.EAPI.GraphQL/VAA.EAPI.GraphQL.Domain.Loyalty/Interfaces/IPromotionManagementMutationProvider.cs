using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPromotionManagementMutationProvider
    {
        Task<string> UpdatePromotion(PromotionUpdateMutationRequest promotion);
        Task<string> CreatePromotion(PromotionCreateMutationRequest promotion);
    }
}
