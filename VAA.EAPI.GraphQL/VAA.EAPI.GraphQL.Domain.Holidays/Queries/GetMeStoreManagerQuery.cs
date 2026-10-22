using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetMeStoreManagerQuery
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public GetMeStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Queries.StoreManagerMe)]
    [GraphQLDescription("Get current store manager identity context.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<ManageMeResponse> GetMe()
    {
        return await _storeServiceProvider.GetMyStoreUserAsync();
    }
}
