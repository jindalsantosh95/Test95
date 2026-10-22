using System.IO;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Error
    {
        public Error(string code = default, string message = default, string developerMessage = default, string errorSource = default)
        {
            // to ensure "code" is required (not null)
            if (code == null)
            {
                throw new InvalidDataException("code is a required property for Error and cannot be null");
            }
            else
            {
                Code = code;
            }

            // to ensure "message" is required (not null)
            if (message == null)
            {
                throw new InvalidDataException("message is a required property for Error and cannot be null");
            }
            else
            {
                Message = message;
            }

            DeveloperMessage = developerMessage;
            ErrorSource = errorSource;
        }

        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
    }
}
