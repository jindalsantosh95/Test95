using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class InviterInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FCNumber { get; set; }

        public static explicit operator InviterInfo(CustomerCustMS source)
        {
            return new InviterInfo()
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                FCNumber = source.LoyaltyNumber,
            };
        }

    }
}
