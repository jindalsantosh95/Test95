using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class CustomerDetailsMapperTests
    {
        [Fact]
        public void Test_SinglePassenger()
        {
            //Arrange

            var request = new RetrieveResponseType()
            {
                TeardownContextResponse = new TeardownContextResponseType()
                {
                    Pnr = new PnrType()
                    {
                        OtherServiceInformation = new OtherServiceInformation[1] { new OtherServiceInformation() { FreeformText = "dumy/dummyemail@dummy.com" } },
                        Phone = new Phone[1] { new Phone() { FreeformText = "1234567890" } },
                        Ptc = new Ptc[1] { new Ptc() { PassengerTypeCode = "A" } }
                    }
                }
            };
            var passengers = new List<Passenger>()
            {
                new Passenger()
                {
                    CustomerId = "dummy",
                    FirstName = "dummy",
                    LastName = "dummy",
                    FirstNameNumber ="dummy",
                    LastNameNumber ="dummy"
                }
            };
            var customers = TestDataProvider.GetTestData<List<EAPI.Domain.Virgin.Models.Customer.Customer>>("OrderSearchMS\\FlightCustomer");

            //Act

            var result = CustomerDetailsMapper.Air4ToOrderSearchResponseMapper(request, passengers, customers);

            //Assert

            Assert.Single(result);
            Assert.Equal("dummyemail@dummy.com", result.First().Contacts.Email);
        }

        [Fact]
        public void Test_MultiplePassengers()
        {
            //Arrange

            var request = new RetrieveResponseType()
            {
                TeardownContextResponse = new TeardownContextResponseType()
                {
                    Pnr = new PnrType()
                    {
                        OtherServiceInformation = new OtherServiceInformation[1] { new OtherServiceInformation() { FreeformText = "dumy/dummyemail@dummy.com" } },
                        Phone = new Phone[1] { new Phone() { FreeformText = "1234567890" } },
                        Ptc = new Ptc[3] { new Ptc() { PassengerTypeCode = "A" }, new Ptc() { PassengerTypeCode = "Y" }, new Ptc() { PassengerTypeCode = "C" } }
                    }
                }
            };
            var passengers = new List<Passenger>()
            {
                new Passenger()
                {
                    CustomerId = "dummy1",
                    FirstName = "dummy1",
                    LastName = "dummy1",
                    FirstNameNumber ="dummy1",
                    LastNameNumber ="dummy1"
                },
                new Passenger()
                {
                    CustomerId = "dummy2",
                    FirstName = "dummy2",
                    LastName = "dummy2",
                    FirstNameNumber ="dummy2",
                    LastNameNumber ="dummy2"
                },
                new Passenger()
                {
                    CustomerId = "dummy3",
                    FirstName = "dummy3",
                    LastName = "dummy3",
                    FirstNameNumber ="dummy3",
                    LastNameNumber ="dummy3"
                }
            };
            var customers = TestDataProvider.GetTestData<List<EAPI.Domain.Virgin.Models.Customer.Customer>>("OrderSearchMS\\FlightCustomer");

            //Act

            var result = CustomerDetailsMapper.Air4ToOrderSearchResponseMapper(request, passengers, customers);

            //Assert

            Assert.Equal(3, result.Count);
        }


        [Fact]
        public void Mapper_ShouldReturnAssociatedAdultId_ForInfantPtc()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS//Air4PnrRetrieveResponse");
            request.TeardownContextResponse.Pnr.Passenger.Last().FirstName = "First Name4";

            //Act
            var result = CustomerDetailsMapper.Air4ToOrderSearchResponseMapper(request, request.TeardownContextResponse.Pnr.Passenger, null);

            //Assert
            Assert.Equal("dummy1", result.First(x => x.PTC == "INF").AssociatedWithAdult);
        }

        [Fact]
        public void Air4ToOrderSearchResponseMapper_ShouldGet_ValidPhoneNumberWithoutCountryCode()
        {

            //Arrange            
            var request = new RetrieveResponseType()
            {
                TeardownContextResponse = new TeardownContextResponseType()
                {
                    Pnr = new PnrType()
                    {
                        OtherServiceInformation = new OtherServiceInformation[1] { new OtherServiceInformation() { FreeformText = "dumy/dummyemail@dummy.com" } },
                        Phone = new Phone[1] { new Phone() { FreeformText = "123456**HOME/44**" } },
                        Ptc = new Ptc[3] { new Ptc() { PassengerTypeCode = "A" }, new Ptc() { PassengerTypeCode = "Y" }, new Ptc() { PassengerTypeCode = "C" } }
                    }
                }
            };
            var passengers = new List<Passenger>()
            {
                new Passenger()
                {
                    CustomerId = "Cust_1",
                    FirstName = "David",
                    LastName = "Smith",
                    FirstNameNumber ="dummy",
                    LastNameNumber ="dummy"
                }
            };
            var customers = TestDataProvider.GetTestData<List<EAPI.Domain.Virgin.Models.Customer.Customer>>("OrderSearchMS\\FlightCustomer");
            string expectedPhoneNumber = "123456";

            //Act           
            var result = CustomerDetailsMapper.Air4ToOrderSearchResponseMapper(request, passengers, customers);

            //Assert
            Assert.Equal(result.First().Contacts.Phone, expectedPhoneNumber);
        }
        [Fact]
        public void Air4ToOrderSearchResponseMapper_WithoutSlashInFreeFromText_ShouldGet_ValidPhoneNumber()
        {

            //Arrange            
            var request = new RetrieveResponseType()
            {
                TeardownContextResponse = new TeardownContextResponseType()
                {
                    Pnr = new PnrType()
                    {
                        OtherServiceInformation = new OtherServiceInformation[1] { new OtherServiceInformation() { FreeformText = "dumy/dummyemail@dummy.com" } },
                        Phone = new Phone[1] { new Phone() { FreeformText = "123456**HOME**" } },
                        Ptc = new Ptc[3] { new Ptc() { PassengerTypeCode = "A" }, new Ptc() { PassengerTypeCode = "Y" }, new Ptc() { PassengerTypeCode = "C" } }
                    }
                }
            };
            var passengers = new List<Passenger>()
            {
                new Passenger()
                {
                    CustomerId = "Cust_1",
                    FirstName = "David",
                    LastName = "Smith",
                    FirstNameNumber ="dummy"
                }
            };
            var customers = TestDataProvider.GetTestData<List<EAPI.Domain.Virgin.Models.Customer.Customer>>("OrderSearchMS\\FlightCustomer");
            string expectedPhoneNumber = "123456";

            //Act           
            var result = CustomerDetailsMapper.Air4ToOrderSearchResponseMapper(request, passengers, customers);

            //Assert
            Assert.Equal(result.First().Contacts.Phone, expectedPhoneNumber);
        }

    }
}
