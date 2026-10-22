using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities
{
    public class GenerateOtpDbEntities : TableEntity
    {
        public int OTP { get; set; }
        public int OTPGenerationCount { get; set; }
        public int OTPValidationCount { get; set; }
        public string OTPExpiryTime { get; set; }
    }
}
