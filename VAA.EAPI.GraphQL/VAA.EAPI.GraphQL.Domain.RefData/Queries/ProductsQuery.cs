using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.RefData.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ProductsQuery
    {
        [UseSorting]
        [UseFiltering]
        [GraphQLName("searchProducts")]
        public async Task<IEnumerable<Product>> GetProducts([Service] IRefDataService<Product> serviceProvider, [Service] ILogger<ProductsQuery> logger)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));

            try
            {
                return await serviceProvider.Query();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Unable to retrieve Product Reference data");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unahandled Exception in Product Reference Data Query");
                throw;
            }
        }
    }
}
