namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public sealed class Membership
    {
        public string MembershipNumber { get; set; }
        public string MemberSurname { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; } = Constants.Application.InvalidStatus;
        public string ValidateLink { get; set; }
    }
}
