namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class NDCParty
    {
        public string PartyType { get; set; }
        public string PartyAirlineCode { get; set; }
        public string IATANumber { get; set; }
        public string PartyLegalName { get; set; }
        public string AgencyID { get; set; }
        public string PseudoCityID { get; set; }

        public string Ordinal { get; set; }

    }
}
