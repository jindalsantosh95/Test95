namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class TierEarnPoints
    {
        public string TierType { get; set; }   // RED, SILVER, GOLD
        public int VirginPoints { get; set; }
        public int TierPoints { get; set; }
    }
}
