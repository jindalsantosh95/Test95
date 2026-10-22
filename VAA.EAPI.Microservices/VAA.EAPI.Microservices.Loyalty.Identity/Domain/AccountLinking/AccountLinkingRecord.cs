namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class AccountLinkingRecord
    {
        public string FlyingClubAccountNumber { get; set; }
        public string TransactionId { get; set; }
        public string TransactionBindingHash { get; set; }
        public string ConsentReference { get; set; }
    }
}
