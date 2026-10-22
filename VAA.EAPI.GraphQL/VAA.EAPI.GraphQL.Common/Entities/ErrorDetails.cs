namespace VAA.EAPI.GraphQL.Common.Entities
{
    public class ErrorDetails
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorTag { get; set; }
        public string? Title { get; set; }
    }
}