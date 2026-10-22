using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class HouseHoldAccountMapper
    {
        public static HashSet<string> GetManagedYoungFlyerCustomerIds(GroupDocumentDTO groupDocument)
        {
            ArgumentNullException.ThrowIfNull(groupDocument);

            return new HashSet<string>(
                (groupDocument.GroupDocuments?.FirstOrDefault()?.GroupHistory ?? Enumerable.Empty<GroupHistory>())
                    .Where(history => history.ExitDate == null && history.HoHManaged && !history.CustomerId.IsNullOrWhiteSpace())
                    .Select(history => history.CustomerId),
                StringComparer.OrdinalIgnoreCase);
        }

        public static RetrieveHHAccountResponse RetrieveHouseHoldAccountDetails(IEnumerable<CustomerDTO> customerResponseList, string tokenFcNumber, RetrieveHHAccountRequest retrieveHHAccountRequest, string GroupName, string leaderFcNumber, IEnumerable<FcMemberDetails> fcMemberDetails, VRedResponse vredResponse, IEnumerable<CustomerDTO> inviteeDetails, IEnumerable<FcMemberDetails> inviteeTierDetails, HashSet<string> youngFlyerCustomerIds)
        {
            youngFlyerCustomerIds ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var retrieveHHAccountResponse = new RetrieveHHAccountResponse();
            var leaderDoc = customerResponseList.Where(c => c.Customer.LoyaltyNumber == leaderFcNumber).First().Customer;

            retrieveHHAccountResponse.Name = $"{leaderDoc.FirstName}{" "}{leaderDoc.LastName}" == GroupName ? Constants.Application.DefaultHHAccountName : GroupName;
            retrieveHHAccountResponse.LeaderInfo = (LeaderInfo)(leaderDoc, fcMemberDetails.SelectMany(f => f.FlyingClubMemberDocuments).FirstOrDefault(fc => fc.MemberNumber == leaderFcNumber).TierCode);

            retrieveHHAccountResponse.LinkedMemberInfo = GetLinkedMemberInfo(customerResponseList, tokenFcNumber, retrieveHHAccountRequest, fcMemberDetails, youngFlyerCustomerIds);

            if (!string.IsNullOrEmpty(retrieveHHAccountRequest.HouseHoldFcNumber) && !(Convert.ToString(retrieveHHAccountRequest.MemberType) == Constants.Application.HouseHoldAccount))
            {
                var otherLinkedMemberInfo = customerResponseList.ToList().Where(res => res.Customer.LoyaltyNumber != tokenFcNumber && res.Customer.LoyaltyNumber != retrieveHHAccountRequest.HouseHoldFcNumber);

                retrieveHHAccountResponse.OtherLinkedMemberInfo = otherLinkedMemberInfo.Select(c => (OtherLinkedMemberInfo)(c, fcMemberDetails.SelectMany(f => f.FlyingClubMemberDocuments).FirstOrDefault(x => x.MemberNumber == c.Customer.LoyaltyNumber).TierCode, !c.Customer.CustomerId.IsNullOrWhiteSpace() && youngFlyerCustomerIds.Contains(c.Customer.CustomerId)));
            }

            retrieveHHAccountResponse.TotalPoints = vredResponse.Current;
            retrieveHHAccountResponse.PageResponse = PageResponseType.HHA_DASHBOARD;

            var inviteesresponse = inviteeDetails.Select(c => (InviteeInfo)(c, inviteeTierDetails.SelectMany(f => f.FlyingClubMemberDocuments).First(x => x.MemberNumber == c.Customer.LoyaltyNumber).TierCode));
            retrieveHHAccountResponse.InviteeInfo = inviteesresponse.Any() ? inviteesresponse : null;

            if (retrieveHHAccountRequest.MemberType == Models.ManageAccount.HouseHoldMemberType.Household)
                retrieveHHAccountResponse.TotalMemberandInviteesCount = inviteesresponse.Count() + retrieveHHAccountResponse.LinkedMemberInfo.Count() + VAA.EAPI.GraphQL.Common.Constants.Application.HouseHoldLeaderCount;
            else
                retrieveHHAccountResponse.TotalMemberandInviteesCount = retrieveHHAccountResponse.LinkedMemberInfo.Count() + retrieveHHAccountResponse.OtherLinkedMemberInfo.Count() + VAA.EAPI.GraphQL.Common.Constants.Application.HouseHoldLeaderCount;

            return retrieveHHAccountResponse;
        }

        private static IEnumerable<LinkedMemberInfo> GetLinkedMemberInfo(IEnumerable<CustomerDTO> customerResponse, string tokenFcNumber, RetrieveHHAccountRequest retrieveHHAccountRequest, IEnumerable<FcMemberDetails> fcMemberDetails, HashSet<string> youngFlyerCustomerIds)
        {
            IEnumerable<CustomerDTO> customers = null;

            if (Convert.ToString(retrieveHHAccountRequest.MemberType) == Constants.Application.LinkedAccount)
            {
                customers = customerResponse.Where(c => c.Customer.LoyaltyNumber == tokenFcNumber);
            }

            if (Convert.ToString(retrieveHHAccountRequest.MemberType) == Constants.Application.HouseHoldAccount && customerResponse != null)
            {
                customers = customerResponse.Where(c => c.Customer.LoyaltyNumber != tokenFcNumber);
            }
            return customers.Select(c => (LinkedMemberInfo)(c, fcMemberDetails.SelectMany(f => f.FlyingClubMemberDocuments).FirstOrDefault(x => x.MemberNumber == c.Customer.LoyaltyNumber).TierCode, !c.Customer.CustomerId.IsNullOrWhiteSpace() && youngFlyerCustomerIds.Contains(c.Customer.CustomerId)));
        }

    }
}
