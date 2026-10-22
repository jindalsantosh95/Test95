using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class Document
    {
        public DocumentType DocumentType { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string DocumentNumber { get; set; }
        public string ExpirationDate { get; set; }
        public IssuingCountry IssuingCountry { get; set; }
        public Nationality Nationality { get; set; }
        public CountryOfResidence CountryOfResidence { get; set; }

        public static explicit operator Document(PaxOps.Models.Document source)
        {
            if (source == null)
            {
                return null;
            }

            return new Document
            {
                DocumentType = source.DocumentType,
                GivenNames = source.GivenNames,
                Surname = source.Surname,
                DateOfBirth = source.DateOfBirth,
                Gender = source.Gender != null ? new Gender { Code = source.Gender.Code } : null,
                DocumentNumber = source.DocumentNumber,
                ExpirationDate = source.ExpirationDate,
                IssuingCountry = source.IssuingCountry != null ? new IssuingCountry { Code = source.IssuingCountry.Code } : null,
                Nationality = source.Nationality != null ? new Nationality { Code = source.Nationality.Code } : null,
                CountryOfResidence = source.CountryOfResidence != null ? new CountryOfResidence { Code = source.CountryOfResidence.Code } : null
            };
        }
    }
}
