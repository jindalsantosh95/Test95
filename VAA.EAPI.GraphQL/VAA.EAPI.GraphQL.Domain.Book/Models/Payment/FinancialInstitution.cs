namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class FinancialInstitution
    {
        public string AccountHolderName { get; set; }
        public string ProviderId { get; set; }
        public string ReturnUrl { get; set; }
        public Customer.Address Address { get; set; }
    }
}