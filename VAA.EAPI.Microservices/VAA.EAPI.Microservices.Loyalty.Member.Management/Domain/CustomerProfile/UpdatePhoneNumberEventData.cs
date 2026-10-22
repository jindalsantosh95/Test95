namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile
{
    public class UpdatePhoneNumberEventData
    {
        public string CustomerId { get; set; } 
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string FcNumber { get; set; }     
        public string CorrelationId { get; set; }
    }
}
