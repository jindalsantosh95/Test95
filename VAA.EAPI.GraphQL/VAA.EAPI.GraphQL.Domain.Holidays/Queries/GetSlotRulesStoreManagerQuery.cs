using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetSlotRulesStoreManagerQuery
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public GetSlotRulesStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Queries.StoreManagerSlotRules)]
    [GraphQLDescription("Get slot rules for a store.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<IEnumerable<SlotRuleResponse>> GetSlotRules(string storeId)
    {
        return await _storeServiceProvider.GetSlotRulesAsync(storeId);
    }
}
