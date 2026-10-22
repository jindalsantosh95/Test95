using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Document
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentType DocumentType { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        [Required]
        public LocalDate? DateOfBirth { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public string DocumentNumber { get; set; }
        [Required]
        public LocalDate? ExpirationDate { get; set; }
        [Required]
        public PassportCountry IssuingCountry { get; set; }
        [Required]
        public Nationality Nationality { get; set; }
        [Required]
        public PassportCountry CountryOfResidence { get; set; }

        public static explicit operator Document(DeltaTrm.IdentityDocument trmIdentityDocument)
        {
            var documentType = trmIdentityDocument.Type switch
            {
                DeltaTrm.IdentityDocumentType.PASSPORT => DocumentType.PASSPORT,
                DeltaTrm.IdentityDocumentType.PERMANENT_RESIDENT_CARD => DocumentType.PERMANENT_RESIDENT_CARD,
                DeltaTrm.IdentityDocumentType.NATIONAL_ID => DocumentType.NATIONAL_ID,
                DeltaTrm.IdentityDocumentType.NEXUS_CARD => DocumentType.NEXUS_CARD,
                DeltaTrm.IdentityDocumentType.VISA => DocumentType.VISA,
                _ => DocumentType.UNKNOWN,
            };

            return new()
            {
                DocumentType = documentType,
                GivenNames = trmIdentityDocument.GivenNames,
                Surname = trmIdentityDocument.Surname,
                DateOfBirth = DateTimeHelper.ParseLocalIsoDate(trmIdentityDocument.BirthDate),
                Gender = (Gender)trmIdentityDocument.Gender,
                DocumentNumber = trmIdentityDocument.Number,
                ExpirationDate = DateTimeHelper.ParseLocalIsoDate(trmIdentityDocument.ExpirationDate),
                IssuingCountry = (PassportCountry)trmIdentityDocument.IssuingCountry,
                Nationality = (Nationality)trmIdentityDocument.Nationality,
                CountryOfResidence = (PassportCountry)trmIdentityDocument.CountryOfResidence
            };
        }
    }
}
