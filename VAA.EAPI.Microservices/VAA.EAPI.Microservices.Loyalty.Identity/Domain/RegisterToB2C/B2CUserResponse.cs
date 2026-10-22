namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C
{
    public class B2CUserResponse
    {
        public string Id { get; set; }
        public bool IsB2COperationSuccess { get; set; }
        public string B2CFaliureMessage { get; set; }
    }
}
