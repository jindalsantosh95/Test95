using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class IndividualHouseHoldResponseMapper
    {
        public static RetrieveHHAccountResponse ResponseWithNoInvites(CustomerCustMS customerDetails, FcMemberDetails customerTierDetails)
        {
            var retrieveHHAccountResponse = new RetrieveHHAccountResponse();

            if (customerDetails.DateOfBirth.AddYears(18) > DateTime.Today)

                retrieveHHAccountResponse.PageResponse = PageResponseType.EXPLAINER_PAGE_WITHOUT_BUTTON;

            else if (customerTierDetails.FlyingClubMemberDocuments.First().TierCode.Is(Common.Constants.Application.SilverTier) || customerTierDetails.FlyingClubMemberDocuments.First().TierCode.Is(Common.Constants.Application.GoldTier))

                retrieveHHAccountResponse.PageResponse = PageResponseType.EXPLAINER_PAGE_WITH_BUTTON;

            return retrieveHHAccountResponse;
        }

        public static RetrieveHHAccountResponse ResponseWithInvitesforInvitee(CustomerCustMS inviterDetails, string groupHouseHoldName)
        {
            var retrieveHHAccountResponse = new RetrieveHHAccountResponse
            {
                Name = groupHouseHoldName,
                PageResponse = PageResponseType.ACCEPT_DECLINE,
                InviterInfo = (InviterInfo)inviterDetails
            };

            return retrieveHHAccountResponse;
        }

        public static RetrieveHHAccountResponse ResponseWithInvitesforInviter(List<CustomerDTO> inviteesDetailsResponse, List<FcMemberDetails> inviteeTierDetailsResponse, string groupHouseHoldName)
        {
            var retrieveHHAccountResponse = new RetrieveHHAccountResponse
            {
                Name = groupHouseHoldName,
                PageResponse = PageResponseType.RESEND_CANCEL,
                InviteeInfo = inviteesDetailsResponse.Select(c => (InviteeInfo)(c, inviteeTierDetailsResponse.SelectMany(f => f.FlyingClubMemberDocuments).FirstOrDefault(x => x.MemberNumber == c.Customer.LoyaltyNumber).TierCode))
            };

            return retrieveHHAccountResponse;
        }
    }
}
