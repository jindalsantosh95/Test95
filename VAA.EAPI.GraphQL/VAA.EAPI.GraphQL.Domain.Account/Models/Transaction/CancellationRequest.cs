using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class CancellationRequest
    {
        public string TransactionId { get; set; }
        public string CancellationReason { get; set; }
        public string MemberNumber { get; set; }
        public string ActivityDate { get; set; }
        public string TransactionChannel { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public static explicit operator CancellationRequest(Detail vredStatmentDetails)
        {
            return new()
            {
                TransactionId = vredStatmentDetails.TransactionId,
                ActivityDate = vredStatmentDetails.ActivityDatetime?.ToString(Common.Constants.Application.FEAPIDateFormat),
                MemberNumber = vredStatmentDetails.MembershipNumber,
                TransactionChannel = vredStatmentDetails.TransactionChannelId,
                CreatedBy = Common.Constants.Application.CreatedUser,
                UpdatedBy = Common.Constants.Application.CreatedUser
            };
        }
    }
}
