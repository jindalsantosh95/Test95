namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? ProfilePassenger { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
        public string Age { get; set; }
        public string Id { get; set; }
        public FQTMembership MembershipDetails { get; set; }
    }
}