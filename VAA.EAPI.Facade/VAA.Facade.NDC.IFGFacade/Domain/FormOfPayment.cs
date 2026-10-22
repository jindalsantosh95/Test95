namespace VAA.Facade.NDC.IFGFacade.Domain
{
    public class FormOfPayment
    {
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public Payer Payer { get; set; }
        public string ExpiryDate { get; set; }
        public string AccountNumber { get; set; }
        public string CardSecurityCode { get; set; }
        public Address Address { get; set; }
        public bool MotoPayment { get; set; }
        public CardAuthentication CardAuthentication { get; set; }
        public System.Collections.Generic.IEnumerable<MerchantDefinedInformation> MerchantDefinedInformation { get; set; }

    }
}
