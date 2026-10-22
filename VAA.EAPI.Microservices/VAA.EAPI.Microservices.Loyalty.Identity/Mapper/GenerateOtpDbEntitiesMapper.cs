using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class GenerateOtpDbEntitiesMapper
    {
        public static GenerateOtpDbEntities MapToDBEntityList(string emailId, int generatedOTP, int OTPGenerationCount, int OTPValidationCount)
        {
            return new GenerateOtpDbEntities()
            {
                RowKey = emailId.ToLower(),
                PartitionKey = Constants.Application.OtpGenerationEvent,
                OTP = generatedOTP,
                OTPGenerationCount = OTPGenerationCount,
                OTPValidationCount = OTPValidationCount,
                OTPExpiryTime = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OTPExpiryTimeLimit)).ToString(Constants.Application.ISODateTimeFormat)
            };
        }
    }
}
