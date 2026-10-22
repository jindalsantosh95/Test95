using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FlyingClubLoginRequest : AccountRequestContext
    {
        public string? MemberNumber { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
    }
}
