namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class InitiatePasswordResetRequest(PasswordResetStartRequest request)
    {
        public string Username { get; set; } = request.FcNumberOrEmail;
    }
}
