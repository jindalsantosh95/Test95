using System.ComponentModel.DataAnnotations;
using System.Globalization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using PassengerIdHelper = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class SavePassportRequest
    {
        [Required]
        public string RequirementsId { get; set; }
        [Required]
        public string PassengerId { get; set; }
        public Document Document { get; set; }
        public bool IsScanned { get; set; }

        public static explicit operator RequirementFormRequest(SavePassportRequest source)
        {
            var passengerId = PassengerIdHelper.FromTravelRequirements(source.PassengerId);

            return new RequirementFormRequest()
            {
                PassengerResourceId = passengerId.TrmPassengerResourceId,
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
                    IssuingCountry = new Country
                    {
                        Code = source.Document.IssuingCountry.Code.Is("D") ? "DEU" : source.Document.IssuingCountry.Code,
                    },
                    Nationality = new Country
                    {
                        Code = source.Document.Nationality.Code.Is("D") ? "DEU" : source.Document.Nationality.Code,
                    },
                    CountryOfResidence = new Country
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
    }
}
