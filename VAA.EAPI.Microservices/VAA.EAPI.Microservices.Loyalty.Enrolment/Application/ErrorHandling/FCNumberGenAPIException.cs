using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.ErrorHandling
{
    public class FCNumberGenAPIException : Exception
    {
        public FCNumberGenAPIException()
        {

        }

        public FCNumberGenAPIException(string code) : base(code)
        {
        }


        public FCNumberGenAPIException(string code, Exception innerException) : base(code, innerException)
        {
        }

    }
}
