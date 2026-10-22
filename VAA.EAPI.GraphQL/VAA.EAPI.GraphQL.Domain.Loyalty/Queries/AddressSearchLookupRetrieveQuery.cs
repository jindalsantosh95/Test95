using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.AddressSearch;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AddressSearchLookupRetrieveQuery
    {
        private readonly ILogger<AddressSearchLookupRetrieveQuery> _logger;
        private readonly IAddressSearchLookupQueryProvider _addressSearchLookupQueryProvider;

        public AddressSearchLookupRetrieveQuery(ILogger<AddressSearchLookupRetrieveQuery> logger, IAddressSearchLookupQueryProvider addressSearchLookupQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _addressSearchLookupQueryProvider = addressSearchLookupQueryProvider ?? throw new ArgumentNullException(nameof(addressSearchLookupQueryProvider));
        }

        [UseFiltering]
        [Authorize(Roles = ["Loyalty.Address.Search"])]
        [GraphQLName("addressSearchbyRetrieve")]
        [GraphQLDescription("Retrieve Address Details for US and UK postcode")]
        public async Task<List<AddressSearchbyRetrieveResponse>> AddressSearchbyRetrieve(string addressId)
        {
            try
            {
                return await _addressSearchLookupQueryProvider.GetAddressSearchbyRetrieve(addressId);               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AddressSearchbyRetrieve Query");
                throw;
            }
        }
    }
}
