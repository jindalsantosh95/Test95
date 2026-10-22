using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest;

namespace VAA.EAPI.GraphQL.Domain.Operations.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PassengerManifestQuery
    {
        private readonly ILogger<PassengerManifestQuery> _logger;
        private readonly IPassengerManifestProvider _provider;

        public PassengerManifestQuery(IPassengerManifestProvider provider, ILogger<PassengerManifestQuery> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Operations.PassengerManifest" })]
        [GraphQLName("operationsPassengerManifest")]
        [GraphQLDescription("Retrieve a list of a member's contact notes")]
        public async Task<PassengerManifestResponse> PassengerManifest(PassengerManifestRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            try
            {
                return await _provider.Query(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to retrieve passenger manifest");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unahandled exception in passenger manifest query");
                throw;
            }
        }
    }
}
