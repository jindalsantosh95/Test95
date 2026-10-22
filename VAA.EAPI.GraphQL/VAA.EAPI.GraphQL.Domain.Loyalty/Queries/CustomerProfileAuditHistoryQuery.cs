using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CustomerProfileAuditHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<CustomerProfileAuditHistoryQuery> _logger;
        private readonly ICustomerProfileAuditHistoryQueryProvider _customerProfileAuditHistoryQueryProvider;

        public CustomerProfileAuditHistoryQuery(ILogger<CustomerProfileAuditHistoryQuery> logger, ICustomerProvider customerService, ICustomerProfileAuditHistoryQueryProvider customerProfileAuditHistoryQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _customerProfileAuditHistoryQueryProvider = customerProfileAuditHistoryQueryProvider ?? throw new ArgumentNullException(nameof(customerProfileAuditHistoryQueryProvider));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.CustomerProfile" })]
        [GraphQLName("retrieveCustomerProfileAuditHistory")]
        [GraphQLDescription("Retreive a list of Customer Profile Audit Documents")]
        public async Task<CustomerProfileAuditHistoryResponse> RetrieveCustomerProfileAuditHistory(string customerId, string memberNumber)
        {
            if (string.IsNullOrWhiteSpace(customerId) && string.IsNullOrWhiteSpace(memberNumber))
            {
                if (customerId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(customerId));
                if (memberNumber.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(memberNumber));
            }

            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await _customerProfileAuditHistoryQueryProvider.GetCustomerProfileAuditHistory(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in award history retrieve query");
                throw;
            }
        }
    }
}