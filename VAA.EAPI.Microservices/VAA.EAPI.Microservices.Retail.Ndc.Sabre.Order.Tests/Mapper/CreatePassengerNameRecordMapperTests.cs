using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests.Mapper
{
    public class CreatePassengerNameRecordMapperTests
    {
        [Fact]
        public void CreatePnrReqMapper_ShouldMapAdvancePassengerInfo_WhenValidInputIsProvided()
        {
            // Arrange
            var segments = new List<Segment> { new() { OperatingFlightNumber = "25", MarketingFlightNumber = "647", MarketingCarrier = "VS" , ScheduledDepartureTime = new() { LocalDateTime = "2025-02-20T10:00"} , ScheduledArrivalTime = new() { LocalDateTime = "2025-02-20T13:00"} } };

            var travelDocuments = new List<TravelDocument> { new() { DocumentNumber = "PQP456S", DocumentType = "Passport", ExpirationDate = "2030-05-26", CountryOfIssue = "US", Nationality = "FR", CountryOfResidence = "US", RegionOfIssuance = "US", IssueDate = "2023-10-11", PersonName = new PersonName { FirstName = "John", LastName = "Doe" } } , new() { DocumentNumber = "USA12345", DocumentType = "VISA", ExpirationDate = "2030-05-26", CountryOfIssue = "US", Nationality = "FR", CountryOfResidence = "FR", RegionOfIssuance = "US", IssueDate = "2023-10-11", PersonName = new PersonName { FirstName = "John", LastName = "Doe" } } };

            var customers = new List<Customer> { new() { TravelDocuments = travelDocuments, FirstName = "John", LastName = "Doe", FirstNameNumber = "01", LastNameNumber = "02", Contacts = new() { Phone = "1234567890", Email = "john.doe@example.com" }, Addresses = new Address { AddressType = "Resident", AddressLine1 = "123 1ST AVE", Country = "US", City = "NEW YORK", Region = "NY", PostalCode = "76092" }, DateOfBirth = "1980-12-02", Ptc = "ADT", Gender = "M" } };

            var orderInstructions = new List<OrderInstructions> { new() { ItemCode = "SSR1", ItemText = "Request" } };

            var parties = new Parties { CorporateAccount = new() { CorporateCode = "CC006" }, Participants = new List<Participant>() { new Participant() { AgencyID = "123456", PartyType = Participant.PartyTypeEnum.Seller, Contacts = new Contacts() { Email = "abc@testmail.com" } } } };

            // Act
            var result = CreatePassengerNameRecordRQMapper.CreatePnrReqMapper(new Domain.OrderCreateRequest() { DataList = new() { SegmentDetails = segments , CustomerDetails = customers , OrderInstructions = orderInstructions }, Parties = parties });
            var advancePassengerInfo = result.SpecialReqDetails.SpecialService.SpecialServiceInfo.AdvancePassenger;

            // Assert
            Assert.NotNull(result);

            foreach (var info in advancePassengerInfo)
            {
                if (info.Document?.Type == "P")
                {
                    Assert.Equal("PQP456S", info.Document.Number);
                    Assert.Equal("US", info.Document.IssueCountry);
                    Assert.Equal("2030-05-26", info.Document.ExpirationDate.ToString("yyyy-MM-dd"));
                    Assert.Equal("FR", info.Document.NationalityCountry);
                    Assert.Equal("John", info.PersonName.GivenName);
                    Assert.Equal("Doe", info.PersonName.Surname);
                    Assert.Equal("02.01", info.PersonName.NameNumber);
                }
                if(info.Document?.Type == "V")
                {
                    Assert.Equal("USA12345", info.Document.Number);
                    Assert.Equal("US", info.Document.Visa.ApplicableCountry);
                    Assert.Equal("2030-05-26", info.Document.Visa.ExpirationDate.ToString("yyyy-MM-dd"));
                    Assert.Equal("FR", info.Document.Visa.PlaceOfBirth);
                    Assert.Equal("US", info.Document.Visa.PlaceOfIssue);
                }
            }
        }

        [Fact]
        public void CreatePnrReqMapper_ShouldMapResidentDestinationAddress_WhenValidInputIsProvided()
        {
            // Arrange
            var segments = new List<Segment> { new() { OperatingFlightNumber = "25", MarketingFlightNumber = "647", MarketingCarrier = "VS", ScheduledDepartureTime = new() { LocalDateTime = "2025-02-20T10:00" }, ScheduledArrivalTime = new() { LocalDateTime = "2025-02-20T13:00" } } };

            var travelDocuments = new List<TravelDocument> { new() { DocumentNumber = "PQP456S", DocumentType = "Passport", ExpirationDate = "2030-05-26", CountryOfIssue = "US", Nationality = "US", CountryOfResidence = "US", RegionOfIssuance = "US", IssueDate = "2023-10-11", PersonName = new PersonName { FirstName = "John", LastName = "Doe" } } };

            var customers = new List<Customer> { new() { TravelDocuments = travelDocuments,FirstName = "John", LastName = "Doe", FirstNameNumber = "01", LastNameNumber = "02", Contacts = new() { Phone = "1234567890", Email = "john.doe@example.com" }, Addresses = new Address { AddressType = "Resident", AddressLine1 = "123 1ST AVE", Country = "US", City = "NEW YORK", Region = "NY", PostalCode = "76092" }, DateOfBirth = "1980-12-02", Ptc = "ADT", Gender = "M" } };

            var orderInstructions = new List<OrderInstructions> { new() { ItemCode = "SSR1", ItemText = "Request" } };

            var parties = new Parties { CorporateAccount = new() { CorporateCode = "CC006" }, Participants = new List<Participant>() { new Participant() { AgencyID = "123456", PartyType = Participant.PartyTypeEnum.Seller, Contacts = new Contacts() { Email = "abc@testmail.com" } } } };

            // Act
            var result = CreatePassengerNameRecordRQMapper.CreatePnrReqMapper(new Domain.OrderCreateRequest() { DataList = new() { SegmentDetails = segments, CustomerDetails = customers, OrderInstructions = orderInstructions }, Parties = parties }); 
            var advancePassengerInfo = result.SpecialReqDetails.SpecialService.SpecialServiceInfo.AdvancePassenger;
            var residentDestinationAddress = advancePassengerInfo.First(x => x.ResidentDestinationAddress != null).ResidentDestinationAddress;

            // Assert
            Assert.Equal("US", residentDestinationAddress.Country);
            Assert.Equal("76092", residentDestinationAddress.Zip);
            Assert.Equal("NEW YORK", residentDestinationAddress.City);
            Assert.Equal("D", residentDestinationAddress.Type);
        }

        [Fact]
        public void CreatePnrReqMapper_ShouldMapSecureFlightInfo_WhenValidInputIsProvided()
        {
            // Arrange
            var segments = new List<Segment> { new() { OperatingFlightNumber = "25", MarketingFlightNumber = "647", MarketingCarrier = "VS", ScheduledDepartureTime = new() { LocalDateTime = "2025-02-20T10:00" }, ScheduledArrivalTime = new() { LocalDateTime = "2025-02-20T13:00" } } };

            var travelDocuments = new List<TravelDocument> { new() { DocumentNumber = "PQP456S", DocumentType = "AEA", ExpirationDate = "2030-05-26", CountryOfIssue = "US", Nationality = "US", CountryOfResidence = "US", RegionOfIssuance = "US", IssueDate = "2023-10-11", PersonName = new PersonName { FirstName = "John", LastName = "Doe" } } };

            var customers = new List<Customer> { new() { TravelDocuments = travelDocuments ,FirstName = "John", LastName = "Doe", FirstNameNumber = "01", LastNameNumber = "02", Contacts = new() { Phone = "1234567890", Email = "john.doe@example.com" }, Addresses = new Address { AddressType = "Resident", AddressLine1 = "123 1ST AVE", Country = "US", City = "NEW YORK", Region = "NY", PostalCode = "76092" }, DateOfBirth = "1980-12-02", Ptc = "ADT" , Gender = "M" } };

            var orderInstructions = new List<OrderInstructions> { new() { ItemCode = "SSR1", ItemText = "Request" } };

            var parties = new Parties { CorporateAccount = new() { CorporateCode = "CC006" }, Participants = new List<Participant>() { new Participant() { AgencyID = "123456", PartyType = Participant.PartyTypeEnum.Seller, Contacts = new Contacts() { Email = "abc@testmail.com" } } } };

            // Act
            var result = CreatePassengerNameRecordRQMapper.CreatePnrReqMapper(new Domain.OrderCreateRequest() { DataList = new() { SegmentDetails = segments, CustomerDetails = customers, OrderInstructions = orderInstructions }, Parties = parties });
            var secureFlightInfo = result.SpecialReqDetails.SpecialService.SpecialServiceInfo.SecureFlight;

            Assert.NotNull(secureFlightInfo);
            Assert.Equal("US", secureFlightInfo.First().IssueCountry);
            Assert.Equal("02.01", secureFlightInfo.First().PersonName.NameNumber);

            if (travelDocuments.FirstOrDefault()?.DocumentType == Constants.Application.RedressDocType || travelDocuments.FirstOrDefault()?.DocumentType == Constants.Application.AEADocType)
            {
                Assert.Equal("PQP456S", secureFlightInfo.First().RedressNumber);
            }
            else
            {
                Assert.Equal("PQP456S", secureFlightInfo.First().KnownTravelerNumber);
            }

        }

        [Fact]
        public void CreatePnrReqMapper_ShouldMapSSRGSTInfo()
        {
            var OrderCreateRequest = TestDataProvider.GetTestData<Domain.OrderCreateRequest>("NDCOrderMS\\OrderCreateRequest");

            // Act
            var result = CreatePassengerNameRecordRQMapper.CreatePnrReqMapper(OrderCreateRequest);
            var GSTInfo = result.SpecialReqDetails.SpecialService.SpecialServiceInfo.Service;

            Assert.Contains(GSTInfo, x => x.SSR_Code == "GSTN");
            Assert.Contains(GSTInfo, x => x.SSR_Code == "GSTA");
            Assert.Contains(GSTInfo, x => x.SSR_Code == "GSTP");
            Assert.Contains(GSTInfo, x => x.SSR_Code == "GSTE");

        }

        [Fact]
        public void CreatePnrReqMapper_ShouldMapCtcrForRefusedPassengerAndContactSsrsForOtherPassenger()
        {
            var segments = new List<Segment>
            {
                new() { OperatingFlightNumber = "103", MarketingFlightNumber = "103", MarketingCarrier = "VS", ClassOfService = "T", ScheduledDepartureTime = new() { LocalDateTime = "2027-01-22T10:50" }, ScheduledArrivalTime = new() { LocalDateTime = "2027-01-22T15:15" } }
            };
            var customers = new List<Customer>
            {
                new() { FirstName = "David", LastName = "Smith", FirstNameNumber = "01", LastNameNumber = "01", Ptc = "ADT", Contacts = new() { ContactRefusedInd = true } },
                new() { FirstName = "Chris", LastName = "Taylor", FirstNameNumber = "01", LastNameNumber = "02", Ptc = "CHD", DateOfBirth = "2020-01-22", Contacts = new() { Email = "chris.taylor@example.com", Phone = "1234567890", CountryDailingCode = "44", PhoneType = "H" } },
                new() { FirstName = "Alex", LastName = "Jones", FirstNameNumber = "01", LastNameNumber = "03", Ptc = "ADT", Contacts = new() { Email = "alex.jones@example.com", Phone = "1234567890", CountryDailingCode = "44", PhoneType = "H", ContactRefusedInd = true } }
            };
            var parties = new Parties
            {
                Participants = new List<Participant>
                {
                    new() { AgencyID = "91281035", PartyType = Participant.PartyTypeEnum.Seller, Contacts = new() { Email = "agent@example.com" } }
                }
            };

            var result = CreatePassengerNameRecordRQMapper.CreatePnrReqMapper(new Domain.OrderCreateRequest
            {
                DataList = new() { SegmentDetails = segments, CustomerDetails = customers },
                Parties = parties
            });
            var services = result.SpecialReqDetails.SpecialService.SpecialServiceInfo.Service;

            Assert.Contains(services, x => x.SSR_Code == "CTCR" && x.PersonName.NameNumber == "01.01");
            Assert.Contains(services, x => x.SSR_Code == "CTCE" && x.PersonName.NameNumber == "03.01");
            Assert.Contains(services, x => x.SSR_Code == "CTCM" && x.PersonName.NameNumber == "03.01");
            Assert.DoesNotContain(services, x => x.SSR_Code == "CTCR" && x.PersonName.NameNumber == "03.01");
            Assert.Equal("03.01", result.TravelItineraryAddInfo.CustomerInfo.Email.Single().NameNumber);
            Assert.Equal("03.01", result.TravelItineraryAddInfo.CustomerInfo.ContactNumbers.Single().NameNumber);
        }

        [Fact]
        public void CreatePnrReqMapper_ShouldPreserveLegacyPrimaryContactSelection_WhenNoPassengerRefusesContact()
        {
            var segments = new List<Segment>
            {
                new() { OperatingFlightNumber = "103", MarketingFlightNumber = "103", MarketingCarrier = "VS", ClassOfService = "T", ScheduledDepartureTime = new() { LocalDateTime = "2027-01-22T10:50" }, ScheduledArrivalTime = new() { LocalDateTime = "2027-01-22T15:15" } }
            };
            var customers = new List<Customer>
            {
                new() { FirstName = "Chris", LastName = "Taylor", FirstNameNumber = "01", LastNameNumber = "01", Ptc = "CHD", DateOfBirth = "2020-01-22", Contacts = new() { Email = "chris.taylor@example.com", Phone = "1234567890", CountryDailingCode = "44", PhoneType = "H" } },
                new() { FirstName = "Alex", LastName = "Jones", FirstNameNumber = "01", LastNameNumber = "02", Ptc = "ADT", Contacts = new() { Email = "alex.jones@example.com", Phone = "1234567890", CountryDailingCode = "44", PhoneType = "H" } }
            };
            var parties = new Parties
            {
                Participants = new List<Participant>
                {
                    new() { AgencyID = "91281035", PartyType = Participant.PartyTypeEnum.Seller, Contacts = new() { Email = "agent@example.com" } }
                }
            };

            var result = CreatePassengerNameRecordRQMapper.CreatePnrReqMapper(new Domain.OrderCreateRequest
            {
                DataList = new() { SegmentDetails = segments, CustomerDetails = customers },
                Parties = parties
            });

            Assert.Equal("chris.taylor@example.com", result.TravelItineraryAddInfo.CustomerInfo.Email.Single().Address);
            Assert.Equal("1.1", result.TravelItineraryAddInfo.CustomerInfo.Email.Single().NameNumber);
            Assert.Equal("1.1", result.TravelItineraryAddInfo.CustomerInfo.ContactNumbers.Single().NameNumber);
        }
                
    }
}
