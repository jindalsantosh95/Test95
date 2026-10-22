namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class CustomerTierEntity
    {
        public string CustomerId { get; set; }
        public string OldTier { get; set; }
        public string NewTier { get; set; }
    }
}
