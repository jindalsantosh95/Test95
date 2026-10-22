using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.RefData.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class TitlesQuery
{
    [UseSorting]
    [UseFiltering]
    [GraphQLName("searchTitles")]
    public async Task<IEnumerable<TitleDetails>> GetTitles([Service] IRefDataService<TitleDetails> refDataService, [Service] ILogger<TitlesQuery> logger)
    {
        ArgumentNullException.ThrowIfNull(refDataService);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            return await refDataService.Query();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to retrieve title reference data");
            throw;
        }
    }
}
