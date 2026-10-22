using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class MemberCardLinksQuery
    {
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<MemberCardLinksQuery> _logger;
        private readonly IVredCardDetailsQueryProvider _vredCardDetailsQueryProvider;
        private readonly IPartnersProfilesQueryProvider _partnersProfilesQueryProvider;
        private readonly IPartnerMembershipDocQueryProvider _partnerMembershipDocQueryProvider;

        public MemberCardLinksQuery(ILogger<MemberCardLinksQuery> logger, IVredCardDetailsQueryProvider vredCardDetailsQueryProvider, IPartnersProfilesQueryProvider partnersProfilesQueryProvider, IPartnerMembershipDocQueryProvider partnerMembershipDocQueryProvider, ICustomerProvider customerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vredCardDetailsQueryProvider = vredCardDetailsQueryProvider ?? throw new ArgumentNullException(nameof(vredCardDetailsQueryProvider));
            _partnersProfilesQueryProvider = partnersProfilesQueryProvider ?? throw new ArgumentNullException(nameof(partnersProfilesQueryProvider));
            _partnerMembershipDocQueryProvider = partnerMembershipDocQueryProvider ?? throw new ArgumentNullException(nameof(partnerMembershipDocQueryProvider));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [UseFiltering]
        [Authorize(Roles = new[] { "Loyalty.MemberCardLinks.Read" })]
        [GraphQLName("loyaltyMemberCardLink")]
        [GraphQLDescription("Retrieve Member card details associated to the member")]
        public async Task<List<MemberCardLinkResponse>> GetMemberCardLink(string customerId, string memberNumber, [Service] ICustomerProvider customerService)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(memberNumber))
                throw new ArgumentException("Either 'Customer ID' or 'Member Number' must be specified");

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId) && customerService is not null)
                customerId = (await customerService.GetCustomer(customerId, memberNumber)).Customer.CustomerId;

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(memberNumber) && customerService is not null)
                memberNumber = (await customerService.GetCustomer(customerId, memberNumber)).Customer.LoyaltyNumber;

            IEnumerable<PartnerProfileDetails> partnerProfileDetails = await _partnersProfilesQueryProvider.GetPartnerProfileDetails();

            if (partnerProfileDetails is null) throw new ArgumentNullException(nameof(partnerProfileDetails));

            PartnerMembershipResponse partnerMembershipResponse = new();

            IEnumerable<PartnerMembership> partnerMembershipList = null;

            VredCardDetails cardDetails = new();

            if (!string.IsNullOrEmpty(customerId))
            {
                partnerMembershipResponse = await _partnerMembershipDocQueryProvider.GetPartnerMembershipDoc(customerId);

                if (partnerMembershipResponse != null)
                {
                    partnerMembershipList = partnerMembershipResponse.PartnerMembershipDocuments
                    .Where(t => t.PartnerId.Equals(partnerProfileDetails.Where(x => x.PartnerCode == Constants.Application.CardLinksPartnerCode)?.FirstOrDefault().RowId));
                }
            }

            if (!string.IsNullOrEmpty(memberNumber))
            {
                cardDetails = await _vredCardDetailsQueryProvider.GetVredCardDetails(memberNumber);
            }

            return MemberCardLinksMapper.MapToMemberCardDetails(cardDetails, partnerProfileDetails, partnerMembershipList);
        }
    }
}
