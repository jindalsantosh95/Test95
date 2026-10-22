using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using static VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange.FlightItems;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class EnhancedAirBookRequestMapper
    {
        public static EnhancedAirBookRQRequest GetEnhancedAirBookRequest(string token, OrderChangeRequestData request, IEnumerable<Customer> customerDetails)
        {
            return new()
            {
                EnhancedAirBookRQ = GetEnhancedAirBookRQ(request, customerDetails),
                MessageHeader = new()
                {
                    Action = Constants.Application.EnhancedAirBookAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }
            };
        }

        private static EnhancedAirBookRQ GetEnhancedAirBookRQ(OrderChangeRequestData request, IEnumerable<Customer> customerDetails)
        {
            var segment = request.DataList.SegmentDetails.ToList();

            return new()
            {
                PostProcessing = new EnhancedAirBookRQPostProcessing()
                {
                    RedisplayReservation = new EnhancedAirBookRQPostProcessingRedisplayReservation()
                    {
                        WaitInterval = Constants.Application.WaitIntervalField
                    },
                    ARUNK_RQ = segment.Any(x => segment.Last() != x && x.DestinationAirportCode != segment.ElementAt(segment.FindIndex(y => y == x) + 1).OriginAirportCode) ? new() : null
                },
                version = Constants.Application.EnhancedAirBookRQVersion,
                HaltOnError = true,
                IgnoreOnError = true,
                OTA_AirBookRQ = new EnhancedAirBookRQOTA_AirBookRQ()
                {
                    HaltOnStatus = GetHaltOnStatusCodes(),
                    RetryRebook = new EnhancedAirBookRQOTA_AirBookRQRetryRebook()
                    {
                        Option = true,
                    },
                    OriginDestinationInformation = GetFlightSegment(request.OrderAmendments.First().FlightItems, customerDetails.Count(x => x.Ptc != Constants.Application.Infant))
                }
            };
        }

        private static EnhancedAirBookRQOTA_AirBookRQFlightSegment[] GetFlightSegment(IEnumerable<FlightItems> flightItems, int paxcount)
        {
            return flightItems.Where(x => x.ActionContext == ActionContexts.Add).SelectMany(x => x.Segments)
                              .Select(x => new EnhancedAirBookRQOTA_AirBookRQFlightSegment()
                              {
                                  ArrivalDateTime = x.ScheduledArrivalTime.LocalDateTime.ToString(),
                                  DepartureDateTime = x.ScheduledDepartureTime.LocalDateTime.ToString(),
                                  FlightNumber = x.MarketingFlightNumber,
                                  NumberInParty = Convert.ToString(paxcount),
                                  ResBookDesigCode = x.ClassOfService,
                                  Status = Constants.Application.Status,
                                  DestinationLocation = new()
                                  {
                                      LocationCode = x.DestinationAirportCode
                                  },
                                  MarketingAirline = new()
                                  {
                                      FlightNumber = x.MarketingFlightNumber,
                                      Code = x.MarketingCarrier
                                  },
                                  OriginLocation = new()
                                  {
                                      LocationCode = x.OriginAirportCode,
                                  }
                              }).ToArray();
        }

        private static EnhancedAirBookRQOTA_AirBookRQHaltOnStatus[] GetHaltOnStatusCodes()
        {
            var haltOnSatusCodes = new string[]
            {
                "HL", "HN", "HX", "LL", "NN", "NO" , "PN" , "UC", "UN", "US", "UU"
            };

            return haltOnSatusCodes.Select(code => new EnhancedAirBookRQOTA_AirBookRQHaltOnStatus()
            {
                Code = code,
            }).ToArray();
        }
    }
}