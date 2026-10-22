using HotChocolate;
using HotChocolate.Types;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class AddressLookupV3Query
    {
        private readonly IAddressLookupV3Provider addressLookupV3Provider;

        public AddressLookupV3Query(IAddressLookupV3Provider addressLookupV3Provider)
        {
            this.addressLookupV3Provider = addressLookupV3Provider ?? throw new ArgumentNullException(nameof(addressLookupV3Provider));
        }

        [GraphQLName("accountAddressSuggestionsV3")]
        [GraphQLDescription("Returns address suggestions through the Address Lookup v3 contract.")]
        public async Task<AddressLookupV3SuggestionResponse> GetSuggestionsAsync(AddressLookupV3SuggestionInput request, CancellationToken cancellationToken)
        {
            return await addressLookupV3Provider.GetSuggestionsAsync(request, cancellationToken);
        }

    }
}
