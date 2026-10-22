using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Exceptions.LoyaltyProfile.CustomException
{
    public class LoyaltyProfileException : Exception
    {
        public LoyaltyProfileException()
        {

        }
        public LoyaltyProfileException(string code) : base(code)
        {
        }
        public LoyaltyProfileException(string code, Exception innerException) : base(code, innerException)
        {
        }

    }
}
