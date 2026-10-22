using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class LinkedMemberInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FCNumber { get; set; }
        public string TotalPoints { get; set; }
        public string TierCode { get; set; }
        public bool IsYoungFlyer { get; set; }

        public static explicit operator LinkedMemberInfo((CustomerDTO source, string tierCode, bool isYoungFlyer) input)
        {
            var source = input.source;
            var tierCode = input.tierCode;

            return new LinkedMemberInfo()
            {
                FirstName = source.Customer.FirstName,
                LastName = source.Customer.LastName,
                FCNumber = source.Customer.LoyaltyNumber,
                TotalPoints = "500", // Added a mock Response in future sprint we have a story to map the response
                TierCode = tierCode,
                IsYoungFlyer = input.isYoungFlyer
            };
        }
    }
}
