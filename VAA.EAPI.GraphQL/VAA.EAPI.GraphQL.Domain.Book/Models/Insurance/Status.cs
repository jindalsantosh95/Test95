using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance
{
    [GraphQLName("InsuranceStatus")]
    public class Status
    {
        public string Code { get; set; }
        public string ProcessId { get; set; }
        public string Version { get; set; }
        public string ErrorText { get; set; }
        public string Message { get; set; }
        public string Reason { get; set; }
    }
}