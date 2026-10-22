using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class InviteeInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FCNumber { get; set; }
        public string TierCode { get; set; }

        public static explicit operator InviteeInfo((CustomerDTO source, string tierCode) input)
        {
            var source = input.source;
            var tierCode = input.tierCode;

            return new InviteeInfo()
            {
                FirstName = source.Customer.FirstName,
                LastName = source.Customer.LastName,
                FCNumber = source.Customer.LoyaltyNumber,
                TierCode = tierCode
            };
        }
    }
}
