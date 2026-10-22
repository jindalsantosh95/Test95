using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class EnrolmentApiRequest
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }
        public string DataContentType { get; set; }
        public CreateCustomerAccountRequest Data { get; set; }
    }
}
