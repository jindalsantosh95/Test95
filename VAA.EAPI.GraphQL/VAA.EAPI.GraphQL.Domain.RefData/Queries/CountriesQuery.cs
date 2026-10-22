using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.RefData.Queries
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
