namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
    }
}
