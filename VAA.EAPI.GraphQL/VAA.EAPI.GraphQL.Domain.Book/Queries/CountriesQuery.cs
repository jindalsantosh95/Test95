using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CountriesQuery
    {
        [UseSorting]
        [UseFiltering]
        [GraphQLName("searchCountries")]
        public async Task<IQueryable<CountryRefData>> GetCountries([Service] IRefServiceProvider serviceProvider, [Service] ILogger<CountriesQuery> logger)
        {
            try
            {
                return (await serviceProvider.RetrieveCountriesAsync()).AsQueryable();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled Exception in Country Reference Data Query");
                throw;
            }
        }
    }
}
