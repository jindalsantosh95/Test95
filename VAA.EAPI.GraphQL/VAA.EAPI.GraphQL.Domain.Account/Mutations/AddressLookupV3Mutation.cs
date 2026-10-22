using HotChocolate;
using HotChocolate.Types;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class AddressLookupV3Mutation
    {
        private readonly IAddressLookupV3Provider addressLookupV3Provider;

        public AddressLookupV3Mutation(IAddressLookupV3Provider addressLookupV3Provider)
        {
            this.addressLookupV3Provider = addressLookupV3Provider ?? throw new ArgumentNullException(nameof(addressLookupV3Provider));
        }

        [GraphQLName("accountAddressSelectionV3")]
        [GraphQLDescription("Retrieves one selected address through the Address Lookup v3 contract.")]
        public async Task<AddressLookupV3SelectionResponse> GetSelectionAsync(AddressLookupV3SelectionInput request, CancellationToken cancellationToken)
        {
            return await addressLookupV3Provider.GetSelectionAsync(request, cancellationToken);
        }
    }
}
