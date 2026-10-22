using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD
{
    public class CapturePaymentMapper
    {
        public static Infrastructure.Models.CPD.CapturePaymentRequest CapturePaymentRequest(Domain.CPD.CapturePaymentRequest request, IEnumerable<CountryId> countryList)
        {
            var participantDetails = request.Parties.Participants.First(participant => participant.Ordinal == "1");

            return new()
            {
                UpdateOrderData = new()
                {
                    Transaction = new()
                    {
                        Id = request.PaymentDetails.First().PaymentTransactionId,
                        OrderNo = request.OrderId,
                        Orders = new()
                        {
                            LineItem = [new LineItem()
                            {
                                Amount = new()
                                {
                                    Value = (int)(request.PaymentDetails.First().Amount.MonetaryAmount.Parse<decimal>() * 100)
                                },
                                Product = new()
                                {
                                    OrderRef = request.AccountableDocumentRefs.First().DocumentNumber,
                                    Sku = request.BookingReferences.RLOC,
                                    Type = request.Segments != null ? new() { Text = ((int)request.ProductType).ToString() } : null,
                                    Name = request.Segments != null ? new() { Text = request.JourneyType } : null,

                                    AirlineData = request.Segments != null ? new()
                                    {
                                        Profiles = new()
                                        {
                                            Profile = request.CustomerDetails.Select(x => new Profile
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
                                                         }
                                                     }
                                                }

                                            }).ToList(),
                                        },

                                        Trips = new()
                                        {
                                            Trip = request.Segments.Select((x, index) => new Trip()
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
                                                            Number = new()
                                                            {
                                                                Text = x.OperatingFlightNumber
                                                            },
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
                                    } : null
                                }
                            }]
                        }
                    }
                }
            };

        }
    }
}
