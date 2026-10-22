namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class Insured
    {
        public int InsuredSequence { get; set; }
        public string InsuredType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthdate { get; set; }
        public string PhoneNumber { get; set; }
        public double TripCost { get; set; }
    }
}