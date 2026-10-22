using NodaTime;
using System.Globalization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class Document
    {
        public string? DocumentType { get; set; }
        public string? GivenNames { get; set; }
        public string? Surname { get; set; }
        public string? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public LocalDate? BirthDate { get; set; }
        public CountryDetails? IssuingCountry { get; set; }
        public CountryDetails? Nationality { get; set; }
        public CountryOfResidence? CountryOfResidence { get; set; }

        public static explicit operator Document(Infrastructure.Models.TripMS.Document source)
        {
            if (source == null) return null;

            return new Document()
            {
                DocumentType = source.DocumentType,
                GivenNames = source.GivenNames,
                Surname = source.Surname,
                DateOfBirth = source.DateOfBirth,
                DocumentNumber = source.DocumentNumber,
                ExpirationDate = source.ExpirationDate,
                Gender = (Gender)source.Gender,
                IssuingCountry = (CountryDetails)source.IssuingCountry,
                CountryOfResidence = (CountryOfResidence)source.CountryOfResidence,
                Nationality = (CountryDetails)source.Nationality,
                BirthDate = DateTime.TryParse(source.DateOfBirth, out var d) ? LocalDate.FromDateTime(d) : null
            };
        }
    }
}
