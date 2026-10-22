using HotChocolate;
using NodaTime;
using NodaTime.Text;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Document")]
    public class Document
    {
        public DocumentType DocumentType { get; set; }

        [GraphQLDescription("Given name(s) exactly as written on the document using Latin characters.")]
        public string GivenNames { get; set; }

        [GraphQLDescription("Surname exactly as written on the document using Latin characters.")]
        public string Surname { get; set; }

        [GraphQLDescription("Deprecated string date field. Prefer birthDate.")]
        [GraphQLDeprecated("Use birthDate")]
        public string DateOfBirth { get; set; }

        [GraphQLDescription("Document date of birth in ISO yyyy-MM-dd format.")]
        public LocalDate? BirthDate { get; set; }

        [GraphQLDescription("Gender code from the document.")]
        public Gender Gender { get; set; }

        [GraphQLDescription("Document number. Treat as sensitive personal data.")]
        public string DocumentNumber { get; set; }

        [GraphQLDescription("Deprecated string expiry field. Prefer expiryDate.")]
        [GraphQLDeprecated("Use expiryDate")]
        public string ExpirationDate { get; set; }

        [GraphQLDescription("Document expiry date in ISO yyyy-MM-dd format.")]
        public LocalDate? ExpiryDate { get; set; }

        [GraphQLDescription("Passport/document issuing country.")]
        public IssuingCountry IssuingCountry { get; set; }

        [GraphQLDescription("Nationality details from the document.")]
        public Nationality Nationality { get; set; }

        [GraphQLDescription("Country of residence details from the document.")]
        public CountryOfResidence CountryOfResidence { get; set; }

        public static explicit operator Document(Infrastructure.Models.CheckInMS.Document source) => new()
        {
            DocumentType = source.DocumentType,
            GivenNames = source.GivenNames,
            Surname = source.Surname,
            DateOfBirth = source.DateOfBirth,
            BirthDate = source.DateOfBirth != null ? LocalDatePattern.Iso.Parse(source.DateOfBirth).Value : null,
            Gender = source.Gender != null ? (Gender)source.Gender : null,
            DocumentNumber = source.DocumentNumber,
            ExpirationDate = source.ExpirationDate,
            ExpiryDate = source.ExpirationDate != null ? LocalDatePattern.Iso.Parse(source.ExpirationDate).Value : null,
            IssuingCountry = source.IssuingCountry != null ? (IssuingCountry)source.IssuingCountry : null,
            Nationality = source.Nationality != null ? (Nationality)source.Nationality : null,
            CountryOfResidence = source.CountryOfResidence != null ? (CountryOfResidence)source.CountryOfResidence : null,
        };
    }
}
