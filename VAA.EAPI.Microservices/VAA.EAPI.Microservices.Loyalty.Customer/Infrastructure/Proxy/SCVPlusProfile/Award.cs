namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Award
    {
        public Award(string awardId = default, string awardNumber = default, string productId = default, string productName = default, string awardValue = default, string startDate = default, string expiryDate = default)
        {
            AwardId = awardId;
            AwardNumber = awardNumber;
            ProductId = productId;
            ProductName = productName;
            AwardValue = awardValue;
            StartDate = startDate;
            ExpiryDate = expiryDate;
        }

        public string AwardId { get; set; }
        public string AwardNumber { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string AwardValue { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
    }
}
