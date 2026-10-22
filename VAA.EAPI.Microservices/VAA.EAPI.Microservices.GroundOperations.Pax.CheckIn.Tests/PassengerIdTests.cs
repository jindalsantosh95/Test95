using NodaTime;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using Xunit;
using ReturnDate = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ReturnDate;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Tests
{
    public sealed class PassengerIdTests
    {
        private const string CheckInPassengerId = "01.02|ssci-passenger-id|customer-id|trm-passenger-resource-id";

        [Fact]
        public void SavePassportRequest_MapsCheckInPassengerIdNinsToSsciRequest()
        {
            var request = new SavePassportRequest
            {
                TravelPartyId = "travel-party-id",
                PassengerId = CheckInPassengerId,
                Document = CreateDocument()
            };

            var result = (DeltaSsci.AddPassportRequest)request;

            Assert.Equal("01", result.FirstNIN);
            Assert.Equal("02", result.LastNIN);
            Assert.Equal("02.01", result.PassengerNINs);
        }

        [Fact]
        public void SelectPassengerRequest_MapsCheckInPassengerIdNinsToSsciRequest()
        {
            var request = new SelectPassengerRequest
            {
                SelectedPassengerIds = [CheckInPassengerId]
            };

            var result = (DeltaSsci.SelectPassengerRequest)request;
            var passenger = Assert.Single(result.SelectedPassenger);

            Assert.Equal("01", passenger.FirstNIN);
            Assert.Equal("02", passenger.LastNIN);
        }

        [Fact]
        public void SelectPassengerRequest_AllowsEmptyTrmPassengerResourceIdForBoardingPassReselection()
        {
            var request = new SelectPassengerRequest
            {
                SelectablePassengers = [("01.02|ssci-passenger-id|customer-id|", false)]
            };

            var result = (DeltaSsci.SelectPassengerRequest)request;
            var passenger = Assert.Single(result.SelectedPassenger);

            Assert.Equal("01", passenger.FirstNIN);
            Assert.Equal("02", passenger.LastNIN);
        }

        [Fact]
        public void PassengerId_ThrowsValidationException_WhenCheckInIdIsMalformed()
        {
            var exception = Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(
                () => new VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId("01.02|ssci-passenger-id"));

            Assert.Equal(
                "PassengerId must be in the expected format '<firstNin>.<lastNin>|<ssciPassengerId>|<customerId>|<trmPassengerResourceId>'.",
                exception.Message);
        }

        [Fact]
        public void SavePassportRequest_MapsCheckInPassengerIdToTrmPassengerResource()
        {
            var request = new SavePassportRequest
            {
                PassengerId = CheckInPassengerId,
                Document = CreateDocument()
            };

            var result = (DeltaTrm.RequirementFormRequest)request;

            Assert.Equal("trm-passenger-resource-id", result.PassengerResourceId);
        }

        [Fact]
        public void CheckInSaveReturnDateRequest_MapsCheckInPassengerIdToTrmPassengerResource()
        {
            var request = new ReturnDate.ReturnDateRequest
            {
                PassengerId = CheckInPassengerId,
                ReturnDate = new DateTime(2030, 1, 1)
            };

            var result = (DeltaTrm.RequirementFormRequest)request;

            Assert.Equal("trm-passenger-resource-id", result.PassengerResourceId);
        }

        private static Document CreateDocument() => new()
        {
            GivenNames = "Test",
            Surname = "Passenger",
            DateOfBirth = new LocalDate(1990, 1, 1),
            Gender = new Gender { Code = "M" },
            DocumentNumber = "123456789",
            ExpirationDate = new LocalDate(2030, 1, 1),
            IssuingCountry = new PassportCountry { Code = "GBR" },
            Nationality = new Nationality { Code = "GBR" },
            CountryOfResidence = new PassportCountry { Code = "GBR" }
        };
    }
}
