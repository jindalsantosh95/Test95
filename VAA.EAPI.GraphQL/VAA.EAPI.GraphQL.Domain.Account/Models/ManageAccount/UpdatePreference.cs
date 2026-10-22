namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class UpdatePreference
    {
        public string FcNumber { get; set; }
        public string CustomerId { get; set; }
        public IEnumerable<Preference> Preference { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
