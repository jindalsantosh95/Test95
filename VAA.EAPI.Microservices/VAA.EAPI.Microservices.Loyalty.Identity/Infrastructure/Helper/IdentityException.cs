using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper
{
    public class IdentityException : Exception
    {
        public IdentityException() { }
        public IdentityException(string code) : base(code) { }
        public IdentityException(string code, Exception innerException) : base(code, innerException) { }
    }
}
