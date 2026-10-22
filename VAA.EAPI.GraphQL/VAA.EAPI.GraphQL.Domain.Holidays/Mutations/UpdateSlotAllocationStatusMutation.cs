using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class UpdateSlotAllocationStatusMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public UpdateSlotAllocationStatusMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerUpdateSlotAllocationStatus)]
    [GraphQLDescription("Blocks or releases one allocation row for a store slot.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<ManageSlotAllocationResponse> UpdateSlotAllocationStatus(
        string storeId,
        string slotId,
        string allocationId,
        StoreManagerSlotAllocationStatusUpdateRequest request)
    {
        return await _storeServiceProvider.UpdateSlotAllocationStatusAsync(storeId, slotId, allocationId, request);
    }
}
