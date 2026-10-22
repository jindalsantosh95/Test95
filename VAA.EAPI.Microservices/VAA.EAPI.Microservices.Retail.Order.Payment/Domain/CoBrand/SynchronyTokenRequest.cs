using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand
{
    public class SynchronyTokenRequest
    {
        public Customer ApplicantDetails { get; set; }
        public string AccountInfo { get; set; }
        public string InitialPurchaseAmount { get; set; }
    }
}
