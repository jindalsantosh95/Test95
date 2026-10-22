using System.ComponentModel.DataAnnotations;
using System.Globalization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class SavePassportRequest
    {
        [Required]
        public string TravelPartyId { get; set; }
        [Required]
        public string RequirementsId { get; set; }
        public Document Document { get; set; }
        public string MRZLine1 { get; set; }
        public string MRZLine2 { get; set; }
        public bool IsScanned { get; set; }
        [Required]
        public string PassengerId { get; set; }

        public static explicit operator RequirementFormRequest(SavePassportRequest source)
        {
            return new RequirementFormRequest()
            {
                PassengerResourceId = source.PassengerId.Split("|")[3],
                Using = RequirementType.PASSPORT,
                IdentityDocument = new IdentityDocument
                {
                    Type = IdentityDocumentType.PASSPORT,
                    GivenNames = source.Document.GivenNames,
                    Surname = source.Document.Surname,
                    Gender = new DeltaTrm.Gender
                    {
                        Code = source.Document.Gender.Code,
                    },
                    BirthDate = source.Document.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Number = source.Document.DocumentNumber,
                    ExpirationDate = source.Document.ExpirationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    IssuingCountry = new DeltaTrm.Country
                    {
                        Code = source.Document.IssuingCountry.Code.Is("D") ? "DEU" : source.Document.IssuingCountry.Code,
                    },
                    Nationality = new DeltaTrm.Country
                    {
                        Code = source.Document.Nationality.Code.Is("D") ? "DEU" : source.Document.Nationality.Code,
                    },
                    CountryOfResidence = new DeltaTrm.Country
                    {
                        Code = source.Document.CountryOfResidence.Code.Is("D") ? "DEU" : source.Document.CountryOfResidence.Code,
                    },
                    DataSourceType = source.IsScanned ? DataSourceCodeType.MACHINE_READABLE_ZONE : DataSourceCodeType.MANUAL
                },
                Consent = true,
                Modified = true,
                SaveToProfile = false
            };
        }

        public static explicit operator AddPassportRequest(SavePassportRequest source)
        {
            var passengerId = new PassengerId(source.PassengerId);

            return new AddPassportRequest()
            {
                TravelPartyId = source.TravelPartyId,
                FirstNIN = passengerId.FirstNin,
                LastNIN = passengerId.LastNin,
                PassengerNINs = passengerId.Nins,
                DocumentType = "P", // ICAO code meaning Passport
                VerifiedByDelta = source.IsScanned,
                PrimaryDocumentProvidedFlag = true, // Delta tell us this has no effect, but suggested we pass as true when saving passport details.
                DocumentFirstName = source.Document.GivenNames,
                DocumentLastName = source.Document.Surname,
                // German passports are issued with the country code D, but the Delta APIs use DEU.
                DocumentNationality = source.Document.Nationality.Code.Is("D") ? "DEU" : source.Document.Nationality.Code,
                DocumentGender = source.Document.Gender.Code,
                DocumentBirthYear = source.Document.DateOfBirth?.ToString("yyyy", CultureInfo.InvariantCulture),
                DocumentBirthMonth = source.Document.DateOfBirth?.ToString("MM", CultureInfo.InvariantCulture),
                DocumentBirthDay = source.Document.DateOfBirth?.ToString("dd", CultureInfo.InvariantCulture),
                DocumentCountryOfResidence = source.Document.CountryOfResidence.Code.Is("D") ? "DEU" : source.Document.CountryOfResidence.Code,
                DocumentNumber = source.Document.DocumentNumber,
                DocumentExpirationYear = source.Document.ExpirationDate?.ToString("yyyy", CultureInfo.InvariantCulture),
                DocumentExpirationMonth = source.Document.ExpirationDate?.ToString("MM", CultureInfo.InvariantCulture),
                DocumentExpirationDay = source.Document.ExpirationDate?.ToString("dd", CultureInfo.InvariantCulture),
                DocumentIssuingCountry = source.Document.IssuingCountry.Code.Is("D") ? "DEU" : source.Document.IssuingCountry.Code,
            };
        }
    }
}