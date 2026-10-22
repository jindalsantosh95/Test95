namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class MemberAttribute
    {
        public MemberAttribute(int? totalVSFlownMiles = default, string pointsComEligibleFlag = default)
        {
            TotalVSFlownMiles = totalVSFlownMiles;
            PointsComEligibleFlag = pointsComEligibleFlag;
        }

        public int? TotalVSFlownMiles { get; set; }
        public string PointsComEligibleFlag { get; set; }
    }
}
