namespace VAA.Facade.AzureB2C.Domain.Entities
{
    public class FailureResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
    }
}
