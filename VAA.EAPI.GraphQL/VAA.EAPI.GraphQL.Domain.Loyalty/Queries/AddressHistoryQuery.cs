using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AddressHistoryQuery
    {
        private readonly ICustomerProvider _customerService;
        public AddressHistoryQuery(ICustomerProvider customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.Audit.AddressHistory" })]
        [GraphQLName("RetrieveAddressHistory")]
        [GraphQLDescription("Retreive a list of Address History Documents")]

        public async Task<IEnumerable<AddressHistory>> RetrieveAddressHistory(string? customerId, string? memberNumber, [Service] IAddressHistoryQueryProvider addressHistoryQueryService)
        {
            if (addressHistoryQueryService is null)
                throw new ArgumentNullException(nameof(addressHistoryQueryService));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");
            try
            {
                if (string.IsNullOrEmpty(customerId))
                    customerId = (await _customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

                return await addressHistoryQueryService.GetAddressHistory(customerId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}