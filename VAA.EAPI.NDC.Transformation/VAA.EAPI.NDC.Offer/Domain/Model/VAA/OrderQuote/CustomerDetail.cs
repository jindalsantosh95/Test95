namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote
{
    public class CustomerDetail
    {
        public string Title { get; set; }

        public string CustId { get; set; }

        public string Ptc { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public Contacts Contacts { get; set; }

        public Addresses Addresses { get; set; }

    }
}
