using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PartnersQuery
    {
        private readonly ILogger<PartnersQuery> _logger;
        public PartnersQuery(ILogger<PartnersQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Partners.Read" })]
        [GraphQLName("loyaltyPartners")]
        [GraphQLDescription("Retreive a list of Campaign Types from reference data")]

        public async Task<List<PartnerProfile>> Partners([Service] IPartnersQueryProvider partnersService, [Service] ILogger<PartnersQuery> logger)
        {
            _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.PartnersQuery.Partners() - Start");

            try
            {
                if (partnersService is null)
                    throw new ArgumentNullException(nameof(partnersService));
                try
                {
                    return await partnersService.Query();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error has occured in partners query");
                    throw;
                }
            }
            finally
            {
                _logger.LogInformation("VAA.EAPI.GraphQL.Domain.Loyalty.Queries.PartnersQuery.Partners() - End");
            }
        }
    }
}
