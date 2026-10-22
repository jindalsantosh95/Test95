using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.MyAccount.Models.Enrolment
{
    public class OtpRequestTableEntity : TableEntity
    {
        public int OTP { get; set; }
        public int OTPGenerationCount { get; set; }
        public int OTPValidationCount { get; set; }
        public string OTPExpiryTime { get; set; }
    }
}
