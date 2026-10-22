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
    public class AmexCenturionAuditHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<AmexCenturionAuditHistoryQuery> _logger;
        private readonly IAmexCenturionAuditHistoryQueryProvider _amexCenturionAuditHistoryQueryProvider;

        public AmexCenturionAuditHistoryQuery(ILogger<AmexCenturionAuditHistoryQuery> logger, ICustomerProvider customerService, IAmexCenturionAuditHistoryQueryProvider amexCenturionAuditHistoryQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _amexCenturionAuditHistoryQueryProvider = amexCenturionAuditHistoryQueryProvider ?? throw new ArgumentNullException(nameof(amexCenturionAuditHistoryQueryProvider));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.CardLinking" })]
        [GraphQLName("retrieveAmexCenturionAuditHistory")]
        [GraphQLDescription("Retreive a list of Amex Centurion Audit Documents")]
        public async Task<AmexCenturionAuditHistoryResponse> RetrieveAmexCenturionAuditHistory(string customerId, string memberNumber)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await _amexCenturionAuditHistoryQueryProvider.GetAmexCenturionAuditHistory(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in amex centurion history retrieve query");
                throw;
            }
        }
    }
}
