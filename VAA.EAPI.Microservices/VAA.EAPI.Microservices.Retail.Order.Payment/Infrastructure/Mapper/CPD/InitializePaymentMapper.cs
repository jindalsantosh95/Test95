using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD
{
    public class InitializePaymentMapper
    {
        public static InitializePaymentRequest InitializePaymentRequestMapper(Domain.CPD.AuthorizePaymentRequest request, IEnumerable<CountryId> countryList)
        {
            var posCountryId = countryList.First(x => x.CountryCode == request.Pos.CountryCode).Id;
            int amount = (int)(request.PaymentDetails.First().Amount.MonetaryAmount.Parse<decimal>() * 100);
            var mobileCountryId = countryList.FirstOrDefault(x => x.DialingCode == request.PaymentDetails.First().Payer.PayerContacts.CountryDailingCode)?.Id;
            string mobile = mobileCountryId != null ? request.PaymentDetails.FirstOrDefault()?.Payer?.PayerContacts?.Phone : null;
            string hmacinput = ConfigReader.Get(Constants.Environment.CPDClientId) + request.OrderId + amount.ToString() + posCountryId + mobile + (string.IsNullOrEmpty(mobile) ? string.Empty : mobileCountryId) + request.PaymentDetails.FirstOrDefault()?.Payer?.PayerContacts?.Email + ConfigReader.Get(Constants.Environment.CPDSaltkey);
            var participantDetails = request.Parties.Participants.First(participant => participant.Ordinal == "1");

            bool usAgent = request.Pos.CountryCode == "US";
            bool isMoto = request.PaymentDetails.First().TransactionTypeId == "7";

            return new()
            {
                Initializepayment = new()
                {
                    ClientId = ConfigReader.Get(Constants.Environment.CPDClientId),
                    Account = PaymentAccountHelper.GetPaymentAccount(usAgent, isMoto),

                    Transaction = new()
                    {
                        Amount = new()
                        {
                            CountryId = posCountryId,
                            CurrencyId = request.PaymentDetails.First().Amount.CurrencyId,
                            Value = amount
                        },

                        TypeId = request.PaymentDetails.First().TransactionTypeId,
                        BookingRef = request.BookingReferences?.RLOC,
                        Hmac = HMACEncryption.Encrypt(hmacinput),
                        
                        AdditionalData = new()
                        {
                            Param = new List<Param>()
                            {
                                new()
                                {
                                    Name = "merchant_payment_ref",
                                    Text = request.BookingReferences?.RLOC
                                },
                                new()
                                {
                                    Name = "TravelAgencyCode",
                                    Text = participantDetails?.AgencyID
                                },
                                new()
                                {
                                    Name = "TravelAgencyName",
                                    Text = participantDetails?.Name
                                },
                                new()
                                {
                                    Name = "TravelAgencyEmail",
                                    Text = participantDetails?.Contacts?.Email
                                }
                            }
                        },
                        OrderNo = request.OrderId,

                        Orders = request.Segments != null ? new()
                        {
                            LineItem = new List<LineItem>()
                            {
                                new()
                                {
                                    Product = new()
                                    {
                                        Sku = request.BookingReferences?.RLOC,
                                        Type = new() { Text = ((int)request.ProductType).ToString() },
                                        Name = new() { Text = request.JourneyType },

                                        AirlineData = new()
                                        {
                                            Profiles = new()
                                            {
                                                Profile = request.CustomerDetails?.Select(x => new Profile
                                                {
                                                    Title = new() { Text = x.Title },
                                                    FirstName = new() { Text = x.FirstName },
                                                    LastName = new() { Text = x.LastName },
                                                    Type = new() { Text = x.Ptc },
                                                    ContactInfo = !string.IsNullOrEmpty(x.Contacts?.Email) ? new()
                                                    {                                                       
                                                        Email = new() { Text = x.Contacts?.Email }
                                                    } : null,
                                                    AdditionalData = new()
                                                    {
                                                        Param = new List<Param>()
                                                        {
                                                            new()
                                                            {
                                                                Name = "TravelAgencyCode",
                                                                Text = participantDetails?.AgencyID
                                                            },
                                                            new()
                                                            {
                                                                Name = "TravelAgencyName",
                                                                Text = participantDetails?.Name
                                                            },
                                                            new()
                                                            {
                                                                Name = "TravelAgencyEmail",
                                                                Text = participantDetails?.Contacts?.Email
                                                            },
                                                            new()
                                                            {
                                                                Name = "dob",
                                                                Text = x.DateOfBirth
                                                            }
                                                        }
                                                    },
                                                }).ToList(),
                                            },
                                            Trips = new()
                                            {
                                                Trip = request.Segments?.Select((x, index) => new Trip()
                                                {
                                                    Seq = "1",
                                                    Tag = (index + 1).ToString(),

                                                    Origin = new()
                                                    {
                                                        CountryId = countryList.FirstOrDefault(a => a.CountryCode == x.OriginCountryCode)?.Id,
                                                        ExternalId = x.OriginAirportCode,
                                                        TimeZone = ""
                                                    },

                                                    Destination = new()
                                                    {
                                                        CountryId = countryList.FirstOrDefault(a => a.CountryCode == x.DestinationCountryCode)?.Id,
                                                        ExternalId = x.DestinationAirportCode,
                                                        TimeZone = ""
                                                    },

                                                    ArrivalTime = new() { Text = x.ScheduledArrivalTime?.LocalDateTime },
                                                    DepartureTime = new() { Text = x.ScheduledDepartureTime?.LocalDateTime },

                                                    Transportation = new()
                                                    {
                                                        Code = x.MarketingCarrier,
                                                        Number = x.MarketingFlightNumber,
                                                        Carriers = new()
                                                        {
                                                            Carrier = new()
                                                            {
                                                                Code = x.OperatingCarrier,
                                                                Number = new() { Text = x.OperatingFlightNumber },
                                                            }
                                                        }
                                                    },

                                                    BookingClass = new()
                                                    {
                                                        Text = x.ClassOfService
                                                    },

                                                    AdditionalData = new()
                                                    {
                                                        Param = new List<Param>()
                                                        {
                                                            new()
                                                            {
                                                                Name = "fare_basis",
                                                                Text = x.FareBasis
                                                            }
                                                        }
                                                    },
                                                }).ToList(),
                                            }
                                        }
                                    }
                                }
                            }
                        } : null
                    },

                    Clientinfo = new()
                    {
                        Language = Constants.Application.Language,
                        SdkVersion = Constants.Application.Sdkversion,
                        Version = Constants.Application.Version,
                        Platform = Constants.Application.Platform,
                        Email = request.PaymentDetails.FirstOrDefault()?.Payer?.PayerContacts?.Email,

                        Mobile = !string.IsNullOrEmpty(mobile) ? new()
                        {
                            Text = request.PaymentDetails.FirstOrDefault()?.Payer?.PayerContacts?.Phone,
                            CountryId = mobileCountryId,
                        } : null
                    }
                }
            }; 


        }
    }
}
