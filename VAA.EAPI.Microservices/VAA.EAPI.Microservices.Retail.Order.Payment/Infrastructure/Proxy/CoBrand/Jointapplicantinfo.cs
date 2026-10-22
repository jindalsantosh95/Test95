namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Jointapplicantinfo
    {
        public string CipherfirstName { get; set; }
        public string CipherlastName { get; set; }
        public string CiphermiddleInitial { get; set; }
        public string CipheremailAddress { get; set; }
        public string CiphersocialSecurity { get; set; }
        public string CipherdateOfBirth { get; set; }
        public int NetIncome { get; set; }
        public string ResidenceType { get; set; }
        public Contactinfo[] ContactInfo { get; set; }
        public Address Address { get; set; }
        public Membershipinfo MembershipInfo { get; set; }
        public string FinderNumber { get; set; }
        public string CipherinitialPurchaseAmount { get; set; }
        public string PaymentAmount { get; set; }
        public string ChargeLoanDate { get; set; }
        public string CustomerSplitPayAmount { get; set; }
    }
}
