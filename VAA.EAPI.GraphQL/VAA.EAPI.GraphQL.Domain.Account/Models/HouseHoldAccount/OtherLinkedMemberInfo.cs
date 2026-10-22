using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class OtherLinkedMemberInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TotalPoints { get; set; }
        public string TierCode { get; set; }
        public bool IsYoungFlyer { get; set; }


        public static explicit operator OtherLinkedMemberInfo((CustomerDTO source, string tierCode, bool isYoungFlyer) input)
        {
            var source = input.source;
            var tierCode = input.tierCode;
            return new OtherLinkedMemberInfo()
            {
                FirstName = source.Customer.FirstName,
                LastName = source.Customer.LastName,
                TotalPoints = "500",
                TierCode = tierCode,
                IsYoungFlyer = input.isYoungFlyer
            };

        }


    }
}
