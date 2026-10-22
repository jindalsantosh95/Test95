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
    public class PreferenceAuditHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<PreferenceAuditHistoryQuery> _logger;
        private readonly IPreferenceAuditHistoryQueryProvider _preferenceAuditHistoryQueryProviderService;

        public PreferenceAuditHistoryQuery(ILogger<PreferenceAuditHistoryQuery> logger, ICustomerProvider customerService, IPreferenceAuditHistoryQueryProvider preferenceAuditHistoryQueryProviderService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _preferenceAuditHistoryQueryProviderService = preferenceAuditHistoryQueryProviderService ?? throw new ArgumentNullException(nameof(preferenceAuditHistoryQueryProviderService));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.Preference" })]
        [GraphQLName("retrievePreferenceAuditHistory")]
        [GraphQLDescription("Retreive a list of Prefrerence Audit Documents")]
        public async Task<PreferenceAuditHistoryResponse> RetrievePreferenceAuditHistory(string customerId, string memberNumber)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await _preferenceAuditHistoryQueryProviderService.GetPreferenceAuditHistory(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in preference history retrieve query");
                throw;
            }
        }
    }
}