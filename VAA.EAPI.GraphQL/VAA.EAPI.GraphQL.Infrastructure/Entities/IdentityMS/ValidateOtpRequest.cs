namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class ValidateOtpRequest
    {
        public string ContinuationToken { get; set; }
        public string OtpCode { get; set; }
        public string UrlEntraJourney { get; set; }

        public static explicit operator ValidateOtpRequest(EntraOtpValidationRequest request)
        {
            return new ValidateOtpRequest()
            {
                ContinuationToken = request?.Token,
                OtpCode = request?.OTP,
                UrlEntraJourney = request.ValidationRequestType.ToString().ToLowerInvariant()
            };
        }
    }
}
