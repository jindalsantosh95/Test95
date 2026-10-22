namespace VAA.EAPI.NDC.Order.Infrastructure.Model
{
    public class CorporateAccount
    {
        public string AccountName { get; set; }
        public string AgencyID { get; set; }
        public string CorporateCode { get; set; }
        public string TicketDesignator { get; set; }
        public bool ActiveContract { get; set; }
    }
}
