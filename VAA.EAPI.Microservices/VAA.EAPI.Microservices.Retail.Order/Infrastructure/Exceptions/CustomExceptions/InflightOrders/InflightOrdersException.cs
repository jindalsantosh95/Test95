using System;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Exceptions.CustomExceptions.InflightOrders
{
    public class InflightOrdersException : Exception
    {
        public InflightOrdersException()
        {

        }
        public InflightOrdersException(string code) : base(code)
        {
        }
        public InflightOrdersException(string code, Exception innerException) : base(code, innerException)
        {
        }

    }
}

