using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Helper
{
    public static class PaymentHelper
    {
        public static string GeneratePaymentId(DateTime date)
        {
            return $"{(long)(date.ToUniversalTime() - Constants.Application.UnixEpochTime).TotalMilliseconds}-{Constants.Application.Random4DigitNum}";
        }
    }
}
