using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Airport;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Mapper
{
    public class PaymentMSRequestMapperTests
    {
        public PaymentMSRequestMapperTests() { }

        [Fact]
        public void Mapper_ShouldReturnValid_PaymentMSAuthorizeRequest()
        {
            //Arrange
            var orderRS = new Order.Infrastructure.Model.OrderMS.OrderRS()
            {
                OrderDocument = new()
                {
                    PointOfSale = new EAPI.Domain.Virgin.Models.Common.Pos()
                    {
                        CountryCode = "GB",
                        CurrencyCode = "GBP"
                    },
                    Parties = new Order.Infrastructure.Model.Parties()
                    {
                        Participants = [new()
                        {
                            PartyType = Order.Infrastructure.Model.Participant.PartyTypeEnum.Seller,
                            AgencyID = "91281035",
                            Ordinal = "1",
                            Contacts = new()
                            {
                                Email = "agentemail@gmail.com"
                            }
                        }]
                    },
                    BookingReference = new EAPI.Domain.Virgin.Models.Order.BookingReference()
                    {
                        RLOC = "ABCDEF"
                    }
                }
            };

            var currencies = new List<Currency>()
            {
                new Currency()
                {
                    CurrencyCode = "GBP",
                    CurrencyNumber = "123"
                },
                new Currency()
                {
                    CurrencyCode = "INR",
                    CurrencyNumber = "567"
                }
            };

            var airportDetails = new List<AirportDetail>()
            {
                new AirportDetail()
                {
                    CountryCode = "GB",
                    AirportCode = "LHR"
                },
                new AirportDetail()
                {
                    CountryCode = "GB",
                    AirportCode = "MAN"
                }
            };

            var iata_orderCreateRq = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");

            var offerDoc = TestDataProvider.GetTestData<Order.Infrastructure.Model.OfferRS.OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE");

            //Act
            var authorizeRequest = PaymentMSRequestMapper.MapOrderCreateAuthorizeRequest(orderRS, iata_orderCreateRq, offerDoc, currencies , airportDetails,"2691.09", Constants.ProductType.Ticket);

            //Assert
            Assert.NotNull(authorizeRequest);
            Assert.NotNull(authorizeRequest.Pos);
            Assert.NotNull(authorizeRequest.Parties);
            Assert.NotNull(authorizeRequest.Parties.Participants.First().Contacts.Email);
            Assert.NotNull(authorizeRequest.BookingReferences);
            Assert.Equal("GB", authorizeRequest.Pos.CountryCode);
            Assert.Equal("GBP", authorizeRequest.Pos.CurrencyCode);
            Assert.Contains("ABCDEF", authorizeRequest.OrderId);
            Assert.StartsWith("VS932", authorizeRequest.OrderId);
            Assert.Equal(100, (int)authorizeRequest.ProductType);
            Assert.Equal("RETURN", authorizeRequest.JourneyType);
            Assert.Equal("2691.09", authorizeRequest.PaymentDetails.First().Amount.MonetaryAmount);
            Assert.Equal("123", authorizeRequest.PaymentDetails.First().Amount.CurrencyId);
            Assert.Equal("David", authorizeRequest.PaymentDetails.First().Payer.FirstName);
            Assert.Equal("RH108HA", authorizeRequest.PaymentDetails.First().Payer.PayerAddress.PostalCode);
            Assert.Equal("David Smith", authorizeRequest.PaymentDetails.First().CardMethod.CardholderName);
            Assert.NotNull(authorizeRequest.CustomerDetails);
            Assert.NotEmpty(authorizeRequest.CustomerDetails);
            Assert.False(string.IsNullOrWhiteSpace(authorizeRequest.CustomerDetails.First().DateOfBirth));
            Assert.Equal("1946-08-05", authorizeRequest.CustomerDetails.First().DateOfBirth);
        }

        [Fact]
        public void Mapper_ShouldUseNextAdultContactWhenFirstAdultRefusesContact()
        {
            var orderRS = new Order.Infrastructure.Model.OrderMS.OrderRS()
            {
                OrderDocument = new()
                {
                    PointOfSale = new EAPI.Domain.Virgin.Models.Common.Pos()
                    {
                        CountryCode = "GB",
                        CurrencyCode = "GBP"
                    },
                    Parties = new Order.Infrastructure.Model.Parties(),
                    BookingReference = new EAPI.Domain.Virgin.Models.Order.BookingReference()
                    {
                        RLOC = "ABCDEF"
                    }
                }
            };
            var currencies = new List<Currency>()
            {
                new()
                {
                    CurrencyCode = "GBP",
                    CurrencyNumber = "123"
                },
                new()
                {
                    CurrencyCode = "INR",
                    CurrencyNumber = "567"
                }
            };
            var airportDetails = new List<AirportDetail>()
            {
                new()
                {
                    CountryCode = "GB",
                    AirportCode = "LHR"
                }
            };
            var orderCreateRequest = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ_Return_2ADT1CHD1GBE1INF");
            var adultPax = orderCreateRequest.Request.DataLists.PaxList.Where(pax => pax.PTC == "ADT").ToArray();
            var refusedContact = orderCreateRequest.Request.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == adultPax[0].ContactInfoRefID);
            var availableContact = orderCreateRequest.Request.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == adultPax[1].ContactInfoRefID);
            var offerDoc = TestDataProvider.GetTestData<Order.Infrastructure.Model.OfferRS.OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE");

            refusedContact.ContactRefusedInd = true;
            refusedContact.EmailAddress = null;
            refusedContact.Phone = null;
            VAA.EAPI.Common.Helpers.ConfigReader.Set(Constants.Environment.TicketStockNumber, "932");

            var authorizeRequest = PaymentMSRequestMapper.MapOrderCreateAuthorizeRequest(orderRS, orderCreateRequest, offerDoc, currencies, airportDetails, "2691.09", Constants.ProductType.Ticket);
            var ifgAuthorizeRequest = PaymentMSRequestMapper.MapOrderCreateCCIFGAuthorizeRequest(orderRS, orderCreateRequest, offerDoc, currencies, airportDetails, "2691.09", Constants.ProductType.Ticket);

            Assert.Equal(availableContact.EmailAddress.First().EmailAddressText, authorizeRequest.PaymentDetails.First().Payer.PayerContacts.Email);
            Assert.Equal(availableContact.Phone.First().PhoneNumber, authorizeRequest.PaymentDetails.First().Payer.PayerContacts.Phone);
            Assert.Equal(availableContact.Phone.First().CountryDialingCode?.Replace("+", ""), authorizeRequest.PaymentDetails.First().Payer.PayerContacts.CountryDailingCode);
            Assert.Equal(availableContact.EmailAddress.First().EmailAddressText, ifgAuthorizeRequest.PaymentDetails.First().Payer.PayerContacts.Email);
            Assert.Equal(availableContact.Phone.First().PhoneNumber, ifgAuthorizeRequest.PaymentDetails.First().Payer.PayerContacts.Phone);
            Assert.Equal(availableContact.Phone.First().CountryDialingCode?.Replace("+", ""), ifgAuthorizeRequest.PaymentDetails.First().Payer.PayerContacts.CountryDailingCode);
        }

        [Fact]
        public void Mapper_ShouldReturnValid_PaymentMSCaptureRequest()
        {
            //Arrange
            var orderRS = new Order.Infrastructure.Model.OrderMS.OrderRS()
            {
                OrderDocument = new()
                {
                    PointOfSale = new EAPI.Domain.Virgin.Models.Common.Pos()
                    {
                        CountryCode = "GB",
                        CurrencyCode = "GBP"
                    },
                    Parties = new Order.Infrastructure.Model.Parties(),
                    BookingReference = new EAPI.Domain.Virgin.Models.Order.BookingReference()
                    {
                        RLOC = "ABCDEF"
                    },
                    OrderItems = [new() {
                        OrderItemId = "1",
                        AccountableDocumentRefs = [new(){
                            DocumentNumber = "9321234567",
                        }],
                        Services = [new(){
                            ServiceBeneficiaries = ["ADULT_1"],
                            SegmentRefIds = ["SEG1"]
                        }]
                    }]
                }
            };

            var currencies = new List<Currency>()
            {
                new Currency()
                {
                    CurrencyCode = "INR",
                    CurrencyNumber = "567"
                },
                new Currency()
                {
                    CurrencyCode = "GBP",
                    CurrencyNumber = "123"
                }
            };

            var airportDetails = new List<AirportDetail>()
            {
                new AirportDetail()
                {
                    CountryCode = "GB",
                    AirportCode = "LHR"
                },
                new AirportDetail()
                {
                    CountryCode = "GB",
                    AirportCode = "MAN"
                }
            };

            var orderTicket = new OrderTicketDocument()
            {
                AccountableDocumentRef =
                [
                    new AccountableDocumentRef()
                    {
                        DocumentNumber = "9321234567"
                    }
                ]
            };

            var iata_orderCreateRq = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");

            var offerDoc = TestDataProvider.GetTestData<Order.Infrastructure.Model.OfferRS.OfferResponse>("NDCOrder\\OrderCreate\\OfferDoc_Return_2ADT1CHD1GBE");

            var authorizeRequest = PaymentMSRequestMapper.MapOrderCreateAuthorizeRequest(orderRS, iata_orderCreateRq, offerDoc, currencies, airportDetails , "2691.09",Constants.ProductType.Ticket);

            var paymentRS = new Order.Infrastructure.Model.PaymentMS.PaymentRS()
            {
                PaymentDocument = new Order.Infrastructure.Model.PaymentMS.PaymentDocument()
                {
                    TransactionId = "123456789"
                }
            };

            //Act
            var captureRequest = PaymentMSRequestMapper.MapOrderCreateCaptureRequest(authorizeRequest, [new AccountableDocumentRef()], paymentRS, offerDoc, Constants.ProductType.Ticket, new EAPI.Domain.Virgin.Models.Order.BookingReference() , orderRS.OrderDocument);

            //Assert
            Assert.NotNull(captureRequest);
            Assert.NotNull(captureRequest.Pos);
            Assert.NotNull(captureRequest.Parties);
            Assert.NotNull(captureRequest.BookingReferences);
            Assert.Equal("GB", captureRequest.Pos.CountryCode);
            Assert.Equal("GBP", captureRequest.Pos.CurrencyCode);
            Assert.Contains("ABCDEF", captureRequest.OrderId);
            Assert.StartsWith("VS932", captureRequest.OrderId);
            Assert.Equal(100, (int)captureRequest.ProductType);
            Assert.Equal("RETURN", captureRequest.JourneyType);
            Assert.Equal("2691.09", captureRequest.PaymentDetails.First().Amount.MonetaryAmount);
            Assert.NotNull(captureRequest.AccountableDocumentRefs);
            Assert.Equal("123456789", captureRequest.PaymentDetails.First().PaymentTransactionId);
            Assert.Equal("123", captureRequest.PaymentDetails.First().Amount.CurrencyId);
            Assert.Equal("David Smith", captureRequest.PaymentDetails.First().CardMethod.CardholderName);
        }
    }
}
