namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
        public string ThirdPartyServiceErrorCode { get; set; }
    }

}
