namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class Award
    {
        public string AwardId { get; set; }
        public string AwardNumber { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string AwardValue { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
    }
}
