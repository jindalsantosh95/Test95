namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate
{
    public class AddressHistoryCreateEventData
    {
        public string PartitionKey { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string PostalStatus { get; set; }
        public string AddressType { get; set; }
        public string City { get; set; }
        public string AdditionalComments { get; set; }
        public string Country { get; set; }
        public string GeoCodeValidFlag { get; set; }
        public string LandlordOrCompanyName { get; set; }
        public string MetroArea { get; set; }
        public string Valid { get; set; }
        public string Province { get; set; }
        public string AddressVerified { get; set; }
        public string AddressVerificationStatus { get; set; }
        public string PostCode { get; set; }
        public string GroupAddress { get; set; }
        public string PartnerFileId { get; set; }
        public string Primary { get; set; }
    }
}
