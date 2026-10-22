namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public sealed class Voucher
    {
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; } = Constants.Application.InvalidStatus;
    }

}
