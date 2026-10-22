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
    public class FQTProgrammeQuery
    {
        [UseSorting]
        [UseFiltering]
        [GraphQLName("retrieveFQTProgrammes")]
        public async Task<IQueryable<FQTProgrammeRefData>> GetFQTProgrammes([Service] IRefServiceProvider serviceProvider, [Service] ILogger<FQTProgrammeQuery> logger)
        {
            try
            {
                return (await serviceProvider.RetrieveFQTProgrammesAsync()).AsQueryable();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled Exception in FQT Programme Reference Data Query");
                throw;
            }
        }
    }
}