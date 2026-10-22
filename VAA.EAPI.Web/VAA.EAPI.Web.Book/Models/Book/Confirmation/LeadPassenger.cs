namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class LeadPassenger
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Type { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public Address Address { get; set; }
        public Contact Contact { get; set; }
    }
}
