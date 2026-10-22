using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PromotionEnrolledMembersQuery
    {
        private readonly ILogger<PromotionEnrolledMembersQuery> _logger;

        public PromotionEnrolledMembersQuery(ILogger<PromotionEnrolledMembersQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.PromotionEnrolledMembers.Read" })]
        [GraphQLName("loyaltyPromotionEnrolledMembers")]
        [GraphQLDescription("Retreive a list of Members enrolled for a given Loyalty promotion")]
        public async Task<PromotionEnrolledMemberResponse> EnrolledMembers(string promotionId, [Service] IPromotionEnrolledMembersQueryProvider promotionEnrolledMembersService, [Service] ILogger<PromotionEnrolledMembersQuery> logger)
        {

            if (string.IsNullOrEmpty(promotionId))
                throw new ArgumentException($"'{nameof(promotionId)}' must be spefied.");

            return await promotionEnrolledMembersService.Query(promotionId);
        }
    }
}

