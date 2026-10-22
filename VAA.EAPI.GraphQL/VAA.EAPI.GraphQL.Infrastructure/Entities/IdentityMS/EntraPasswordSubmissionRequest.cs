namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class EntraPasswordSubmissionRequest
    {
        public string ContinuationToken { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public static explicit operator EntraPasswordSubmissionRequest(NewPasswordSubmissionRequest request)
        {
            return new EntraPasswordSubmissionRequest()
            {
                ContinuationToken = request?.Token,
                Password = request?.NewPassword,
                Email = request?.Email
            };
        }
    }
}
