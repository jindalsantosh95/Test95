
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class LoyaltyMembership
    {
        public string? ProgrammeCode { get; set; }
        public string? Number { get; set; }
        public string? TierName { get; set; }

        public static explicit operator LoyaltyMembership(Infrastructure.Models.TripMS.LoyaltyMembership source)
        {
            if (source == null)
                return null;

            return new LoyaltyMembership()
            {
                ProgrammeCode = source.ProgrammeCode,
                Number = source.Number,
                TierName = source.TierName
            };
        }
    }
}