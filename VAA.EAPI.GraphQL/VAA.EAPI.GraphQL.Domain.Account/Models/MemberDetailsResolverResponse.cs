using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class MemberDetailsResolverResponse
    {
        public FlightDetailsResponse FlightDetails { get; set; }
        public VoucherResponse VoucherResponse { get; set; }
        public MemberVoucherResponse MemberVoucherResponse { get; set; }
        public ActivityResponse ActivityResponse { get; set; }
        public FlyingClubMemberDetails flyingClubMemberDetails { get; set; }
        public int VouchersCount { get; set; }
        public int ActivitiesCount { get; set; }
        public int FlightsCount { get; set; }
    }
}
