using System;


namespace VAA.EAPI.Domain.Exceptions
{
    public class ExternalServiceIntegrationException : Exception
    {
        public ExternalServiceIntegrationException()
        {

        }

        public ExternalServiceIntegrationException(string code, string message) : base(code)
        {
        }


        public ExternalServiceIntegrationException(string code, Exception innerException) : base(code, innerException)
        {
        }
    }

    public class CosmosDBException : Exception
    {
        public CosmosDBException(string message) : base(message)
        {
        }
    }
    public class MessageBrokerException : Exception
    {
        public MessageBrokerException(string message) : base(message)
        {
        }
    }
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}
