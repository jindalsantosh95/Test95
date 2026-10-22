namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount
{
    public class VRedMemberDetails
    {
        public string PartnerUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public string Channel { get; set; }
        public string Operation { get; set; }
        public string RequestId { get; set; }
        public string AccountStatus { get; set; }
        public string HouseholdUserId { get; set; }
        public string CreateUpdateTimestamp { get; set; }
    }
}
