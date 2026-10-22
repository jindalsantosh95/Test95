using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class LeaderInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FCNumber { get; set; }
        public string TotalPoints { get; set; }
        public string TierCode { get; set; }

        public static explicit operator LeaderInfo((CustomerCustMS source, string tierCode) input)
        {
            var source = input.source;
            var tierCode = input.tierCode;
            return new LeaderInfo()
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                FCNumber = source.LoyaltyNumber,
                TotalPoints = "500",//Added a mock Response in future sprint we have a story to map the response
                TierCode = tierCode
            };

        }
    }

}
