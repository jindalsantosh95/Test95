namespace VAA.EAPI.Microservices.Security.Infrastructure.Exception
{
    public class SecurityException : System.Exception
    {
        public SecurityException()
        {
        }
        public SecurityException(string code) : base(code)
        {
        }
        public SecurityException(string code, System.Exception innerException) : base(code, innerException)
        {
        }
    }
}
