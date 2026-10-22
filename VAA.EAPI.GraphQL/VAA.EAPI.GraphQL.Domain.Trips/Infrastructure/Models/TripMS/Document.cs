namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
{
    public class Document
    {
        public string DocumentType { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public CountryDetails IssuingCountry { get; set; }
        public CountryDetails Nationality { get; set; }
        public CountryOfResidence CountryOfResidence { get; set; }

    }
}
