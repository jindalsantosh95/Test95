using VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment.CoBrand
{
    public class SynchronyTokenMSRequest
    {
        public CustomerInfo ApplicantDetails { get; set; }
        public string AccountInfo { get; set; }
        public string InitialPurchaseAmount { get; set; }
    }
}
