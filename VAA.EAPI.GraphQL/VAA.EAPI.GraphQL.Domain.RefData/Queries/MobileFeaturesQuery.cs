using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.RefData.Models;

namespace VAA.EAPI.GraphQL.Domain.RefData.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class MobileFeaturesQuery
    {
        [GraphQLName("mobileFeatures")]
        public Task<MobileFeatures> GetMobileFeaturesAsync([Service] IRefServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            return serviceProvider.RetrieveMobileFeaturesAsync(cancellationToken);
        }
    }
}
