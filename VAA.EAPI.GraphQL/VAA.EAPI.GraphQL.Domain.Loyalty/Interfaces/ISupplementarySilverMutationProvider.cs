using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ISupplementarySilverMutationProvider
    {
        Task<string> SupplementarySilverDelinking(SupplementarySilverDelinkingRequest delinkingRequest);
        Task<string> SupplementarySilverLinking(SupplementarySilverLinkingRequest linking);
    }
}
