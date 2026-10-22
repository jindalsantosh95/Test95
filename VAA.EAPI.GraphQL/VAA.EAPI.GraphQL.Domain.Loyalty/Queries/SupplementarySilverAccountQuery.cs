using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SupplementarySilverAccountQuery
    {
        public SupplementarySilverAccountQuery()
        {
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.SupplementarySilver.Read" })]
        [GraphQLName("loyaltySupplementarySilver")]
        [GraphQLDescription("Retreive Supplementary Silver Account Details associated to the member")]
        public async Task<SupplementarySilverAccountResponse> SupplementarySilverAccountAsync(string customerId,
            string memberId,
            [Service] ICustomerProvider customerService,
            [Service] ISupplementarySilverAccountQueryProvider supplementarySilverAccountProvider,
            [Service] ISupplementarySilverAuditHistoryQueryProvider supplementarySilverAuditHistoryQueryProvider)
        {
            if (customerService is null)
                throw new ArgumentNullException(nameof(customerService));

            if (supplementarySilverAccountProvider is null)
                throw new ArgumentNullException(nameof(supplementarySilverAccountProvider));

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberId))
                throw new ArgumentException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be spefied.");

            if (supplementarySilverAuditHistoryQueryProvider is null)
                throw new ArgumentNullException(nameof(supplementarySilverAuditHistoryQueryProvider));

            if (string.IsNullOrEmpty(customerId))
            {
                customerId = (await customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;
            }

            SupplementarySilverAccountResponse response = await supplementarySilverAccountProvider.Query(customerId);

            SupplementarySilverAuditHistoryResponse ssaAuditResponse = await supplementarySilverAuditHistoryQueryProvider.GetSupplementarySilverAuditHistory(customerId);
            IEnumerable<LoyaltyGenericAudit> genericAudits = ssaAuditResponse.SupplementarySilverAuditHistory.Where(ii => ii.AuditSubCategory == Constants.Application.SupplementarySilverAuditSubCategoryLinking);
            DateTime? associationDate = genericAudits?.LastOrDefault()?.DataUpdatedOn;

            if (response.SsaChildMember != null && response.SsaChildMember.MemberNumber != null)
            {
                CustomerDTO customerDto = await customerService.GetCustomer(null, response.SsaChildMember.MemberNumber);
                response.SsaChildMember.MemberNumber = customerDto.Customer.LoyaltyNumber;
                response.SsaChildMember.FirstName = customerDto.Customer.FirstName;
                response.SsaChildMember.LastName = customerDto.Customer.LastName;
                response.SsaChildMember.MemberStatus = customerDto.Customer.Status;
                response.SsaChildMember.AssociationDate = associationDate;
                response.SsaChildMember.CustomerId = customerDto.Customer.CustomerId;
            }

            if (response.SsaHeadMember != null && response.SsaHeadMember.MemberNumber != null)
            {
                CustomerDTO customerDto = await customerService.GetCustomer(null, response.SsaHeadMember.MemberNumber);
                response.SsaHeadMember.MemberNumber = customerDto.Customer.LoyaltyNumber;
                response.SsaHeadMember.FirstName = customerDto.Customer.FirstName;
                response.SsaHeadMember.LastName = customerDto.Customer.LastName;
                response.SsaHeadMember.MemberStatus = customerDto.Customer.Status;
                response.SsaHeadMember.AssociationDate = associationDate;
                response.SsaHeadMember.CustomerId = customerDto.Customer.CustomerId;
            }
            return response;
        }
    }
}