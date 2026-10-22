using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class MemberAccount
    {
        [GraphQLDescription("The Customer ID")]
        public string CustomerId { get; set; }
        [GraphQLDescription("By Date of Member")]
        public string ByDate { get; set; }
        [GraphQLDescription("Retain Point details")]
        public string RetainPointTotal { get; set; }
        [GraphQLDescription("Rolling Point details")]
        public string RollingPointTotal { get; set; }
        [GraphQLDescription("Total VS Flown Miles details")]
        public string TotalVsflownMiles { get; set; }
        [GraphQLDescription("Tier Start Date")]
        public string TierStartDate { get; set; }
        [GraphQLDescription("Tier End Date")]
        public string TierEndDate { get; set; }
        [GraphQLDescription("Number of Years at Gold")]
        public string NumberofYearsAtGold { get; set; }
        [GraphQLDescription("Total VS Tier Points in Current Year")]
        public string TotalVSTierPointsInCurrentYear { get; set; }
        [GraphQLDescription("Total VS Tier Points Since Becoming Gold")]
        public string TotalVSTierPointsSinceBecomingGold { get; set; }
        [GraphQLDescription("Tier Points For Retain To Gold")]
        public string TierPointsForRetainToGold { get; set; }
        [GraphQLDescription("Tier Points For Retain To Silver")]
        public string TierPointsForRetainToSilver { get; set; }
        [GraphQLDescription("Tier Points For Red To Silver")]
        public string TierPointsForRedToSilver { get; set; }
        [GraphQLDescription("Tier Points For Silver To Gold")]
        public string TierPointsForSilverToGold { get; set; }
        [GraphQLDescription("Wings Start Date")]
        public string WingsStartDate { get; set; }
        [GraphQLDescription("Total Tier Points Since Becoming Wings")]
        public string TotalTierPointsSinceBecomingWings { get; set; }
        [GraphQLDescription("Total VS Tier Points Since Becoming Wings")]
        public string TotalVSTierPointsSinceBecomingWings { get; set; }
        public string GoldStartDate { get; set; }
        public MemberCustomAttributes MemberCustomAttributes { get; set; }
        public string RetainCoreTierPoints { get; set; }
        public string RollingCoreTierPoints { get; set; }
        public string CoreTierPointsSinceBecomingWings { get; set; }
    }
}
