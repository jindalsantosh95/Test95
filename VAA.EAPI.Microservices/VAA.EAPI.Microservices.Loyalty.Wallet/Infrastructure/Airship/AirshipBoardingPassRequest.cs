namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class AirshipBoardingPassRequest
    {
        public string IosTemplateId { get; set; }
        public string AndroidTemplateId { get; set; }
        public WalletPayload Payload { get; set; }
    }
}
