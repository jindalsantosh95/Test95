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
    public class MemberAccountQuery
    {
        private readonly ILogger<MemberAccountQuery> _logger;
        public MemberAccountQuery(ILogger<MemberAccountQuery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.MemberAccount.Read" })]
        [GraphQLName("loyaltyMemberAccount")]
        [GraphQLDescription("Retreive a list of Loyalty Accounts assocaited to the member")]

        public async Task<MemberAccount> MemberAccount(string customerId, string memberId, [Service] ICustomerProvider customerService, [Service] IMemberAccountQueryProvider memberAccountService, [Service] ICustomerTransactionQueryProvider customerTransactionQueryProvider, [Service] ILogger<MemberAccountQuery> logger)
        {
            if (memberAccountService is null)
                throw new ArgumentNullException(nameof(memberAccountService));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberId))
                throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be spefied.");

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            var memberAccount =  await memberAccountService.Query(customerId);
            
            var response = await customerTransactionQueryProvider.GetMemberCustomAttributes(customerId);
            
            memberAccount.MemberCustomAttributes = response.MemberCustomAttributesDocuments.FirstOrDefault();

            return memberAccount;
        }

    }
}
