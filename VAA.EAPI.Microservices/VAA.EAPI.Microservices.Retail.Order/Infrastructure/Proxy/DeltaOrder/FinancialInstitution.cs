namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FinancialInstitution
    {
        public string AccountHolderName { get; set; }
        public string FinancialInstitutionProviderId { get; set; }
        public string FinancialInstitutionReturnUrl { get; set; }
        public FinancialInstitutionAddress FinancialInstitutionAddress { get; set; }
    }
}