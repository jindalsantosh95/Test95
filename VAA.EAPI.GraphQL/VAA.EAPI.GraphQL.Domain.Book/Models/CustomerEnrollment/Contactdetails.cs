namespace VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment
{
    public class Contactdetails
    {
        public IEnumerable<Emaildetail> EmailDetails { get; set; }
        public IEnumerable<Phonedetail> PhoneDetails { get; set; }
    }
}
