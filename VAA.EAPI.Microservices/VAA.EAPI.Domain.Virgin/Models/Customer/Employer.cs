namespace VAA.EAPI.Domain.Virgin.Models.Customer
{
    public class Employer
    {
        public string Name { get; set; }
        public SalesTaxRegistration SalesTaxRegistration { get; set; }
        public bool TaxDeductedAtSourceCapabilityInd { get; set; }

    }
}