namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class Errors
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
        public string ThirdPartyServiceErrorCode { get; set; }
    }

}
