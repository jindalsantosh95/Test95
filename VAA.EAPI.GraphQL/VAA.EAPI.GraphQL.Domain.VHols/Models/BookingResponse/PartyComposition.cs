namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class PartyComposition
    {
        public int? Adults { get; set; }
        public int? Children { get; set; }
        public int? Infant { get; set; }
        public int? Oap { get; set; }
        public int? Total { get; set; }
        public List<string> AdultsCustomerIds { get; set; }
        public List<string> ChildrenCustomerIds { get; set; }
        public List<string> InfantCustomerIds { get; set; }
        public List<string> OapCustomerIds { get; set; }
    }

}

