namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class EntraOtpValidationRequest
    {
        public string Token { get; set; }
        public string OTP { get; set; }
        public OtpValidationRequestType ValidationRequestType { get; set; }
    }
}
