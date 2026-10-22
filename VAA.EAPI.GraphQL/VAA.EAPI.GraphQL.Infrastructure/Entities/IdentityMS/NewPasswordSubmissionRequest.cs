namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class NewPasswordSubmissionRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
