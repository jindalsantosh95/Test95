using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.AddressSearch;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AddressSearchLookupQuery
    {
        private readonly ILogger<AddressSearchLookupQuery> _logger;
        private readonly IAddressSearchLookupQueryProvider _addressSearchLookupQueryProvider;

        public AddressSearchLookupQuery(ILogger<AddressSearchLookupQuery> logger, IAddressSearchLookupQueryProvider addressSearchLookupQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _addressSearchLookupQueryProvider = addressSearchLookupQueryProvider ?? throw new ArgumentNullException(nameof(addressSearchLookupQueryProvider));
        }

        [UseFiltering]
        [Authorize(Roles = ["Loyalty.Address.Search"])]
        [GraphQLName("addressSearchbyPostcode")]
        [GraphQLDescription("Retrieve Address Details for US and UK postcode")]
        public async Task<List<AddressSearchbyPostcodeResponse>> AddressSearchbyPostcode(AddressSearchbyPostcodeRequest request)
        {
            try
            {
                _logger.LogInformation("Retrieving Address Details");                

                var apiResponse = await _addressSearchLookupQueryProvider.GetAddressSearchbyPostcode(request);
                return apiResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AddressSearchbyPostcode Query");
                throw;
            }
        }   
    }
}
