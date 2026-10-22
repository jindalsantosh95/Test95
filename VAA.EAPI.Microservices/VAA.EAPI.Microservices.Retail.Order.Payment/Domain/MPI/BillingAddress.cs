namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI
{
    public class BillingAddress
    {
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }
    }
}
