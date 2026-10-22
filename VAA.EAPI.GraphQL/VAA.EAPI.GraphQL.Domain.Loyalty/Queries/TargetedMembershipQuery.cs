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
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TargetedMembershipQuery
    {
        private readonly ILogger<TargetedMembershipQuery> _logger;

        public TargetedMembershipQuery(ILogger<TargetedMembershipQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "TargetedMembership.Read" })]
        [GraphQLName("loyaltyTargetedMembership")]
        [GraphQLDescription("Retreive a list of Targeted Membership documents for the member")]
        public async Task<IEnumerable<TargetedMembership>> TargetedMembership(string customerId, string memberId, [Service] ICustomerProvider customerService, [Service] ITargetedMembershipQueryProvider notesService, [Service] ILogger<TargetedMembershipQuery> logger)
        {
            if (notesService is null)
                throw new ArgumentNullException(nameof(notesService));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberId))
                throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be spefied.");

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            return await notesService.Query(customerId);
        }
    }
}
