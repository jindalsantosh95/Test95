using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper
{
    public static class updateVredRequestMapper
    {
        public static VRedMemberDetails ManageAccountRequestMapToVredRequest(UpdateVredMemberDetailsRequest updateVredMemberDetails)
        {
            return new VRedMemberDetails
            {
                AccountStatus = updateVredMemberDetails.MemberStatus,
                PartnerUserId = updateVredMemberDetails.MemberNumber,
                RequestId = updateVredMemberDetails.CustomerId,
                FirstName = updateVredMemberDetails.FirstName,
                LastName = updateVredMemberDetails.LastName,
                DOB = Common.Helpers.DateTimeHelper.ConvertToUtc(updateVredMemberDetails.DateOfBirth, Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat),
                Channel = updateVredMemberDetails.Channel,
                Email = updateVredMemberDetails.EmailId,
                Operation = Constants.Application.Operation,
                HouseholdUserId = !string.IsNullOrEmpty(updateVredMemberDetails.HouseholdUserId) ? updateVredMemberDetails.HouseholdUserId : string.Empty,
                CreateUpdateTimestamp = !string.IsNullOrEmpty(updateVredMemberDetails.CreateUpdateTimestamp) ? updateVredMemberDetails.CreateUpdateTimestamp : string.Empty
            };
        }
    }
}
