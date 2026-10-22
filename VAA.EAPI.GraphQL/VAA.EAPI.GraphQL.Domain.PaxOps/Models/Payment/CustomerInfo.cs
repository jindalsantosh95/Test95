namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment
{
    public class CustomerInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public Contacts Contacts { get; set; }
    }
}
