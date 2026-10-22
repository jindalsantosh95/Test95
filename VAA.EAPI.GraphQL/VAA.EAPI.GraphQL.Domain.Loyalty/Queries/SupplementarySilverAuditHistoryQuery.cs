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
    public class SupplementarySilverAuditHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<SupplementarySilverAuditHistoryQuery> _logger;
        private readonly ISupplementarySilverAuditHistoryQueryProvider _supplementarySilverAuditHistoryQueryProfider;

        public SupplementarySilverAuditHistoryQuery(ILogger<SupplementarySilverAuditHistoryQuery> logger, ICustomerProvider customerService, ISupplementarySilverAuditHistoryQueryProvider supplementarySilverAuditHistoryQueryProfider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _supplementarySilverAuditHistoryQueryProfider = supplementarySilverAuditHistoryQueryProfider ?? throw new ArgumentNullException(nameof(supplementarySilverAuditHistoryQueryProfider));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.Others" })]
        [GraphQLName("retrieveSupplementarySilverAuditHistory")]
        [GraphQLDescription("Retreive a list of Supplementary Silver Audit Documents")]
        public async Task<SupplementarySilverAuditHistoryResponse> RetrieveSupplementarySilverAuditHistory(string customerId, string memberNumber)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await _supplementarySilverAuditHistoryQueryProfider.GetSupplementarySilverAuditHistory(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured in Supplementary Silver history retrieve query");
                throw;
            }
        }
    }
}