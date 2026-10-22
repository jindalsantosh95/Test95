using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IHouseHoldMutationProvider
    {
        Task<ResponseMessage> HouseHoldLinking(HouseHoldLinkingRequest linking);
    }
}
