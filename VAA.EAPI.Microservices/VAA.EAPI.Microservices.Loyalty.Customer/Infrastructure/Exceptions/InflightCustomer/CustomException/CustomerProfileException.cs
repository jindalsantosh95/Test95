using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Exceptions.InflightCustomer.CustomException
{
    public class CustomerProfileException : Exception
    {
        public CustomerProfileException()
        {

        }
        public CustomerProfileException(string code) : base(code)
        {
        }
        public CustomerProfileException(string code, Exception innerException) : base(code, innerException)
        {
        }

    }
}
