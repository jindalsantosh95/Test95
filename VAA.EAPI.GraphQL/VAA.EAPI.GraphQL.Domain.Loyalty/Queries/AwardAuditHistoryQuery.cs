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
    public class AwardAuditHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<AwardAuditHistoryQuery> _logger;
        private readonly IAwardAuditHistoryQueryProvider _awardAuditHistoryQueryProvider;

        public AwardAuditHistoryQuery(ILogger<AwardAuditHistoryQuery> logger, ICustomerProvider customerService, IAwardAuditHistoryQueryProvider awardAuditHistoryQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _awardAuditHistoryQueryProvider = awardAuditHistoryQueryProvider ?? throw new ArgumentNullException(nameof(awardAuditHistoryQueryProvider));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.Award" })]
        [GraphQLName("retrieveAwardAuditHistory")]
        [GraphQLDescription("Retreive a list of Award Audit Documents")]
        public async Task<AwardAuditHistoryResponse> RetrieveAwardAuditHistory(string customerId, string memberNumber)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await _awardAuditHistoryQueryProvider.GetAwardAuditHistory(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in award history retrieve query");
                throw;
            }
        }
    }
}