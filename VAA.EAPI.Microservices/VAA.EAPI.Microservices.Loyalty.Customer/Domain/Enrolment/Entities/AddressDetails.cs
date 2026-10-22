namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Entities
{
    public class AddressDetails
    {
        public string AddressType { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string CompanyName { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Region { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string PreferredFlag { get; set; }

        public bool? AddressVerified { get; set; }
        public string MetroArea { get; set; }
    }
}
