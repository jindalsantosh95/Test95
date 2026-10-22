using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IHouseHoldMutationProvider
    {
        Task<string> HouseHoldLinking(HouseHoldLinkingRequest linking);
        Task<string> HouseHoldDelinking(HouseHoldDelinkingRequest delinkingRequest);
    }
}
