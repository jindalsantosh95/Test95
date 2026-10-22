using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile
{
    public class UpdatePhoneNumberNotificationEventRequest
    {
        public string HOHCustomerId { get; set; }
        public string FcNumber {  get; set; }
        public string GroupID { get; set; }
        public Phone Phone { get; set; }
        public string InitiatedBy { get; set; }
    }
}
