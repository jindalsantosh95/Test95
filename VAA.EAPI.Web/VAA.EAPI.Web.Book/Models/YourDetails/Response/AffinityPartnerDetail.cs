namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class AffinityPartnerDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string IdentifierDisplayName { get; set; }
        public AffinityMemberDetails AffinityMemberDetails { get; set; }
    }
}
