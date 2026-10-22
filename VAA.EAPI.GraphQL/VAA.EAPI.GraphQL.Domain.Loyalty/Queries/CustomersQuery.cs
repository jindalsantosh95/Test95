using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CustomersQuery
    {
        private readonly ILogger<ICustomerLogger> _logger;
        private readonly ICustomerProvider _customerProvider;

        public CustomersQuery(ICustomerProvider customerProvider, ILogger<ICustomerLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [Authorize(Roles = new[] { "Customer.Read" })]
        [UseFiltering]
        [GraphQLName("loyaltyCustomer")]
        [GraphQLDescription("Retrieve customer details, either with customer ID or loyalty member number")]
        public async Task<CustomerResponse> Customer(string customerId, string memberNumber)
        {
            try
            {
                var cust = await _customerProvider.GetCustomer(customerId, memberNumber);

                return new CustomerResponse(cust.Customer);
            }
            catch (HttpRequestException ex)
            {
                var content = await ex.ReadResponseContentAsStringAsync();

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unable to retrieve Customer details [{ex.Message}]")
                            .SetExtension("APIResponse", content)
                            .SetCode(((int)ex.StatusCode).ToString())
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unahandled Exception in Customer query\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Unahandled Exception in Customer query")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
        }
    }
}
