using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ContactHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<ContactHistoryQuery> _logger;
        private readonly IContactHistoryQueryProvider _contactHistoryQueryService;

        public ContactHistoryQuery(ILogger<ContactHistoryQuery> logger, ICustomerProvider customerService, IContactHistoryQueryProvider contactHistoryQueryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _contactHistoryQueryService = contactHistoryQueryService ?? throw new ArgumentNullException(nameof(contactHistoryQueryService));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.ContactHistory" })]
        [GraphQLName("retrieveContactHistory")]
        [GraphQLDescription("Retreive a list of Contact History Documents")]

        public async Task<ContactHistoryAuditResponse> RetrieveContactHistory(string customerId, string memberNumber)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");
            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await _contactHistoryQueryService.GetContactHistory(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in contatc history retrieve query");
                throw;
            }
        }
    }
}
