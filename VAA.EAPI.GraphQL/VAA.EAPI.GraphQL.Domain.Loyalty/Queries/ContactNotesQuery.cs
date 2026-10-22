using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ContactNotesQuery
    {
        private readonly ILogger<ContactNotesQuery> _logger;

        public ContactNotesQuery(ILogger<ContactNotesQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "ContactNotes.Read" })]
        [GraphQLName("loyaltyContactNotes")]
        [GraphQLDescription("Retrieve a list of a member's contact notes")]
        public async Task<IEnumerable<ContactNote>> ContactNotes(string? customerId, string? memberId, [Service] ICustomerProvider customerService, [Service] IContactNoteQueryProvider notesService, [Service] ILogger<ContactNotesQuery> logger)
        {
            if (notesService is null)
                throw new ArgumentNullException(nameof(notesService));

            if (customerService is null)
                throw new ArgumentNullException(nameof(customerService));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberId))
                throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be spefied.");

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            return await notesService.Query(customerId);
        }
    }
}
