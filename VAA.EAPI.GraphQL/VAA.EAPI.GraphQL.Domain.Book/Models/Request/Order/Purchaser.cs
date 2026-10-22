namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class Purchaser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public int FiscalId { get; set; }
    }
}