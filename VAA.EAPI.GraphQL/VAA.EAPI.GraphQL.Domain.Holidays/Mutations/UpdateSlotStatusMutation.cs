using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class UpdateSlotStatusMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public UpdateSlotStatusMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerUpdateSlotStatus)]
    [GraphQLDescription("Update status for a store slot.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<StoreManagerWindowResponse> UpdateSlotStatus(string storeId, string slotId, SlotStatusUpdateRequest request)
    {
        return await _storeServiceProvider.UpdateSlotStatusAsync(storeId, slotId, request);
    }
}
